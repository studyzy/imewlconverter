namespace ImeWlConverter.Formats.SougouScel;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Sougou Pinyin scel cell dictionary exporter (binary).</summary>
[FormatPlugin("scel", "搜狗细胞词库scel", 20, FileExtension = ".scel")]
public sealed partial class SougouScelExporter : IFormatExporter
{
    /// <summary>
    /// 搜狗 scel 文件标准拼音表（413个音节）
    /// </summary>
    internal static readonly string[] StandardPinyinTable =
    {
        "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi",
        "bian", "biao", "bie", "bin", "bing", "bo", "bu", "ca", "cai", "can", "cang", "cao", "ce", "cen",
        "ceng", "cha", "chai", "chan", "chang", "chao", "che", "chen", "cheng", "chi", "chong", "chou",
        "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci", "cong", "cou", "cu",
        "cuan", "cui", "cun", "cuo", "da", "dai", "dan", "dang", "dao", "de", "dei", "den", "deng", "di",
        "dia", "dian", "diao", "die", "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", "e",
        "ei", "en", "eng", "er", "fa", "fan", "fang", "fei", "fen", "feng", "fiao", "fo", "fou", "fu",
        "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong", "gou", "gu", "gua", "guai",
        "guan", "guang", "gui", "gun", "guo", "ha", "hai", "han", "hang", "hao", "he", "hei", "hen",
        "heng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo", "ji", "jia",
        "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", "jiu", "ju", "juan", "jue", "jun", "ka",
        "kai", "kan", "kang", "kao", "ke", "kei", "ken", "keng", "kong", "kou", "ku", "kua", "kuai",
        "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan", "lang", "lao", "le", "lei", "leng", "li",
        "lia", "lian", "liang", "liao", "lie", "lin", "ling", "liu", "lo", "long", "lou", "lu", "luan",
        "lue", "lun", "luo", "lv", "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", "mi",
        "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu", "na", "nai", "nan", "nang", "nao",
        "ne", "nei", "nen", "neng", "ni", "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong",
        "nou", "nu", "nuan", "nue", "nun", "nuo", "nv", "o", "ou", "pa", "pai", "pan", "pang", "pao",
        "pei", "pen", "peng", "pi", "pian", "piao", "pie", "pin", "ping", "po", "pou", "pu", "qi", "qia",
        "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu", "qu", "quan", "que", "qun", "ran",
        "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", "rua", "ruan", "rui", "run", "ruo",
        "sa", "sai", "san", "sang", "sao", "se", "sen", "seng", "sha", "shai", "shan", "shang", "shao",
        "she", "shei", "shen", "sheng", "shi", "shou", "shu", "shua", "shuai", "shuan", "shuang", "shui",
        "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun", "suo", "ta", "tai", "tan", "tang",
        "tao", "te", "tei", "teng", "ti", "tian", "tiao", "tie", "ting", "tong", "tou", "tu", "tuan",
        "tui", "tun", "tuo", "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu", "xi", "xia",
        "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu", "xu", "xuan", "xue", "xun", "ya",
        "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", "yong", "you", "yu", "yuan", "yue", "yun",
        "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng", "zha", "zhai", "zhan", "zhang",
        "zhao", "zhe", "zhei", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan",
        "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", "zuan", "zui", "zun", "zuo"
    };

    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        var pinyinToIndex = BuildPinyinIndex();
        var groups = GroupByPinyin(entries, pinyinToIndex);

        var groupCount = groups.Count;
        var totalWordCount = groups.Sum(g => g.Words.Count);

        var totalPinyinBytes = groups.Sum(g => g.PinyinIndices.Length * 2);
        var totalWordBytes = groups.Sum(g => g.Words.Sum(w => Encoding.Unicode.GetByteCount(w)));
        var cSize = totalPinyinBytes + groupCount * 2;
        var wSize = totalWordBytes + totalWordCount * 2;

        WriteHeader(output);
        WriteStatistics(output, groupCount, totalWordCount, cSize, wSize);
        WriteMetaInfo(output, entries);
        WritePinyinTable(output);
        WriteWordData(output, groups);

