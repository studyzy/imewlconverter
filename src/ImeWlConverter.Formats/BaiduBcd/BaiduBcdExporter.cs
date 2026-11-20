namespace ImeWlConverter.Formats.BaiduBcd;

using System.Buffers.Binary;
using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// Baidu mobile bcd dictionary exporter (binary).
/// 只写常规词条区（0x350 起，纯中文/ASCII 混合），与手机百度 bcd 的固定头部布局一致。
/// </summary>
[FormatPlugin("bcd", "百度手机bcd", 1020, IsBinary = true, FileExtension = ".bcd")]
public sealed partial class BaiduBcdExporter : IFormatExporter
{
    private const int HeaderSize = 0x350;
    private const int NameOffset = 0x90;
    private const int AuthorOffset = 0xD0;
    private const int ExampleOffset = 0x110;
    private const int DescriptionOffset = 0x150;
    private const int NameFieldSize = 0x40;
    private const int AuthorFieldSize = 0x40;
    private const int ExampleFieldSize = 0x40;
    private const int DescriptionFieldSize = 0x200;
    private const int MaxNameChars = NameFieldSize / 2;
    private const int MaxAuthorChars = AuthorFieldSize / 2;
    private const int MaxExampleChars = ExampleFieldSize / 2;
    private const int MaxDescriptionChars = DescriptionFieldSize / 2;
    private const ushort DefaultFrequency = 300;

    private static readonly string[] Shengmu =
    [
        "c", "d", "b", "f", "g", "h", "ch", "j", "k", "l",
        "m", "n", "", "p", "q", "r", "s", "t", "sh", "zh",
        "w", "x", "y", "z"
    ];

    private static readonly string[] Yunmu =
    [
        "uang", "iang", "iong", "ang", "eng", "ian", "iao", "ing", "ong", "uai",
        "uan", "ai", "an", "ao", "ei", "en", "er", "ua", "ie", "in",
        "iu", "ou", "ia", "ue", "ui", "un", "uo", "a", "e", "i",
        "o", "u", "v"
    ];

