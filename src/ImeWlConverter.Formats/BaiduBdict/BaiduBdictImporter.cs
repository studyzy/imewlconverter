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

        // 需要回退 2 字节做格式判别，先整体缓冲（与 BaiduBcdImporter 一致）
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        var bytes = ms.ToArray();
        var dataEnd = ResolveDataEnd(bytes);

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
    /// </summary>
    private static long ResolveDataEnd(byte[] bytes)
    {
        long dataEnd = DataStart;
        (int OffsetPos, int SizePos)[] sections = [(0x40, 0x44), (0x50, 0x54), (0x60, 0x64)];
        foreach (var (offsetPos, sizePos) in sections)
        {
            if (sizePos + 4 > bytes.Length)
                continue;
            var offset = BitConverter.ToUInt32(bytes, offsetPos);
            var size = BitConverter.ToUInt32(bytes, sizePos);
            if (offset >= DataStart && size > 0)
                dataEnd = Math.Max(dataEnd, (long)offset + size);
        }

        // 旧版头部：0x44 为常规区长度，词条区 = 0x350 + 该长度
        if (bytes.Length >= 0x48)
        {
            var regularSize = BitConverter.ToUInt32(bytes, 0x44);
            if (regularSize > 0 && regularSize < bytes.Length)
                dataEnd = Math.Max(dataEnd, DataStart + (long)regularSize);
        }

        dataEnd = Math.Min(dataEnd, bytes.Length);
        return dataEnd <= DataStart ? bytes.Length : dataEnd;
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
                return null;
            var codeLen = reader.ReadUInt16();
            var wordLen = reader.ReadUInt16();
            if (codeLen == 0 || wordLen == 0)
                return null; // 尾部零填充，正常结束
            return ReadMixed(reader, stream, dataEnd, codeLen, wordLen);
        }

        if (pyLen > MaxSyllables)
            return null;

        // 窥视 2 字节判别格式（不移动游标）
        if (stream.Position + 2 > dataEnd)
            return null;
        var peek = stream.Position;
        var b0 = reader.ReadByte();
        var b1 = reader.ReadByte();
        stream.Position = peek;

        // 格式三-A（现代颜文字）：[0000][wordLen][code UTF-16][word UTF-16]
        if (b0 == 0 && b1 == 0)
        {
            stream.Position += 2; // 跳过 0000 标记
            if (stream.Position + 2 > dataEnd)
                return null;
            var wordLen = reader.ReadUInt16();
            return ReadMixed(reader, stream, dataEnd, pyLen, wordLen);
        }

        // 格式二（纯英文）：首字节不是合法声母下标，直接是 ASCII 词面
        if (b0 >= Shengmu.Length && b0 != 0xFF)
        {
            if (stream.Position + pyLen > dataEnd)
                return null;
            var ascii = reader.ReadBytes(pyLen);
            if (ascii.Length < pyLen)
                return null;
            var englishWord = Encoding.ASCII.GetString(ascii);
            return Make(englishWord, [englishWord]);
        }

        // 格式一（常规）：[pyLen][freq][(声母,韵母)×pyLen][word UTF-16]
        if (stream.Position + (long)pyLen * 4 > dataEnd)
            return null;

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
            return null;
        var word = Encoding.Unicode.GetString(wordBytes);

        // 整条都是符号时也要给一个非空编码，否则会被 RemoveEmpty 过滤掉
        return Make(word, codes.Any(c => c.Length > 0) ? codes : [word]);
    }

    private static WordEntry? ReadMixed(
        BinaryReader reader, Stream stream, long dataEnd, int codeLen, int wordLen)
    {
        var total = 2L * (codeLen + wordLen);
        if (stream.Position + total > dataEnd)
            return null;

        var codeBytes = reader.ReadBytes(2 * codeLen);
        var wordBytes = reader.ReadBytes(2 * wordLen);
        if (codeBytes.Length < 2 * codeLen || wordBytes.Length < 2 * wordLen)
            return null;

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
