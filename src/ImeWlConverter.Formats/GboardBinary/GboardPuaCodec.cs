namespace ImeWlConverter.Formats.GboardBinary;

/// <summary>
/// Gboard 拼音 → PUA 编码。
///
/// 编码规则(从官方 user_dict_3_3 词典 195,842 个路径段零违例验证):
/// <code>
///   c0 码 = { 0xEE, 声母索引, 0x80 }              缩写形式, 只标声母
///   c1 码 = { 0xEE, 声母索引, 韵母索引 }           完整形式
/// </code>
/// 声调完全忽略 —— 妈/麻/马/骂(mā/má/mǎ/mà)编码相同。
/// 还原出的 (声母,韵母) 组合全部是合法普通话音节。
/// </summary>
internal static class GboardPuaCodec
{
    /// <summary>声母索引(byte 2)。0x81 表示零声母(a/e/o 开头)。</summary>
    private static readonly Dictionary<string, byte> InitialIndex = new()
    {
        [""] = 0x81,
        ["b"] = 0x82, ["c"] = 0x83, ["ch"] = 0x84, ["d"] = 0x85,
        ["f"] = 0x86, ["g"] = 0x87, ["h"] = 0x88, ["j"] = 0x89,
        ["k"] = 0x8A, ["l"] = 0x8B, ["m"] = 0x8C, ["n"] = 0x8D,
        ["p"] = 0x8E, ["q"] = 0x8F, ["r"] = 0x90, ["s"] = 0x91,
        ["sh"] = 0x92, ["t"] = 0x93, ["w"] = 0x94, ["x"] = 0x95,
        ["y"] = 0x96, ["z"] = 0x97, ["zh"] = 0x98,
    };

    /// <summary>韵母索引(byte 3, c1 形式)。</summary>
    private static readonly Dictionary<string, byte> FinalIndex = new()
    {
        ["ang"] = 0x82, ["ei"] = 0x83, ["ai"] = 0x85, ["in"] = 0x86,
        ["iu"] = 0x87, ["ong"] = 0x88, ["ao"] = 0x89, ["an"] = 0x8A,
        ["uai"] = 0x8B, ["en"] = 0x8C, ["iong"] = 0x8D, ["uan"] = 0x8E,
        ["ia"] = 0x8F, ["ing"] = 0x90, ["ie"] = 0x91, ["er"] = 0x92,
        ["iao"] = 0x93, ["ian"] = 0x94, ["eng"] = 0x96, ["iang"] = 0x97,
        ["ui"] = 0x98, ["uang"] = 0x9A, ["a"] = 0x9B, ["e"] = 0x9C,
        ["i"] = 0x9D, ["o"] = 0x9E, ["uo"] = 0x9F, ["un"] = 0xA0,
        ["u"] = 0xA1, ["v"] = 0xA2, ["ve"] = 0xA3, ["ou"] = 0xA4,
        ["ua"] = 0xA5,
    };

    /// <summary>声母按长度降序, 用于最长优先匹配(zh/ch/sh 先于 z/c/s)。</summary>
    private static readonly string[] InitialsByLength =
        InitialIndex.Keys.Where(k => k.Length > 0).OrderByDescending(k => k.Length).ToArray();

    /// <summary>声调符号 → 无调字母。</summary>
    private static readonly Dictionary<char, char> ToneMap = new()
    {
        ['ā'] = 'a', ['á'] = 'a', ['ǎ'] = 'a', ['à'] = 'a',
        ['ē'] = 'e', ['é'] = 'e', ['ě'] = 'e', ['è'] = 'e',
        ['ī'] = 'i', ['í'] = 'i', ['ǐ'] = 'i', ['ì'] = 'i',
        ['ō'] = 'o', ['ó'] = 'o', ['ǒ'] = 'o', ['ò'] = 'o',
        ['ū'] = 'u', ['ú'] = 'u', ['ǔ'] = 'u', ['ù'] = 'u',
        ['ǖ'] = 'v', ['ǘ'] = 'v', ['ǚ'] = 'v', ['ǜ'] = 'v',
        ['ü'] = 'v',
    };