    private static readonly IReadOnlyDictionary<string, (byte Shengmu, byte Yunmu)> SyllableIndex =
        BuildSyllableIndex();

    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries,
        Stream output,
        ExportOptions? options = null,
        CancellationToken ct = default)
    {
        using var payload = new MemoryStream();
        var sampleWords = new List<string>(5);
        var count = 0;
        var errorCount = 0;

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                if (TryWriteEntry(payload, entry))
                {
                    count++;
                    if (sampleWords.Count < 5)
                        sampleWords.Add(entry.Word);
                }
                else
                {
                    errorCount++;
                }
            }
            catch
            {
                errorCount++;
            }
        }

        WriteHeader(output, count, sampleWords, options);
        payload.Position = 0;
        payload.CopyTo(output);
        output.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = count,
            ErrorCount = errorCount
        });
    }

    /// <summary>写一条常规词条：u16 拼音长、u16 词频、拼音(声母+韵母索引)、UTF-16LE 词。</summary>
    private static bool TryWriteEntry(Stream output, WordEntry entry)
    {
        var word = entry.Word;
        if (string.IsNullOrEmpty(word) || word.Length > ushort.MaxValue)
            return false;

        var segments = entry.Code?.Segments;
        if (segments is null || segments.Count != word.Length)
            return false;

        // 常规区没有单独的 wordLen，词长等于 UTF-16 码元数；代理对会破坏 1:1 对应
        for (var i = 0; i < word.Length; i++)
        {
            if (char.IsSurrogate(word[i]))
                return false;
        }

        var pinyinBytes = new byte[word.Length * 2];
        for (var i = 0; i < word.Length; i++)
        {
            var ch = word[i];
            if (ch <= 0x7F)
            {
                // 0xFF 表示该字符按 ASCII 原样编码（英文/数字/标点）
                pinyinBytes[i * 2] = 0xFF;
                pinyinBytes[i * 2 + 1] = (byte)ch;
                continue;
            }

            if (!TryMapSegment(segments[i], out var sm, out var ym))
                return false;

            pinyinBytes[i * 2] = sm;
            pinyinBytes[i * 2 + 1] = ym;
        }

        Span<byte> header = stackalloc byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(header, (ushort)word.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(header[2..], DefaultFrequency);
        output.Write(header);
        output.Write(pinyinBytes);
        output.Write(Encoding.Unicode.GetBytes(word));
        return true;
    }

    private static bool TryMapSegment(IReadOnlyList<string> segment, out byte sm, out byte ym)
    {
        foreach (var candidate in segment)
        {
            if (TryMapSyllable(candidate, out sm, out ym))
                return true;
        }

        sm = 0;
        ym = 0;
        return false;
    }

    private static bool TryMapSyllable(string raw, out byte sm, out byte ym)
    {
        var syllable = NormalizeSyllable(raw);

        // 声母韵母表只有 ue，没有 lve/nve；标准拼音的 üe 在词库里写作 lue/nue
        if (syllable == "lve")
            syllable = "lue";
        else if (syllable == "nve")
            syllable = "nue";

        if (SyllableIndex.TryGetValue(syllable, out var index))
        {
            sm = index.Shengmu;
            ym = index.Yunmu;
            return true;
        }

        sm = 0;
        ym = 0;
        return false;
    }

    private static string NormalizeSyllable(string raw)
    {
        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw)
        {
            // 去掉声调数字与分隔符
            if (ch is >= '1' and <= '5' or '\'' or ' ' or '\t' or '\r' or '\n')
                continue;

            sb.Append(ch switch
            {
                'ā' or 'á' or 'ǎ' or 'à' => 'a',
                'ē' or 'é' or 'ě' or 'è' => 'e',
                'ī' or 'í' or 'ǐ' or 'ì' => 'i',
                'ō' or 'ó' or 'ǒ' or 'ò' => 'o',
                'ū' or 'ú' or 'ǔ' or 'ù' => 'u',
                'ü' or 'ǖ' or 'ǘ' or 'ǚ' or 'ǜ' => 'v',
                'ń' or 'ň' or 'ǹ' => 'n',
                'ḿ' => 'm',
                _ => char.ToLowerInvariant(ch)
            });
        }

        return sb.ToString().Replace("u:", "v", StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, (byte Shengmu, byte Yunmu)> BuildSyllableIndex()
    {
        var index = new Dictionary<string, (byte Shengmu, byte Yunmu)>(
            Shengmu.Length * Yunmu.Length, StringComparer.Ordinal);
        for (var sm = 0; sm < Shengmu.Length; sm++)
        {
            for (var ym = 0; ym < Yunmu.Length; ym++)
            {
                index[Shengmu[sm] + Yunmu[ym]] = ((byte)sm, (byte)ym);
            }
        }

        return index;
    }

    private static void WriteHeader(
        Stream output, int count, IReadOnlyList<string> sampleWords, ExportOptions? options)
    {
        var header = new byte[HeaderSize];

        // 0x00: 文件签名 "biptbdsw"
        Encoding.ASCII.GetBytes("biptbdsw").CopyTo(header, 0);
        // 0x08: 版本
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x08), 1);
        // 0x10: 词库 ID（名称优先取显式配置，其次源文件名，最后格式默认值）
        var defaultName = "深蓝词库转换";
        if (!string.IsNullOrWhiteSpace(options?.SourceFileName))
        {
            var stem = Path.GetFileNameWithoutExtension(options.SourceFileName);
            if (!string.IsNullOrWhiteSpace(stem))
                defaultName = stem;
        }

        var name = LimitText(options?.DictionaryName, defaultName, MaxNameChars);
        BinaryPrimitives.WriteUInt32LittleEndian(
            header.AsSpan(0x10), ResolveDictionaryId(options?.DictionaryId, name));
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x18), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x2C), 1);
        // 0x70/0x250: 常规词条数（部分版本只看 0x250）
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x70), (uint)count);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x250), (uint)count);

        var author = LimitText(null, "深蓝词库转换", MaxAuthorChars);
        var example = LimitText(string.Join("、", sampleWords), "", MaxExampleChars);
        var description = LimitText(
            options?.DictionaryDescription, "由深蓝词库转换工具生成", MaxDescriptionChars);

        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x80), (uint)name.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x84), (uint)author.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x88), (uint)example.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x8C), (uint)description.Length);

        WriteUtf16Field(header, NameOffset, NameFieldSize, name);
        WriteUtf16Field(header, AuthorOffset, AuthorFieldSize, author);
        WriteUtf16Field(header, ExampleOffset, ExampleFieldSize, example);
        WriteUtf16Field(header, DescriptionOffset, DescriptionFieldSize, description);

        output.Write(header);
    }

    private static uint ResolveDictionaryId(string? configuredId, string name)
    {
        if (uint.TryParse(configuredId, out var id) && id != 0)
            return id;

        // 按词库名稳定生成非零 ID，便于不同词库在输入法中区分
        var hash = 2166136261u;
        foreach (var ch in name)
            hash = (hash ^ ch) * 16777619u;
        return hash % 9_000_000u + 1_000_000u;
    }

    private static string LimitText(string? value, string fallback, int maxChars)
    {
        var text = string.IsNullOrWhiteSpace(value) ? fallback : value;
        if (text.Length > maxChars)
            text = text[..maxChars];
        if (text.Length > 0 && char.IsHighSurrogate(text[^1]))
            text = text[..^1];
        return text;
    }

    private static void WriteUtf16Field(byte[] header, int offset, int size, string text)
    {
        var bytes = Encoding.Unicode.GetBytes(text);
        Buffer.BlockCopy(bytes, 0, header, offset, Math.Min(bytes.Length, size));
    }
}
