namespace ImeWlConverter.Formats.BaiduBdict;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// Baidu Pinyin bdict dictionary importer (binary format).
/// 词条区分三段：常规(格式一)、纯英文(格式二)、混合/颜文字(格式三)。
/// 早期实现只认格式一，且把 0x60（混合区起始）误当成词条区结束，
/// 导致含英文词条解析错误、最后一个词条/颜文字词条被丢弃。
/// </summary>
[FormatPlugin("bdict", "百度分类词库bdict", 100)]
public sealed partial class BaiduBdictImporter : BinaryFormatImporter
{
    private const int DataStart = 0x350;
    private const int MaxSyllables = 1000;

    private static readonly string[] Shengmu =
    {
        "c", "d", "b", "f", "g", "h", "ch", "j", "k", "l",
        "m", "n", "", "p", "q", "r", "s", "t", "sh", "zh",
        "w", "x", "y", "z"
    };

    private static readonly string[] Yunmu =
    {
        "uang", "iang", "iong", "ang", "eng", "ian", "iao", "ing", "ong", "uai",
        "uan", "ai", "an", "ao", "ei", "en", "er", "ua", "ie", "in",
        "iu", "ou", "ia", "ue", "ui", "un", "uo", "a", "e", "i",
        "o", "u", "v"
    };

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        var results = new List<WordEntry>();

        // 需要回退 2 字节做格式判别，先整体缓冲（与 BaiduBcdImporter 一致）。
        // 用 TryGetBuffer 复用内部数组，避免 ToArray() 再复制一份大文件。
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        if (!ms.TryGetBuffer(out var segment))
            throw new InvalidDataException("百度 bdict 数据无法缓冲，解析失败");
        var bytes = segment.Array!;
        var length = ms.Length;
        var dataEnd = ResolveDataEnd(bytes, length);

        using var reader = new BinaryReader(ms, Encoding.Unicode, leaveOpen: true);
        ms.Position = DataStart;

        while (ms.Position + 4 <= dataEnd)
        {
            ct.ThrowIfCancellationRequested();

            var entry = ReadOneWord(reader, ms, dataEnd);
            if (entry is null)
                break;
            if (entry.Word.Length > 0)
                results.Add(entry);
        }

