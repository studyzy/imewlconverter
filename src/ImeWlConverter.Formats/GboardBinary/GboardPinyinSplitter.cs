namespace ImeWlConverter.Formats.GboardBinary;

/// <summary>
/// 把连写的拼音串切分成音节(用于源格式只提供整串拼音的情况)。
/// 已知目标音节数时按该数量做动态规划, 结果确定。
/// </summary>
internal static class GboardPinyinSplitter
{
    /// <summary>合法普通话音节表(386 个, 取自 Gboard 官方词典实际使用的组合)。</summary>
    private static readonly string[] Syllables =
    {
        "a", "ai", "an", "ao", "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng",
        "bi", "bian", "biao", "bie", "bin", "bing", "bo", "bu", "ca", "cai", "can", "cang",
        "cao", "ce", "ceng", "cha", "chai", "chan", "chang", "chao", "che", "chen", "cheng", "chi",
        "chong", "chou", "chu", "chuan", "chuang", "chui", "chun", "ci", "cong", "cou", "cu", "cui",
        "cun", "cuo", "da", "dai", "dan", "dang", "dao", "de", "deng", "di", "dian", "diao",
        "die", "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", "e", "ei",
        "en", "er", "fa", "fan", "fang", "fei", "fen", "feng", "fo", "fou", "fu", "ga",
        "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong", "gou", "gu", "gua",
        "guai", "guan", "guang", "gui", "gun", "guo", "ha", "hai", "han", "hang", "hao", "he",
        "hei", "hen", "heng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun",
        "huo", "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", "jiu", "ju",
        "juan", "jun", "jve", "ka", "kai", "kan", "kang", "kao", "ke", "ken", "keng", "kong",
        "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan",
        "lang", "lao", "le", "lei", "leng", "li", "lian", "liang", "liao", "lie", "lin", "ling",
        "liu", "long", "lou", "lu", "luan", "lun", "luo", "lv", "lve", "ma", "mai", "man",
        "mang", "mao", "me", "mei", "men", "meng", "mi", "mian", "miao", "mie", "min", "ming",
        "mo", "mou", "mu", "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng",
        "ni", "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nou", "nu", "nuan",
        "nuo", "nv", "o", "ou", "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng",
        "pi", "pian", "piao", "pie", "pin", "ping", "po", "pu", "qi", "qian", "qiang", "qiao",
        "qie", "qin", "qing", "qiong", "qiu", "qu", "quan", "qun", "qve", "ran", "rang", "rao",
        "re", "ren", "reng", "ri", "rong", "rou", "ru", "ruan", "rui", "run", "ruo", "sa",
        "sai", "san", "sang", "sao", "se", "sen", "sha", "shai", "shan", "shang", "shao", "she",
        "shei", "shen", "sheng", "shi", "shou", "shu", "shua", "shuai", "shuang", "shui", "shun", "shuo",
        "si", "song", "sou", "su", "suan", "sui", "sun", "suo", "ta", "tai", "tan", "tang",
        "tao", "te", "teng", "ti", "tian", "tiao", "tie", "ting", "tong", "tou", "tu", "tuan",
        "tui", "tun", "tuo", "wa", "wai", "wan", "wang", "wei", "wen", "wo", "wu", "xi",
        "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu", "xu", "xuan", "xun",
        "xve", "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", "yong", "you",
        "yu", "yuan", "yun", "yve", "za", "zai", "zan", "zang", "zao", "ze", "zen", "zeng",
        "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu",
        "zhua", "zhuan", "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", "zuan", "zui",
        "zun", "zuo",
    };

    private static readonly HashSet<string> Valid = new(Syllables, StringComparer.Ordinal);
    private static readonly string[] ByLength = Syllables.OrderByDescending(s => s.Length).ToArray();

    /// <summary>音节串是否合法。</summary>
    public static bool IsValidSyllable(string s) => Valid.Contains(s);

    /// <summary>
    /// 把 <paramref name="joined"/> 切成恰好 <paramref name="count"/> 个音节。
    /// 成功返回音节列表, 失败返回 null。
    /// </summary>
    public static List<string>? Split(string joined, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(joined))
            return null;

        var text = joined.ToLowerInvariant();
        var n = text.Length;

        // dp[i, k] = 前 i 个字符切成 k 个音节的方案数(只保留可达性)
        var reachable = new bool[n + 1, count + 1];
        reachable[0, 0] = true;

        for (var i = 0; i < n; i++)
        {
            for (var k = 0; k < count; k++)
            {
                if (!reachable[i, k])
                    continue;
                foreach (var s in ByLength)
                {
                    if (i + s.Length > n)
                        continue;
                    if (!Valid.Contains(text.Substring(i, s.Length)))
                        continue;
                    reachable[i + s.Length, k + 1] = true;
                }
            }
        }

        if (!reachable[n, count])
            return null;

        // 回溯(优先长音节)
        var result = new List<string>(count);
        var pos = n;
        var left = count;
        while (pos > 0 && left > 0)
        {
            var matched = false;
            foreach (var s in ByLength)
            {
                var start = pos - s.Length;
                if (start < 0)
                    continue;
                if (!Valid.Contains(text.Substring(start, s.Length)))
                    continue;
                if (!reachable[start, left - 1])
                    continue;
                result.Add(text.Substring(start, s.Length));
                pos = start;
                left--;
                matched = true;
                break;
            }
            if (!matched)
                return null;
        }

        result.Reverse();
        return result;
    }
}