    /// <summary>
    /// 把一个拼音音节拆成 (声母, 韵母)。失败返回 false。
    /// </summary>
    public static bool TrySplit(string pinyin, out string initial, out string final)
    {
        initial = "";
        final = "";
        if (string.IsNullOrWhiteSpace(pinyin))
            return false;

        var sb = new System.Text.StringBuilder(pinyin.Length);
        foreach (var raw in pinyin.Trim().ToLowerInvariant())
        {
            if (raw >= '0' && raw <= '9')
                continue;                                   // 声调数字
            sb.Append(ToneMap.TryGetValue(raw, out var plain) ? plain : raw);
        }
        var py = sb.ToString();
        if (py.Length == 0)
            return false;

        var ini = "";
        foreach (var candidate in InitialsByLength)
        {
            if (py.StartsWith(candidate, StringComparison.Ordinal))
            {
                ini = candidate;
                break;
            }
        }

        var fin = py[ini.Length..];

        // 归一化: 标准拼音把 üe 写成 ue, 而 Gboard 用 ve
        //   jue→jve  que→qve  xue→xve  yue→yve  lue→lve  nue→nve
        // (项目 ChineseCode.txt 里这 6 种写法共影响 271 个字)
        if (fin == "ue")
            fin = "ve";

        if (!FinalIndex.ContainsKey(fin))
            return false;

        initial = ini;
        final = fin;
        return true;
    }

    /// <summary>拼音音节 → 3 字节 PUA 码(c1 完整形式)。</summary>
    public static bool TryEncodeC1(string pinyin, out byte[] code)
    {
        code = new byte[3];
        if (!TrySplit(pinyin, out var ini, out var fin))
            return false;
        code[0] = 0xEE;
        code[1] = InitialIndex[ini];
        code[2] = FinalIndex[fin];
        return true;
    }

    /// <summary>拼音音节 → 3 字节 PUA 码(c0 缩写形式, 只标声母)。</summary>
    public static bool TryEncodeC0(string pinyin, out byte[] code)
    {
        code = new byte[3];
        if (!TrySplit(pinyin, out var ini, out _))
            return false;
        code[0] = 0xEE;
        code[1] = InitialIndex[ini];
        code[2] = 0x80;
        return true;
    }

    /// <summary>拼音音节 → 3 字节 PUA 码, 失败抛异常。</summary>
    public static byte[] EncodeC1(string pinyin)
        => TryEncodeC1(pinyin, out var c) ? c : throw new ArgumentException($"无法识别的拼音音节: {pinyin}", nameof(pinyin));

    /// <summary>拼音音节 → 3 字节 PUA 码(c0), 失败抛异常。</summary>
    public static byte[] EncodeC0(string pinyin)
        => TryEncodeC0(pinyin, out var c) ? c : throw new ArgumentException($"无法识别的拼音音节: {pinyin}", nameof(pinyin));

    /// <summary>3 字节 PUA 码 → 拼音音节(c0 形式只还原声母, 韵母为 '?')。</summary>
    public static string Decode(ReadOnlySpan<byte> code)
    {
        if (code.Length != 3 || code[0] != 0xEE)
            throw new ArgumentException("不是 Gboard PUA 码", nameof(code));

        // 注意: ReadOnlySpan 是 ref-like 类型, 不能进 lambda, 先取出标量
        var iniIdx = code[1];
        var finIdx = code[2];

        var ini = InitialIndex.FirstOrDefault(kv => kv.Value == iniIdx).Key
                  ?? throw new ArgumentException($"未知声母索引: {iniIdx:X2}", nameof(code));
        if (finIdx == 0x80)
            return ini + "?";
        var fin = FinalIndex.FirstOrDefault(kv => kv.Value == finIdx).Key
                  ?? throw new ArgumentException($"未知韵母索引: {finIdx:X2}", nameof(code));
        return ini + fin;
    }

    /// <summary>
    /// 判断一个码是否为 c0 缩写形式(byte3 == 0x80)。
    /// </summary>
    public static bool IsC0(ReadOnlySpan<byte> code) => code.Length == 3 && code[2] == 0x80;

    /// <summary>
    /// 构造一个词的路径。逐字拼接: 汉字 → 3 字节 PUA, ASCII 可打印字符 → 1 字节小写 ASCII。
    /// </summary>
    /// <param name="word">词面</param>
    /// <param name="pinyins">每个汉字对应的拼音音节(长度须等于词中汉字数)</param>
    /// <param name="useC1">true = c1 完整形式; false = c0 缩写形式</param>
    /// <param name="path">输出路径(含结尾 0x00)</param>
    public static bool TryBuildPath(string word, IReadOnlyList<string> pinyins, bool useC1, out byte[] path)
    {
        path = [];
        var buf = new List<byte>(word.Length * 3 + 1);
        var pi = 0;

        foreach (var ch in word)
        {
            if (ch < 128)
            {
                // ASCII 可打印字符: 直接用小写 ASCII(实测 42 个字符 0 例外)
                buf.Add(ch >= 'A' && ch <= 'Z' ? (byte)(ch + 32) : (byte)ch);
                continue;
            }

            if (pi >= pinyins.Count)
                return false;
            var ok = useC1
                ? TryEncodeC1(pinyins[pi], out var code)
                : TryEncodeC0(pinyins[pi], out code);
            if (!ok)
                return false;
            buf.AddRange(code);
            pi++;
        }

        buf.Add(0x00);
        path = buf.ToArray();
        return true;
    }
}
