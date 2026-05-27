namespace ImeWlConverter.Formats.Win10MsSelfStudy;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Win10 Microsoft Pinyin self-study dictionary importer (binary DAT format).</summary>
[FormatPlugin("win10mspyss", "Win10微软拼音（自学习词汇）", 130)]
public sealed partial class Win10MsPinyinSelfStudyImporter : BinaryFormatImporter
{
    private const int UserWordBase = 0x2400;
    private const int EntrySize = 60;

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        var results = new List<WordEntry>();
        var fileSize = input.Length;

        if (fileSize < UserWordBase)
            throw new InvalidDataException(
                $"词库文件格式不正确,文件大小至少需要{UserWordBase}字节,当前为{fileSize}字节");

        // Read word count at offset 12
        input.Position = 12;
        var countBytes = new byte[4];
        input.ReadExactly(countBytes, 0, 4);
        var cnt = BitConverter.ToInt32(countBytes, 0);

        if (cnt < 0 || cnt > 100000)
            throw new InvalidDataException($"词条数量异常: {cnt},可能是文件格式不兼容");

        for (var i = 0; i < cnt; i++)
        {
            ct.ThrowIfCancellationRequested();

            var curIdx = UserWordBase + i * EntrySize;

            if (curIdx + EntrySize > fileSize)
                break;

            // Read word length at curIdx + 10
            input.Position = curIdx + 10;
            var wordLen = input.ReadByte() & 0xFF;

            if (wordLen <= 0 || wordLen > 24)
                continue;

            // Read word at curIdx + 12
            input.Position = curIdx + 12;
            if (curIdx + 12 + wordLen * 2 > fileSize)
                break;

            var wordBytes = new byte[wordLen * 2];
            input.ReadExactly(wordBytes, 0, wordLen * 2);
            var word = Encoding.Unicode.GetString(wordBytes, 0, wordLen * 2);

            // Read pinyin indices (each is 2 bytes, one per character)
            var pinyin = new string[wordLen];
            for (var j = 0; j < wordLen; j++)
            {
                var pyIndexBytes = new byte[2];
                input.ReadExactly(pyIndexBytes, 0, 2);
                var pyIndex = BitConverter.ToInt16(pyIndexBytes, 0);

                if (pyIndex >= 0 && pyIndex < PinyinTable.Length)
                    pinyin[j] = PinyinTable[pyIndex];
                else
                    pinyin[j] = "a";
            }

            results.Add(new WordEntry
            {
                Word = word,
                Rank = 0,
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(pinyin)
            });
        }

        if (results.Count == 0)
            throw new InvalidDataException("未能成功解析任何词条,可能是文件格式不兼容或文件已损坏");

        return results;
    }

    internal static readonly string[] PinyinTable =
    {
        "a", "ai", "an", "ang", "ao",
        "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian",
        "biao", "bie", "bin", "bing", "bo", "bu",
        "ca", "cai", "can", "cang", "cao", "ce", "cen", "ceng",
        "cha", "chai", "chan", "chang", "chao", "che", "chen", "cheng", "chi",
        "chong", "chou", "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", "chuo",
        "ci", "cong", "cou", "cu", "cuan", "cui", "cun", "cuo",
        "da", "dai", "dan", "dang", "dao", "de", "dei", "den", "deng", "di",
        "dia", "dian", "diao", "die", "ding", "diu", "dong", "dou", "du", "duan",
        "dui", "dun", "duo",
        "e", "ei", "en", "eng", "er",
        "fa", "fan", "fang", "fei", "fen", "feng", "fiao", "fo", "fou", "fu",
        "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong",
        "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo",
        "ha", "hai", "han", "hang", "hao", "he", "hei", "hen", "heng", "hong",
        "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo",
        "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", "jiu",
        "ju", "juan", "jue", "jun",
        "ka", "kai", "kan", "kang", "kao", "ke", "kei", "ken", "keng", "kong",
        "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo",
        "la", "lai", "lan", "lang", "lao", "le", "lei", "leng", "li", "lia",
        "lian", "liang", "liao", "lie", "lin", "ling", "liu", "lo", "long", "lou",
        "lu", "luan", "lue", "lun", "luo", "lv",
        "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", "mi",
        "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu",
        "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ni",
        "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nou", "nu",
        "nuan", "nue", "nun", "nuo", "nv",
        "o", "ou",
        "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng", "pi", "pian",
        "piao", "pie", "pin", "ping", "po", "pou", "pu",
        "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu",
        "qu", "quan", "que", "qun",
        "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru",
        "rua", "ruan", "rui", "run", "ruo",
        "sa", "sai", "san", "sang", "sao", "se", "sen", "seng",
        "sha", "shai", "shan", "shang", "shao", "she", "shei", "shen", "sheng", "shi",
        "shou", "shu", "shua", "shuai", "shuan", "shuang", "shui", "shun", "shuo",
        "si", "song", "sou", "su", "suan", "sui", "sun", "suo",
        "ta", "tai", "tan", "tang", "tao", "te", "tei", "teng", "ti", "tian",
        "tiao", "tie", "ting", "tong", "tou", "tu", "tuan", "tui", "tun", "tuo",
        "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu",
        "xi", "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu",
        "xu", "xuan", "xue", "xun",
        "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", "yong",
        "you", "yu", "yuan", "yue", "yun",
        "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng",
        "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhei", "zhen", "zheng", "zhi",
        "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan", "zhuang", "zhui", "zhun", "zhuo",
        "zi", "zong", "zou", "zu", "zuan", "zui", "zun", "zuo"
    };
}