        // 回填校验和
        output.Flush();
        output.Seek(0x1540, SeekOrigin.Begin);
        var dataFromPinyin = new byte[output.Length - 0x1540];
        output.ReadExactly(dataFromPinyin, 0, dataFromPinyin.Length);
        var checksum = SougouCheckSum(dataFromPinyin);
        output.Seek(0x0C, SeekOrigin.Begin);
        for (var i = 0; i < 4; i++)
            output.Write(BitConverter.GetBytes(checksum[i]));

        output.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = totalWordCount,
            ErrorCount = entries.Count - totalWordCount
        });
    }

    #region 导出辅助方法

    private static Dictionary<string, int> BuildPinyinIndex()
    {
        var dict = new Dictionary<string, int>();
        for (var i = 0; i < StandardPinyinTable.Length; i++)
            dict[StandardPinyinTable[i]] = i;
        return dict;
    }

    private static List<PinyinWordGroup> GroupByPinyin(
        IReadOnlyList<WordEntry> entries, Dictionary<string, int> pinyinToIndex)
    {
        var groupDict = new Dictionary<string, PinyinWordGroup>();

        foreach (var entry in entries)
        {
            var pinyins = ExtractPinyin(entry);
            if (pinyins == null || pinyins.Length == 0)
                continue;

            var normalized = NormalizePinyin(pinyins);

            var allValid = true;
            foreach (var py in normalized)
            {
                if (!pinyinToIndex.ContainsKey(py))
                {
                    allValid = false;
                    break;
                }
            }

            if (!allValid) continue;

            var key = string.Join("'", normalized);
            if (!groupDict.TryGetValue(key, out var group))
            {
                group = new PinyinWordGroup
                {
                    PinyinIndices = normalized.Select(py => pinyinToIndex[py]).ToArray()
                };
                groupDict[key] = group;
            }

            group.Words.Add(entry.Word);
        }

        return groupDict.Values
            .OrderBy(g => string.Join(",", g.PinyinIndices.Select(i => i.ToString("D4"))))
            .ToList();
    }

    private static string[]? ExtractPinyin(WordEntry entry)
    {
        if (entry.Code?.Segments == null || entry.Code.Segments.Count == 0)
            return null;
        return entry.Code.Segments.Select(s => s[0]).ToArray();
    }

    private static string[] NormalizePinyin(string[] pinyin)
    {
        var result = new string[pinyin.Length];
        for (var i = 0; i < pinyin.Length; i++)
            result[i] = pinyin[i].ToLower().TrimEnd('0', '1', '2', '3', '4', '5');
        return result;
    }

    private static void WriteHeader(Stream fs)
    {
        // 文件签名: 40 15 00 00 44 43 53 01
        fs.Write(new byte[] { 0x40, 0x15, 0x00, 0x00, 0x44, 0x43, 0x53, 0x01 });

        // 标志位
        fs.Write(new byte[] { 0x01, 0x00, 0x00, 0x00 });

        // 0x000C-0x001B: 校验和占位（16字节，写完文件后回填）
        fs.Write(new byte[16]);

        // 0x001C-0x0027: 随机文件ID
        var rng = new Random();
        var idStr = rng.Next(100000, 999999).ToString();
        var idBytes = Encoding.Unicode.GetBytes(idStr);
        fs.Write(idBytes);
        var idPadding = 12 - idBytes.Length;
        if (idPadding > 0)
            fs.Write(new byte[idPadding]);

        // 填充至 0x011C
        var padding1 = 0x11C - (int)fs.Position;
        fs.Write(new byte[padding1]);

        // 0x011C: Unix时间戳
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        fs.Write(BitConverter.GetBytes(timestamp));
    }

    private static void WriteStatistics(Stream fs, int groupCount, int totalWordCount, int cSize, int wSize)
    {
        fs.Write(BitConverter.GetBytes(groupCount));
        fs.Write(BitConverter.GetBytes(totalWordCount));
        fs.Write(BitConverter.GetBytes(cSize));
        fs.Write(BitConverter.GetBytes(wSize));
    }

    private static void WriteMetaInfo(Stream fs, IReadOnlyList<WordEntry> entries)
    {
        // 0x0130: 名称（520字节）
        WriteScelField(fs, "深蓝词库转换", 520);
        // 0x0338: 类型（520字节）
        WriteScelField(fs, "自定义", 520);
        // 0x0540: 描述（2048字节）
        WriteScelField(fs, "由深蓝词库转换工具生成", 2048);
        // 0x0D40: 示例词（2048字节）
        var sample = string.Join(" ", entries.Take(5).Select(w => w.Word));
        WriteScelField(fs, sample, 2048);
    }

    private static void WriteScelField(Stream fs, string text, int fieldSize)
    {
        var bytes = Encoding.Unicode.GetBytes(text + "\0");
        if (bytes.Length > fieldSize)
        {
            bytes = bytes[..fieldSize];
            bytes[fieldSize - 2] = 0;
            bytes[fieldSize - 1] = 0;
        }

        fs.Write(bytes);
        var padding = fieldSize - bytes.Length;
        if (padding > 0)
            fs.Write(new byte[padding]);
    }

    private static void WritePinyinTable(Stream fs)
    {
        fs.Write(BitConverter.GetBytes(StandardPinyinTable.Length));

        for (var i = 0; i < StandardPinyinTable.Length; i++)
        {
            var pyBytes = Encoding.Unicode.GetBytes(StandardPinyinTable[i]);
            fs.Write(BitConverter.GetBytes((short)i));
            fs.Write(BitConverter.GetBytes((short)pyBytes.Length));
            fs.Write(pyBytes);
        }
    }

    private static void WriteWordData(Stream fs, List<PinyinWordGroup> groups)
    {
        foreach (var group in groups)
        {
            var samePyCount = (short)group.Words.Count;
            var pinyinByteLen = (short)(group.PinyinIndices.Length * 2);

            fs.Write(BitConverter.GetBytes(samePyCount));
            fs.Write(BitConverter.GetBytes(pinyinByteLen));

            foreach (var idx in group.PinyinIndices)
                fs.Write(BitConverter.GetBytes((short)idx));

            foreach (var word in group.Words)
            {
                var wordBytes = Encoding.Unicode.GetBytes(word);
                fs.Write(BitConverter.GetBytes((short)wordBytes.Length));
                fs.Write(wordBytes);
                // 附加信息固定12字节
                fs.Write(new byte[] { 0x0A, 0x00, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            }
        }
    }

    #endregion

    #region 搜狗校验和算法

    private static uint[] SougouCheckSum(byte[] data)
    {
        var state = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476 };

        var blockCount = data.Length / 64;
        var remainder = data.Length % 64;

        var fullBlocks = remainder == 0 ? blockCount - 1 : blockCount;
        for (var i = 0; i < fullBlocks; i++)
        {
            var block = new byte[64];
            Array.Copy(data, i * 64, block, 0, 64);
            SougouBlockProcess(ref state, block);
        }

        var lastData = new byte[data.Length - fullBlocks * 64];
        Array.Copy(data, fullBlocks * 64, lastData, 0, lastData.Length);
        var padded = SougouPadBlock(lastData, data.Length);

        var paddedBlocks = padded.Length / 64;
        for (var i = 0; i < paddedBlocks; i++)
        {
            var block = new byte[64];
            Array.Copy(padded, i * 64, block, 0, 64);
            SougouBlockProcess(ref state, block);
        }

        return state;
    }

    private static byte[] SougouPadBlock(byte[] data, int totalLength)
    {
        var lengthInBits = totalLength * 8;
        var paddingLength = 64 - (data.Length + 8) % 64;
        if (paddingLength <= 0)
            paddingLength += 64;

        var padded = new byte[data.Length + paddingLength + 8];
        Array.Copy(data, padded, data.Length);
        padded[data.Length] = 0x80;
        var lengthBytes = BitConverter.GetBytes((long)lengthInBits);
        Array.Copy(lengthBytes, 0, padded, padded.Length - 8, 8);

        return padded;
    }

    private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));

    private static void SougouBlockProcess(ref uint[] state, byte[] block)
    {
        var x = new uint[16];
        for (var i = 0; i < 16; i++)
            x[i] = BitConverter.ToUInt32(block, i * 4);

        var a = state[0];
        var b = state[1];
        var c = state[2];
        var d = state[3];

        // round 1
        a = (~b & d | c & b) + x[0] + 0xD76AA478 + a; a = RotateLeft(a, 7) + b;
        d = (~a & c | b & a) + x[1] + 0xE8C7B756 + d; d = RotateLeft(d, 12) + a;
        c = (~d & b | d & a) + x[2] + 0x242070DB + c; c = RotateLeft(c, 17) + d;
        b = (~c & a | d & c) + x[3] + 0xC1BDCEEE + b; b = RotateLeft(b, 22) + c;
        a = (~b & d | c & b) + x[4] + 0xF57C0FAF + a; a = RotateLeft(a, 7) + b;
        d = (~a & c | b & a) + x[5] + 0x4787C62A + d; d = RotateLeft(d, 12) + a;
        c = (~d & b | d & a) + x[6] + 0xA8304613 + c; c = RotateLeft(c, 17) + d;
        b = (~c & a | d & c) + x[7] + 0xFD469501 + b; b = RotateLeft(b, 22) + c;
        a = (~b & d | c & b) + x[8] + 0x698098D8 + a; a = RotateLeft(a, 7) + b;
        d = (~a & c | b & a) + x[9] + 0x8B44F7AF + d; d = RotateLeft(d, 12) + a;
        c = (~d & b | d & a) + x[10] + 0xFFFF5BB1 + c; c = RotateLeft(c, 17) + d;
        b = (~c & a | d & c) + x[11] + 0x895CD7BE + b; b = RotateLeft(b, 22) + c;
        a = (~b & d | c & b) + x[12] + 0x6B901122 + a; a = RotateLeft(a, 7) + b;
        d = (~a & c | b & a) + x[13] + 0xFD987193 + d; d = RotateLeft(d, 12) + a;
        c = (~d & b | d & a) + x[14] + 0xA679438E + c; c = RotateLeft(c, 17) + d;
        b = (~c & a | d & c) + x[15] + 0x49B40821 + b; b = RotateLeft(b, 22) + c;

        // round 2
        a = (~d & c | d & b) + x[1] + 0xF61E2562 + a; a = RotateLeft(a, 5) + b;
        d = (~c & b | c & a) + x[6] + 0xC040B340 + d; d = RotateLeft(d, 9) + a;
        c = (~b & a | d & b) + x[11] + 0x265E5A51 + c; c = RotateLeft(c, 14) + d;
        b = (~a & d | c & a) + x[0] + 0xE9B6C7AA + b; b = RotateLeft(b, 20) + c;
        a = (~d & c | d & b) + x[5] + 0xD62F105D + a; a = RotateLeft(a, 5) + b;
        d = (~c & b | c & a) + x[10] + 0x02441453 + d; d = RotateLeft(d, 9) + a;
        c = (~b & a | d & b) + x[15] + 0xD8A1E681 + c; c = RotateLeft(c, 14) + d;
        b = (~a & d | c & a) + x[4] + 0xE7D3FBC8 + b; b = RotateLeft(b, 20) + c;
        a = (~d & c | d & b) + x[9] + 0x21E1CDE6 + a; a = RotateLeft(a, 5) + b;
        d = (~c & b | c & a) + x[14] + 0xC33707D6 + d; d = RotateLeft(d, 9) + a;
        c = (~b & a | d & b) + x[3] + 0xF4D50D87 + c; c = RotateLeft(c, 14) + d;
        b = (~a & d | c & a) + x[8] + 0x455A14ED + b; b = RotateLeft(b, 20) + c;
        a = (~d & c | d & b) + x[13] + 0xA9E3E905 + a; a = RotateLeft(a, 5) + b;
        d = (~c & b | c & a) + x[2] + 0xFCEFA3F8 + d; d = RotateLeft(d, 9) + a;
        c = (~b & a | d & b) + x[7] + 0x676F02D9 + c; c = RotateLeft(c, 14) + d;
        b = (~a & d | c & a) + x[12] + 0x8D2A4C8A + b; b = RotateLeft(b, 20) + c;

        // round 3
        a = (d ^ c ^ b) + x[5] + 0xFFFA3942 + a; a = RotateLeft(a, 4) + b;
        d = (c ^ b ^ a) + x[8] + 0x8771F681 + d; d = RotateLeft(d, 11) + a;
        c = (d ^ b ^ a) + x[11] + 0x6D9D6122 + c; c = RotateLeft(c, 16) + d;
        b = (d ^ c ^ a) + x[14] + 0xFDE5380C + b;
        var magic = RotateLeft(b, 23) + c;
        a = a + 0xA4BEEA44 + (d ^ c ^ magic) + x[1]; b = RotateLeft(a, 4) + magic;
        d = (c ^ magic ^ b) + x[4] + 0x4BDECFA9 + d; d = RotateLeft(d, 11) + b;
        c = (d ^ magic ^ b) + x[7] + 0xF6BB4B60 + c; c = RotateLeft(c, 16) + d;
        a = magic + 0xBEBFBC70 + (d ^ c ^ b) + x[10]; a = RotateLeft(a, 23) + c;
        b = (d ^ c ^ a) + x[13] + 0x289B7EC6 + b; b = RotateLeft(b, 4) + a;
        d = (c ^ a ^ b) + x[0] + 0xEAA127FA + d; d = RotateLeft(d, 11) + b;
        c = (d ^ a ^ b) + x[3] + 0xD4EF3085 + c; c = RotateLeft(c, 16) + d;
        a = a + 0x04881D05 + (d ^ c ^ b) + x[6]; a = RotateLeft(a, 23) + c;
        b = (d ^ c ^ a) + x[9] + 0xD9D4D039 + b; b = RotateLeft(b, 4) + a;
        d = (c ^ a ^ b) + x[12] + 0xE6DB99E5 + d; d = RotateLeft(d, 11) + b;
        c = (d ^ a ^ b) + x[15] + 0x1FA27CF8 + c; c = RotateLeft(c, 16) + d;
        a = (d ^ c ^ b) + x[2] + 0xC4AC5665 + a; a = RotateLeft(a, 23) + c;

        // round 4
        b = ((~d | a) ^ c) + x[0] + 0xF4292244 + b; b = RotateLeft(b, 6) + a;
        d = ((~c | b) ^ a) + x[7] + 0x432AFF97 + d; d = RotateLeft(d, 10) + b;
        c = ((~a | d) ^ b) + x[14] + 0xAB9423A7 + c; c = RotateLeft(c, 15) + d;
        a = ((~b | c) ^ d) + x[5] + 0xFC93A039 + a; a = RotateLeft(a, 21) + c;
        b = ((~d | a) ^ c) + x[12] + 0x655B59C3 + b; b = RotateLeft(b, 6) + a;
        d = ((~c | b) ^ a) + x[3] + 0x8F0CCC92 + d; d = RotateLeft(d, 10) + b;
        c = ((~a | d) ^ b) + x[10] + 0xFFEFF47D + c; c = RotateLeft(c, 15) + d;
        a = ((~b | c) ^ d) + x[1] + 0x85845DD1 + a; a = RotateLeft(a, 21) + c;
        b = ((~d | a) ^ c) + x[8] + 0x6FA87E4F + b; b = RotateLeft(b, 6) + a;
        d = ((~c | b) ^ a) + x[15] + 0xFE2CE6E0 + d; d = RotateLeft(d, 10) + b;
        c = ((~a | d) ^ b) + x[6] + 0xA3014314 + c; c = RotateLeft(c, 15) + d;
        a = ((~b | c) ^ d) + x[13] + 0x4E0811A1 + a; a = RotateLeft(a, 21) + c;
        b = ((~d | a) ^ c) + x[4] + 0xF7537E82 + b; b = RotateLeft(b, 6) + a;
        d = ((~c | b) ^ a) + x[11] + 0xBD3AF235 + d; d = RotateLeft(d, 10) + b;
        c = ((~a | d) ^ b) + x[2] + 0x2AD7D2BB + c; c = RotateLeft(c, 15) + d;
        a = ((~b | c) ^ d) + x[9] + 0xEB86D391 + a; a = RotateLeft(a, 21) + c;

        state[0] += b;
        state[1] += a;
        state[2] += c;
        state[3] += d;
    }

    private sealed class PinyinWordGroup
    {
        public int[] PinyinIndices { get; init; } = [];
        public List<string> Words { get; } = new();
    }

    #endregion
}
