namespace ImeWlConverter.Formats.QingJian;

using System.Buffers.Binary;
using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// 青简词库（.qj 二进制容器）导出器，可直接通过青简「导入词库…」或放入词库目录使用。
/// 布局：TEXT/KEYS 两个 UTF-8 arena + INDX 按键字节序升序 + SLOT 同键下按词频降序，
/// 与青简 <c>Dictionary::write_qj</c> 的落盘布局一致。
/// 词条需要拼音编码；导出时 lue/nue 统一为青简规范形式 lve/nve。
/// </summary>
[FormatPlugin("qj", "青简词库（.qj）", 161, IsBinary = true, FileExtension = ".qj")]
public sealed partial class QingJianQjExporter : IFormatExporter
{
    private const int MaxFieldBytes = 65535; // 词与拼音的字节长度上限（u16）

    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        var rows = new List<(byte[] Key, byte[] Text, uint Frequency)>(entries.Count);
        var errorCount = 0;

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();

            var word = entry.Word;
            var pinyin = QingJianQjSpec.CanonicalizePinyin(entry.Code?.GetPrimaryCode(" ") ?? "");
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(pinyin))
            {
                errorCount++;
                continue;
            }

            var textBytes = Encoding.UTF8.GetByteCount(word);
            var keyBytes = Encoding.UTF8.GetByteCount(pinyin);
            if (textBytes == 0 || textBytes > MaxFieldBytes || keyBytes > MaxFieldBytes)
            {
                errorCount++;
                continue;
            }

            var frequency = entry.Rank < 0 ? 0u : (uint)entry.Rank;
            rows.Add((Encoding.UTF8.GetBytes(pinyin), Encoding.UTF8.GetBytes(word), frequency));
        }

        if (rows.Count == 0)
            throw new InvalidOperationException(
                "没有可导出的词条: 全部缺少拼音或无法编码。请确认使用了拼音编码生成器。");

        // 键按字节序升序（LINQ 排序稳定，同频保持原序，与青简 sort_by 行为一致）；
        // 同一键下词频降序
        var sorted = rows
            .OrderBy(r => r.Key, ByteArrayComparer.Instance)
            .ThenByDescending(r => r.Frequency)
            .ToList();

        var textArena = new MemoryStream();
        var keysArena = new MemoryStream();
        var index = new MemoryStream();
        var slots = new MemoryStream();

        var position = 0;
        while (position < sorted.Count)
        {
            var key = sorted[position].Key;
            var groupEnd = position + 1;
            while (groupEnd < sorted.Count && sorted[groupEnd].Key.AsSpan().SequenceEqual(key))
                groupEnd++;

            WriteUInt32(index, (uint)keysArena.Length);                 // key_start
            WriteUInt32(index, (uint)(slots.Length / QingJianQjSpec.SlotSize)); // first_slot
            WriteUInt32(index, (uint)(groupEnd - position));            // slot_count
            WriteUInt16(index, (ushort)key.Length);                     // key_len
            WriteUInt16(index, 0);                                      // reserved
            keysArena.Write(key);

            for (var i = position; i < groupEnd; i++)
            {
                var (_, text, frequency) = sorted[i];
                WriteUInt32(slots, (uint)textArena.Length);             // text_start
                WriteUInt32(slots, frequency);                          // frequency
                WriteUInt16(slots, (ushort)text.Length);                // text_len
                WriteUInt16(slots, 0);                                  // reserved
                textArena.Write(text);
            }

            position = groupEnd;
        }

        var sections = new List<(byte[] Tag, byte[] Body)>
        {
            (QingJianQjSpec.MetaTag, Encoding.UTF8.GetBytes(BuildMeta(options, sorted.Count))),
            (QingJianQjSpec.TextTag, textArena.ToArray()),
            (QingJianQjSpec.KeysTag, keysArena.ToArray()),
            (QingJianQjSpec.IndexTag, index.ToArray()),
            (QingJianQjSpec.SlotsTag, slots.ToArray()),
        };

        var container = SerializeContainer(sections);
        output.Write(container, 0, container.Length);
        output.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = sorted.Count,
            ErrorCount = errorCount
        });
    }

    private static string BuildMeta(ExportOptions? options, int entryCount)
    {
        return "name = \"" + QingJianQjSpec.EscapeToml(ResolveName(options)) + "\"\n"
            + "license = \"\"\n"
            + "attribution = \"\"\n"
            + "source = \"\"\n"
            + "version = \"\"\n"
            + $"entries = {entryCount}\n"
            + "generator = \"ImeWlConverter\"\n";
    }

    /// <summary>
    /// META.name 取值优先级：显式指定的词库名称（--dict-name 等）→ 源词库文件名主干 → 默认名称。
    /// </summary>
    private static string ResolveName(ExportOptions? options)
    {
        if (!string.IsNullOrWhiteSpace(options?.DictionaryName))
            return options.DictionaryName!;

        var source = options?.SourceFileName;
        if (!string.IsNullOrWhiteSpace(source))
        {
            var stem = Path.GetFileNameWithoutExtension(source);
            if (!string.IsNullOrWhiteSpace(stem))
                return stem;
        }

        return "深蓝词库转换";
    }

    /// <summary>32 字节头 + 24 字节/节的分节表 + 按 8 字节对齐落盘的各分节正文。</summary>
    private static byte[] SerializeContainer(List<(byte[] Tag, byte[] Body)> sections)
    {
        using var stream = new MemoryStream();

        var header = new byte[QingJianQjSpec.HeaderSize];
        QingJianQjSpec.Magic.CopyTo(header, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(8, 2), QingJianQjSpec.FormatVersion);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10, 2), QingJianQjSpec.KindDictionary);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(12, 4), (uint)sections.Count);
        stream.Write(header);

        var tableEnd = QingJianQjSpec.HeaderSize + sections.Count * QingJianQjSpec.SectionEntrySize;
        var offset = AlignUp(tableEnd);
        foreach (var (tag, body) in sections)
        {
            stream.Write(tag);
            stream.Write(new byte[4]); // reserved
            WriteUInt64(stream, offset);
            WriteUInt64(stream, body.Length);
            offset = AlignUp(offset + body.Length);
        }

        // 分节表与第一个分节之间的填充
        WritePadding(stream);

        foreach (var (_, body) in sections)
        {
            stream.Write(body);
            WritePadding(stream);
        }

        return stream.ToArray();
    }

    private static int AlignUp(int value) =>
        (value + QingJianQjSpec.SectionAlign - 1) / QingJianQjSpec.SectionAlign * QingJianQjSpec.SectionAlign;

    /// <summary>从流的当前位置填充零到下一个 8 字节对齐边界。</summary>
    private static void WritePadding(MemoryStream stream)
    {
        var remainder = (int)(stream.Length % QingJianQjSpec.SectionAlign);
        if (remainder != 0)
            stream.Write(new byte[QingJianQjSpec.SectionAlign - remainder]);
    }

    private static void WriteUInt32(MemoryStream stream, uint value)
    {
        var buffer = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteUInt16(MemoryStream stream, ushort value)
    {
        var buffer = new byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteUInt64(MemoryStream stream, long value)
    {
        var buffer = new byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, unchecked((ulong)value));
        stream.Write(buffer);
    }

    /// <summary>按 UTF-8 字节序比较（与青简 Rust 的 str 字节序排序一致）。</summary>
    private sealed class ByteArrayComparer : IComparer<byte[]>
    {
        public static readonly ByteArrayComparer Instance = new();

        public int Compare(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x is null)
                return -1;
            if (y is null)
                return 1;
            var min = Math.Min(x.Length, y.Length);
            for (var i = 0; i < min; i++)
            {
                var diff = x[i] - y[i];
                if (diff != 0)
                    return diff;
            }
            return x.Length - y.Length;
        }
    }
}