        return results;
    }

    /// <summary>
    /// 词条区结束位置取三个声明分区（常规/英文/混合）的最大末端，兼容旧版只用 0x44 长度的头。
    /// 0x60 是「混合区起始」而非整个词条区结束，不能再拿它当边界。
    /// 声明的末端超过文件长度说明文件被截断，必须显式报错而不是静默返回部分词条。
    /// </summary>
    private static long ResolveDataEnd(byte[] bytes, long length)
    {
        long dataEnd = DataStart;
        var declared = false;
        (int OffsetPos, int SizePos)[] sections = [(0x40, 0x44), (0x50, 0x54), (0x60, 0x64)];
        foreach (var (offsetPos, sizePos) in sections)
        {
            if (offsetPos + 4 > length || sizePos + 4 > length)
                continue;
            var offset = BitConverter.ToUInt32(bytes, offsetPos);
            var size = BitConverter.ToUInt32(bytes, sizePos);
            if (offset < DataStart || size == 0)
                continue;

            var end = (long)offset + size;
            if (end > length)
                throw new InvalidDataException(
                    $"百度 bdict 文件被截断：声明的词条区末端 {end} 超过文件长度 {length}");
            dataEnd = Math.Max(dataEnd, end);
            declared = true;
        }

        // 旧版头部：0x44 为常规区长度，词条区 = 0x350 + 该长度
        if (length >= 0x48)
        {
            var regularSize = BitConverter.ToUInt32(bytes, 0x44);
            if (regularSize > 0 && regularSize < length)
            {
                var end = DataStart + (long)regularSize;
                if (end > length)
                    throw new InvalidDataException(
                        $"百度 bdict 文件被截断：常规词条区末端 {end} 超过文件长度 {length}");
                dataEnd = Math.Max(dataEnd, end);
                declared = true;
            }
        }

        // 没有任何有效分区信息时扫描到文件末尾（配合 +4 边界守卫）
        return !declared || dataEnd <= DataStart ? length : dataEnd;
    }

    private static WordEntry? ReadOneWord(BinaryReader reader, Stream stream, long dataEnd)
    {
        // 所有格式的头 4 字节都是 [pyLen:u16][freq:u16]
        var pyLen = reader.ReadUInt16();
        reader.ReadUInt16(); // freq：当前不使用

        // 格式三-B（旧版 SpecialWord / movie.bdict 尾部）：pyLen==0 前缀 + [codeLen][wordLen]
        if (pyLen == 0)
        {
            if (stream.Position + 4 > dataEnd)
                return null; // 尾部不足一条记录的填充，正常结束
            var codeLen = reader.ReadUInt16();
            var wordLen = reader.ReadUInt16();
            if (codeLen == 0 || wordLen == 0)
                return null; // 尾部零填充，正常结束
            return ReadMixed(reader, stream, dataEnd, codeLen, wordLen);
        }

        if (pyLen > MaxSyllables)
            throw new InvalidDataException(
                $"百度 bdict 词条音节数异常: {pyLen}（文件可能损坏或加密）");

        // 窥视 2 字节判别格式（不移动游标）
        if (stream.Position + 2 > dataEnd)
            throw new InvalidDataException("百度 bdict 词条在格式判别处被截断");
        var peek = stream.Position;
        var b0 = reader.ReadByte();
        var b1 = reader.ReadByte();
        stream.Position = peek;

        // 格式三-A（现代颜文字）：[0000][wordLen][code UTF-16][word UTF-16]
        if (b0 == 0 && b1 == 0)
        {
            stream.Position += 2; // 跳过 0000 标记
            if (stream.Position + 2 > dataEnd)
                throw new InvalidDataException("百度 bdict 混合词条缺少词长字段");
            var wordLen = reader.ReadUInt16();
            return ReadMixed(reader, stream, dataEnd, pyLen, wordLen);
        }

        // 格式二（纯英文）：首字节不是合法声母下标，直接是 ASCII 词面
        if (b0 >= Shengmu.Length && b0 != 0xFF)
        {
            if (stream.Position + pyLen > dataEnd)
                throw new InvalidDataException("百度 bdict 英文词条被截断");
            var ascii = reader.ReadBytes(pyLen);
            if (ascii.Length < pyLen)
                throw new InvalidDataException("百度 bdict 英文词条被截断");
            var englishWord = Encoding.ASCII.GetString(ascii);
            return Make(englishWord, [englishWord]);
        }

        // 格式一（常规）：[pyLen][freq][(声母,韵母)×pyLen][word UTF-16]
        if (stream.Position + (long)pyLen * 4 > dataEnd)
            throw new InvalidDataException("百度 bdict 常规词条被截断");

        var codes = new string[pyLen];
        for (var i = 0; i < pyLen; i++)
        {
            var smIndex = reader.ReadByte();
            var ymIndex = reader.ReadByte();
            if (smIndex == 0xFF)
                codes[i] = ((char)ymIndex).ToString(); // 英文/数字/符号字符
            else if (smIndex < Shengmu.Length && ymIndex < Yunmu.Length)
                codes[i] = Shengmu[smIndex] + Yunmu[ymIndex];
            else
                codes[i] = "";
        }

        var wordBytes = reader.ReadBytes(2 * pyLen);
        if (wordBytes.Length < 2 * pyLen)
            throw new InvalidDataException("百度 bdict 常规词条词面被截断");
        var word = Encoding.Unicode.GetString(wordBytes);

        // 整条都是符号时也要给一个非空编码，否则会被 RemoveEmpty 过滤掉
        return Make(word, codes.Any(c => c.Length > 0) ? codes : [word]);
    }

    private static WordEntry ReadMixed(
        BinaryReader reader, Stream stream, long dataEnd, int codeLen, int wordLen)
    {
        var total = 2L * (codeLen + wordLen);
        if (stream.Position + total > dataEnd)
            throw new InvalidDataException("百度 bdict 混合词条被截断");

        var codeBytes = reader.ReadBytes(2 * codeLen);
        var wordBytes = reader.ReadBytes(2 * wordLen);
        if (codeBytes.Length < 2 * codeLen || wordBytes.Length < 2 * wordLen)
            throw new InvalidDataException("百度 bdict 混合词条被截断");

        var word = Encoding.Unicode.GetString(wordBytes);
        var codes = Encoding.Unicode.GetString(codeBytes)
            .Split('\'', StringSplitOptions.RemoveEmptyEntries);
        return Make(word, codes.Length > 0 ? codes : [word]);
    }

    private static WordEntry Make(string word, IEnumerable<string> codes) => new()
    {
        Word = word,
        Rank = 0,
        CodeType = CodeType.Pinyin,
        Code = WordCode.FromSingle(codes)
    };
}
