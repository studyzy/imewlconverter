namespace ImeWlConverter.Formats.GboardBinary;

/// <summary>
/// 词面中的一个"单元": ASCII 字符直接作字面量;
/// 非 ASCII 码点带候选读音, 每个候选是**音节序列**
/// (正常汉字 1 个音节; emoji 的拼音可能有多个, 如 🐊 对应 "e"+"yu")。
/// </summary>
internal sealed class GboardChar
{
    /// <summary>ASCII 字符(&lt; 128 时作为小写字面量写入路径)。</summary>
    public char Ascii { get; init; }

    /// <summary>非 ASCII 码点的候选读音(第一个为主读音; 多个用于多音字)。</summary>
    public IReadOnlyList<string[]> Pronunciations { get; init; } = [];

    public bool IsAscii => Ascii > 0 && Ascii < 128;
}

/// <summary>构建 Gboard user_dict_3_3 所需的一个词条。</summary>
internal sealed class GboardWord
{
    /// <summary>词面。</summary>
    public required string Word { get; init; }

    /// <summary>按码点展开的单元序列(ASCII 字符 + 非 ASCII 码点)。</summary>
    public required IReadOnlyList<GboardChar> Chars { get; init; }

    /// <summary>词频(越大越常用)。</summary>
    public int Rank { get; init; }
}

/// <summary>
/// Gboard <c>user_dict_3_3</c> 二进制词典构建器。
///
/// 结构(全部从官方词典实测反推, 并在 iOS / Android 真机验证):
/// <code>
/// [16B 头] magic 96a4cba7 | 0 | version=3 | 1
/// [proto0] field1 = Σ(所有 KEY 条目的 F1)
/// [proto1] field5 = FPT2 条目总数
/// "VariableValueLengthTrie" / "DATrie"
/// "DA-TRIE\0" + crc64 + 536B 结构 + 8×N 节点
/// FPT1(6B/行) + P-TABLE(4B/槽)
/// FPT2(14B/条)
/// </code>
///
/// 关键不变式:
/// <list type="number">
///   <item>P-TABLE 双索引: 每个 FPT2 条目同时出现在 <c>key_node</c> 链和 <c>val_node</c> 块里
///         → <c>F1.kind</c> = 引用该节点的条目总数</item>
///   <item>F1 行 6 字节 = [kind:u8][val:u16][slot_hi:u8][hash:u16],
///         槽号 <c>slot = (val>>1) | (slot_hi&lt;&lt;15)</c>(23 位)</item>
///   <item>DA-trie 终端节点 <c>base</c> = 6 × F1 行号</item>
///   <item>词表按 KEY 路径字节序排序</item>
///   <item>KEY 的 F1 = 词频(用户选中次数, 官方量级 1~140); 平局正常</item>
/// </list>
/// </summary>
internal static class GboardDictionaryBuilder
{
    /// <summary>表尾预留的空闲槽数。</summary>
    private const int ExtraFreeSlots = 13;

    /// <summary>
    /// FPT2 的 D 字段 = 词条最后修改日（1970-01-01 起的天数）。
    /// 与 PinyinIME 的 lmt 同源：那边把 [lmt:16][freq:16] 打包成一个 u32，
    /// 这边拆成 D（日期）+ F1（频次）。官方词典里变体停在导入那天、KEY 条目
    /// 要等用户选过才会变新，所以导入时统一写“今天”最贴近 Gboard 自己的行为。
    /// 用本地日期（与参考实现 gboard_build3.py 的 date.today() 一致）。
    /// </summary>
    private static ushort DefaultD =>
        (ushort)(DateTime.Now.Date - new DateTime(1970, 1, 1)).TotalDays;

    private const byte FlagVariant = 0xA0;
    private const byte FlagKey = 0xC0;
    private const byte FlagValue = 0xA8;

    private const int Fpt1RowSize = 6;
    private const int Fpt2RowSize = 14;

    /// <summary>
    /// F1 行的 kind 字段是 u8, 所以单个节点的链最多 255 条。
    ///
    /// 触发场景: 单字词的 var1 是 c0 缩写码(只标声母), 于是**同声母的所有字共享一个节点** ——
    /// 若词表包含上万个单字, 该节点会被引用上千次, 超出上限。
    /// 处理方式: 超限的**变体**节点放弃建链(变体是可选的), 保证 KEY/VAL 完整可用。
    /// </summary>
    private const int MaxChainLength = 255;

    /// <summary>536 字节 DA-TRIE 结构模板(前 8 字节与末尾 2×256 表固定, 中间两处由构建器回填)。</summary>
    private static readonly byte[] TrieTemplate =
    {
        0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
        0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
        0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B,
        0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B,
        0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B,
        0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B,
        0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B,
        0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B,
        0x6C, 0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B,
        0x7C, 0x7D, 0x7E, 0x7F, 0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B,
        0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B,
        0x9C, 0x9D, 0x9E, 0x9F, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB,
        0xAC, 0xAD, 0xAE, 0xAF, 0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB,
        0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB,
        0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB,
        0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB,
        0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB,
        0xFC, 0xFD, 0xFE, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
        0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B,
        0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B,
        0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B,
        0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B,
        0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B,
        0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B,
        0x6C, 0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B,
        0x7C, 0x7D, 0x7E, 0x7F, 0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B,
        0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B,
        0x9C, 0x9D, 0x9E, 0x9F, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB,
        0xAC, 0xAD, 0xAE, 0xAF, 0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB,
        0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB,
        0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB,
        0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB,
        0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB,
        0xFC, 0xFD, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00,    };

    /// <summary>构建完整词典。返回可直接写入 user_dict_3_3 的字节。</summary>
    public static byte[] Build(IReadOnlyList<GboardWord> words, ICollection<string>? warnings = null)
    {
        var items = new List<Item>(words.Count);
        foreach (var w in words)
        {
            if (TryMakeItem(w, warnings, out var item))
                items.Add(item);
        }

        if (items.Count == 0)
            throw new InvalidOperationException("没有可导出的词条(拼音缺失或词面为空)。");

        // Gboard 要求词表按 KEY 路径字节序排序
        items.Sort(static (a, b) => CompareBytes(a.KeyPath, b.KeyPath));

        return Serialize(items, warnings);
    }

    // ==================================================================
    // 单个词条的构建结果
    // ==================================================================
    private sealed class Item
    {
        public required string Word { get; init; }
        public required byte[] KeyPath { get; init; }                    // c1 全拼路径
        public required byte[] ValPath { get; init; }                    // 词面 UTF-8 + 0x00
        public required List<(string Name, byte[] Path)> Variants { get; init; }
        public int Rank { get; init; }
        public int F1 { get; set; }
    }

    private static bool TryMakeItem(GboardWord w, ICollection<string>? warnings, out Item item)
    {
        item = null!;
        var word = w.Word;
        if (string.IsNullOrEmpty(word) || w.Chars.Count == 0)
            return false;

        var keyPath = BuildPath(w.Chars, static (c, _) => 0, useC1: true);
        if (keyPath is null)
        {
            warnings?.Add($"跳过「{word}」: 拼音无法编码");
            return false;
        }

        // ---- 变体 ----
        // 官方规则: 含 ASCII 字符的词一律 0 变体; 纯 CJK 词按字长生成
        var cjkCount = w.Chars.Count(c => !c.IsAscii);
        var variants = new List<(string, byte[])>();
        if (cjkCount == w.Chars.Count)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pat in VariantPatterns(cjkCount))
            {
                var p = BuildMixedPath(w.Chars, pat);
                if (p is null || !seen.Add(Convert.ToHexString(p)))
                    continue;
                variants.Add(($"var{variants.Count + 1}", p));
            }
        }

        var valBytes = System.Text.Encoding.UTF8.GetBytes(word);
        var valPath = new byte[valBytes.Length + 1];
        Buffer.BlockCopy(valBytes, 0, valPath, 0, valBytes.Length);

        item = new Item
        {
            Word = word,
            KeyPath = keyPath,
            ValPath = valPath,
            Variants = variants,
            Rank = w.Rank,
        };
        return true;
    }

    /// <summary>
    /// 按单元序列构造路径。ASCII 单元写小写字节; 非 ASCII 单元写其读音的音节码。
    /// </summary>
    /// <param name="variantOf">给定单元与它的 CJK 序号, 返回选用第几个候选读音。</param>
    /// <param name="useC1">true = c1 完整码; false = c0 缩写码(由 variantOf 之外的开关控制)</param>
    private static byte[]? BuildPath(IReadOnlyList<GboardChar> chars, Func<GboardChar, int, int> variantOf, bool useC1)
    {
        var buf = new List<byte>(chars.Count * 3 + 1);
        var ci = 0;

        foreach (var ch in chars)
        {
            if (ch.IsAscii)
            {
                buf.Add(ch.Ascii is >= 'A' and <= 'Z' ? (byte)(ch.Ascii + 32) : (byte)ch.Ascii);
                continue;
            }

            var idx = variantOf(ch, ci);
            if (idx < 0 || idx >= ch.Pronunciations.Count)
                return null;

            foreach (var syllable in ch.Pronunciations[idx])
            {
                if (!(useC1
                        ? GboardPuaCodec.TryEncodeC1(syllable, out var code)
                        : GboardPuaCodec.TryEncodeC0(syllable, out code)))
                    return null;
                buf.AddRange(code);
            }
            ci++;
        }

        buf.Add(0x00);
        return buf.ToArray();
    }

    /// <summary>变体路径: pattern[ci] == 1 的汉字用 c1 完整码, 否则用 c0 缩写码。</summary>
    private static byte[]? BuildMixedPath(IReadOnlyList<GboardChar> chars, int[] pattern)
    {
        var buf = new List<byte>(chars.Count * 3 + 1);
        var ci = 0;

        foreach (var ch in chars)
        {
            if (ch.IsAscii)
            {
                buf.Add(ch.Ascii is >= 'A' and <= 'Z' ? (byte)(ch.Ascii + 32) : (byte)ch.Ascii);
                continue;
            }

            if (ch.Pronunciations.Count == 0)
                return null;

            var useC1 = pattern[ci] != 0;
            foreach (var syllable in ch.Pronunciations[0])
            {
                if (!(useC1
                        ? GboardPuaCodec.TryEncodeC1(syllable, out var code)
                        : GboardPuaCodec.TryEncodeC0(syllable, out code)))
                    return null;
                buf.AddRange(code);
            }
            ci++;
        }

        buf.Add(0x00);
        return buf.ToArray();
    }

    /// <summary>变体位掩码(0 = c0 缩写码, 1 = c1 完整码)。</summary>
    private static List<int[]> VariantPatterns(int n)
    {
        var list = new List<int[]>();
        if (n <= 0)
            return list;
        if (n == 1)
        {
            list.Add([0]);
            return list;
        }
        if (n == 2)
        {
            list.Add([0, 0]);
            list.Add([1, 0]);
            list.Add([0, 1]);
            return list;
        }

        var all0 = new int[n];
        list.Add(all0);
        var allButLast2 = new int[n];
        for (var i = 0; i < n - 2; i++) allButLast2[i] = 1;
        list.Add(allButLast2);
        var allButLast = new int[n];
        for (var i = 0; i < n - 1; i++) allButLast[i] = 1;
        list.Add(allButLast);
        var lastOnly = new int[n];
        lastOnly[n - 1] = 1;
        list.Add(lastOnly);
        return list;
    }

    private static bool TryBuildPatternPath(string word, string[] primary, int[] pattern, out byte[] path)
    {
        path = [];
        var buf = new List<byte>(word.Length * 3 + 1);
        var ci = 0;
        foreach (var ch in word)
        {
            if (ch < 128)
            {
                buf.Add(ch is >= 'A' and <= 'Z' ? (byte)(ch + 32) : (byte)ch);
                continue;
            }
            byte[] code;
            var ok = pattern[ci] != 0
                ? GboardPuaCodec.TryEncodeC1(primary[ci], out code)
                : GboardPuaCodec.TryEncodeC0(primary[ci], out code);
            if (!ok)
                return false;
            buf.AddRange(code);
            ci++;
        }
        buf.Add(0x00);
        path = buf.ToArray();
        return true;
    }

    // ==================================================================
    // 序列化
    // ==================================================================
    private static byte[] Serialize(List<Item> items, ICollection<string>? warnings)
    {
        var n = items.Count;

        // ---- KEY 的 F1 = 词频 ----
        // F1 就是 Gboard 记的「用户选中次数」：官方词典里绝大多数词是 1、最高约 140，
        // 用户每选一次 +1。所以导入时必须把词频保持在**这个量级**。
        // 早期版本按名次摊成 1..n 的大跨度，实测把 亟需 写成 F1=7775，
        // 而用户反复选中的 继续 才涨到 33 —— 候选顺序被永久冻死，再也纠不回来。
        // 平局是正常的：官方词典里 8619 个词同为 F1=1。
        var maxRank = 0;
        foreach (var it in items)
            if (it.Rank > maxRank) maxRank = it.Rank;
        foreach (var it in items)
        {
            if (it.Rank <= 0)
                it.F1 = 1;
            else if (maxRank <= 255)
                it.F1 = it.Rank;
            else
                it.F1 = Math.Max(1, (int)Math.Round(255.0 * it.Rank / maxRank));
        }

        // ---- 插入顺序: [var1, val, var2..varN, key, key2..] ----
        var insertOrder = new List<(int Item, string Name, byte[] Path)>();
        for (var i = 0; i < n; i++)
        {
            var it = items[i];
            if (it.Variants.Count > 0)
            {
                insertOrder.Add((i, it.Variants[0].Name, it.Variants[0].Path));
                insertOrder.Add((i, "val", it.ValPath));
                for (var v = 1; v < it.Variants.Count; v++)
                    insertOrder.Add((i, it.Variants[v].Name, it.Variants[v].Path));
            }
            else
            {
                insertOrder.Add((i, "val", it.ValPath));
            }
            insertOrder.Add((i, "key", it.KeyPath));
        }

        // ---- DA-trie ----
        var trie = new GboardDaTrie();
        trie.InitFree();
        trie.SetAlphabet(insertOrder.SelectMany(t => t.Path));

        foreach (var t in insertOrder)
        {
            if (trie.Lookup(t.Path) is null)
                trie.Insert(t.Path, 0);
        }

        // ⚠️ relocate 会搬移节点 → 必须全部插完后再统一解析终端节点号
        var term = new Dictionary<(int, string), long>();
        foreach (var t in insertOrder)
        {
            var node = trie.Lookup(t.Path)
                       ?? throw new InvalidOperationException($"trie 查找失败: {t.Name}");
            term[(t.Item, t.Name)] = node;
        }

        // ---- FPT2 条目 ----
        // 顺序: [变体.., key, val, key2..]
        var fpt2 = new List<(long KeyNode, long ValNode, uint Flag, ushort D)>();
        var entryOwner = new List<int>();      // 条目 → 词号
        var dropEntry = new HashSet<int>();
        var droppedWords = new HashSet<int>();
        var droppedVariants = 0;
        for (var i = 0; i < n; i++)
        {
            var it = items[i];
            var valNode = term[(i, "val")];
            var names = new List<string>();
            foreach (var (nm, _) in it.Variants)
                names.Add(nm);
            names.Add("key");
            names.Add("val");

            var secondary = it.F1 == 0 ? 0u : 1u;
            foreach (var nm in names)
            {
                uint c;
                if (nm.StartsWith("key", StringComparison.Ordinal))
                    c = ((uint)FlagKey << 24) | (uint)it.F1;
                else if (nm == "val")
                    c = ((uint)FlagValue << 24) | secondary;
                else
                    c = ((uint)FlagVariant << 24) | secondary;
                fpt2.Add((term[(i, nm)], valNode, c, DefaultD));
                entryOwner.Add(i);
            }
        }

        bool isVariantEntry(int e)
        {
            var f = fpt2[e].Flag >> 24;
            return (f & 0x40) == 0 && (f & 0x08) == 0;
        }

        // ---- 节点 → 引用它的条目(P-TABLE 双索引) ----
        // 先按"全部条目"统计, 找出超出 u8 上限的节点
        var nodeEntries = new Dictionary<long, List<int>>();
        foreach (var (e, t) in fpt2.Select((v, i) => (i, v)))
        {
            AddEntry(nodeEntries, t.KeyNode, e);
            if (t.ValNode != t.KeyNode)
                AddEntry(nodeEntries, t.ValNode, e);
        }

        void Rebuild()
        {
            var kept = new List<(long KeyNode, long ValNode, uint Flag, ushort D)>(fpt2.Count);
            var newOwner = new List<int>(fpt2.Count);
            for (var e = 0; e < fpt2.Count; e++)
            {
                if (dropEntry.Contains(e) || droppedWords.Contains(entryOwner[e]))
                    continue;
                kept.Add(fpt2[e]);
                newOwner.Add(entryOwner[e]);
            }
            fpt2 = kept;
            entryOwner = newOwner;
            dropEntry.Clear();

            nodeEntries = new Dictionary<long, List<int>>();
            for (var e = 0; e < fpt2.Count; e++)
            {
                var t = fpt2[e];
                AddEntry(nodeEntries, t.KeyNode, e);
                if (t.ValNode != t.KeyNode)
                    AddEntry(nodeEntries, t.ValNode, e);
            }
        }

        // 超长节点降级
        //
        // F1.kind 是 u8(≤255), 所以单个 trie 节点的链最多 255 条。
        // 两种超限来源:
        //   ① 变体节点: 单字词的 var1 是 c0 缩写码(只标声母), 同声母的字共享一个节点
        //   ② KEY 节点: 同音字太多(例如两万个单字里读 "yi" 的有 300+ 个)
        // ①可以丢变体(变体可选); ②只能丢弃整个词条 —— 按词频保留高频的。
        for (var round = 0; round < 64; round++)
        {
            var over = nodeEntries.Where(kv => kv.Value.Count > MaxChainLength).ToList();
            if (over.Count == 0)
                break;

            var changed = false;
            foreach (var (node, entries) in over)
            {
                // 该节点被哪些词引用(通过条目反查词号)
                var words = entries.Select(e => entryOwner[e]).Distinct().ToList();
                if (words.Count <= 1)
                    continue;                                   // 单个词自身超长, 无法通过丢词解决

                // 变体条目可以直接丢
                var variants = entries.Where(e => isVariantEntry(e)).ToList();
                if (variants.Count >= entries.Count - MaxChainLength)
                {
                    foreach (var e in variants)
                        dropEntry.Add(e);
                    droppedVariants += variants.Count;
                    changed = true;
                    continue;
                }

                // 否则按词频从低到高丢词, 直到链长降到上限
                var ordered = words.OrderBy(w => items[w].Rank).ThenBy(w => w).ToList();
                var excess = entries.Count - MaxChainLength;
                var dropped = 0;
                foreach (var w in ordered)
                {
                    if (dropped >= excess || words.Count - dropped <= 1)
                        break;
                    droppedWords.Add(w);
                    dropped += entries.Count(e => entryOwner[e] == w);
                }
                if (dropped > 0)
                    changed = true;
            }

            if (!changed)
                break;

            // 重建 fpt2 / nodeEntries
            Rebuild();
        }

        if (droppedWords.Count > 0 || droppedVariants > 0)
        {
            warnings?.Add(
                $"F1.kind 上限 255: 丢弃 {droppedWords.Count} 个词条(同音字过多, 已按词频保留高频)、" +
                $"{droppedVariants} 条变体。建议避免导入上万个单字。");
        }

        // ---- val 节点 → key 哈希(写入 F1 行的 hash 字段) ----
        var valHash = new Dictionary<long, ushort>();
        for (var i = 0; i < n; i++)
            valHash[term[(i, "val")]] = GboardKeyHash.Compute(items[i].Word);

        // ---- F1 行号 = 节点在 FPT2 中的首次出现顺序 ----
        var rowIdx = new Dictionary<long, int>();
        var rowOrder = new List<long>();
        foreach (var t in fpt2)
        {
            if (!rowIdx.ContainsKey(t.KeyNode)) { rowIdx[t.KeyNode] = rowOrder.Count; rowOrder.Add(t.KeyNode); }
            if (!rowIdx.ContainsKey(t.ValNode)) { rowIdx[t.ValNode] = rowOrder.Count; rowOrder.Add(t.ValNode); }
        }

        // ---- 链分配: 按节点号升序, 从槽 2 起紧凑排列 ----
        var chainStart = new Dictionary<long, int>();
        var cursor = 2;
        foreach (var nd in nodeEntries.Keys.OrderBy(x => x))
        {
            chainStart[nd] = cursor;
            cursor += nodeEntries[nd].Count;
        }
        var usedSlots = cursor;
        var slotCount = Math.Max(usedSlots + ExtraFreeSlots, usedSlots + 2);

        // ---- F1 行: [kind:u8][val:u16][slot_hi:u8][hash:u16] ----
        //   slot = (val >> 1) | (slot_hi << 15)   —— 23 位, 大词典必需
        foreach (var kv in nodeEntries)
        {
            if (kv.Value.Count > MaxChainLength)
                throw new InvalidOperationException(
                    $"节点 {kv.Key} 的链长 {kv.Value.Count} 超过 F1.kind 上限 {MaxChainLength} —— 无法编码。");
        }

        var f1Rows = new byte[rowOrder.Count * Fpt1RowSize];
        foreach (var (nd, entries) in nodeEntries)
        {
            var r = rowIdx[nd];
            var s = chainStart[nd];
            var h = valHash.TryGetValue(nd, out var hv) ? hv : (ushort)0;
            var off = r * Fpt1RowSize;
            var lo = (s & 0x7FFF) << 1;
            f1Rows[off + 0] = (byte)entries.Count;
            f1Rows[off + 1] = (byte)(lo & 0xFF);
            f1Rows[off + 2] = (byte)((lo >> 8) & 0xFF);
            f1Rows[off + 3] = (byte)((s >> 15) & 0xFF);
            f1Rows[off + 4] = (byte)(h & 0xFF);
            f1Rows[off + 5] = (byte)((h >> 8) & 0xFF);
        }

        // ---- DA-trie 终端节点 base = 6 × F1 行号 ----
        foreach (var (nd, r) in rowIdx)
            trie.Base[(int)nd] = (uint)(Fpt1RowSize * r);

        // ---- P-TABLE ----
        var pt = new uint[slotCount];
        foreach (var (nd, entries) in nodeEntries)
        {
            var start = chainStart[nd];
            for (var j = 0; j < entries.Count; j++)
                pt[start + j] = 0x80000000u | (uint)(entries[j] * Fpt2RowSize);
        }

        // ---- 空闲链表: 区域 [st, st+sz)
        //   slot[st]   = (下一区域起始槽 << 2) | min(sz, 3)
        //   slot[st+1] = 上一区域起始槽            (sz >= 2)
        //   slot[st+2] = 真实 sz                   (sz >= 3, 即低 2 位 == 3 时的逃逸)
        var regions = new List<(int Start, int Size)>();
        for (var s = 0; s < slotCount; s++)
        {
            if (pt[s] != 0)
                continue;
            if (regions.Count > 0 && s == regions[^1].Start + regions[^1].Size)
                regions[^1] = (regions[^1].Start, regions[^1].Size + 1);
            else
                regions.Add((s, 1));
        }
        if (regions.Count == 0)
        {
            regions.Add((slotCount, 2));
            slotCount += 2;
            Array.Resize(ref pt, slotCount);
        }
        var regionCount = regions.Count;
        for (var i = 0; i < regionCount; i++)
        {
            var (st, sz) = regions[i];
            var next = regions[(i + 1) % regionCount].Start;
            var prev = regions[(i - 1 + regionCount) % regionCount].Start;
            pt[st] = (uint)((next << 2) | (sz <= 2 ? sz : 3));
            if (sz >= 2)
                pt[st + 1] = (uint)prev;
            if (sz >= 3)
                pt[st + 2] = (uint)sz;
        }

        // ---- 拼装 ----
        var f1Sum = 0u;
        foreach (var t in fpt2)
            if (((t.Flag >> 24) & 0x40) != 0)
                f1Sum += t.Flag & 0xFFFFFF;

        using var ms = new MemoryStream();
        void Raw(ReadOnlySpan<byte> b) => ms.Write(b);
        void U32(uint v)
        {
            ms.WriteByte((byte)v);
            ms.WriteByte((byte)(v >> 8));
            ms.WriteByte((byte)(v >> 16));
            ms.WriteByte((byte)(v >> 24));
        }
        void U64(ulong v)
        {
            for (var i = 0; i < 8; i++)
                ms.WriteByte((byte)(v >> (8 * i)));
        }
        void Align8()
        {
            while ((ms.Length & 7) != 0)
                ms.WriteByte(0);
        }

        // ⚠️ 文件魔数是字节序列 96 A4 CB A7(不是 u32 0x96A4CBA7 —— 小端序会把字节倒过来)
        Raw([0x96, 0xA4, 0xCB, 0xA7]);
        U32(0); U32(3); U32(1);

        foreach (var proto in new[] { Proto0(f1Sum), Proto1((uint)fpt2.Count) })
        {
            U32((uint)proto.Length);
            Raw(proto);
            Align8();
        }

        Raw(U32Prefix("VariableValueLengthTrie"));
        Align8();
        Raw(U32Prefix("DATrie"));
        Align8();

        // DA-TRIE
        var trieStruct = (byte[])TrieTemplate.Clone();
        WriteU32(trieStruct, 8, (uint)rowOrder.Count);
        WriteU32(trieStruct, 12, (uint)trie.N);
        var nodeBlob = new byte[trie.N * 8];
        for (var i = 0; i < trie.N; i++)
        {
            WriteU32(nodeBlob, i * 8, trie.Base[i]);
            WriteU32(nodeBlob, i * 8 + 4, trie.Check[i]);
        }
        var trieBody = new byte[trieStruct.Length + nodeBlob.Length];
        Buffer.BlockCopy(trieStruct, 0, trieBody, 0, trieStruct.Length);
        Buffer.BlockCopy(nodeBlob, 0, trieBody, trieStruct.Length, nodeBlob.Length);
        Raw("DA-TRIE\0"u8);
        U64(GboardCrc64.Compute(trieBody));
        Raw(trieBody);
        Align8();

        // FPT1
        Raw(U32Prefix("FixedPhraseTable"));
        Align8();
        U32((uint)rowOrder.Count);
        U32(Fpt1RowSize);
        Raw(f1Rows);
        Align8();

        // P-TABLE
        var ptHead = new byte[8];
        WriteU32(ptHead, 0, 0);
        WriteU32(ptHead, 4, (uint)slotCount);
        var ptBody = new byte[8 + slotCount * 4];
        Buffer.BlockCopy(ptHead, 0, ptBody, 0, 8);
        for (var i = 0; i < slotCount; i++)
            WriteU32(ptBody, 8 + i * 4, pt[i]);
        Raw("P-TABLE\0"u8);
        U64(GboardCrc64.Compute(ptBody));
        Raw(ptBody);
        Align8();

        // FPT2
        Raw(U32Prefix("FixedPhraseTable"));
        Align8();
        U32((uint)fpt2.Count);
        U32(Fpt2RowSize);
        foreach (var (kn, vn, flag, d) in fpt2)
        {
            U32((uint)kn);
            U32((uint)vn);
            U32(flag);
            ms.WriteByte((byte)(d & 0xFF));
            ms.WriteByte((byte)(d >> 8));
        }

        // ⚠️ 文件总长度必须 8 字节对齐（官方文件全部如此）。
        // 不对齐 → Gboard 拒绝整份词典并把它清空。
        Align8();

        return ms.ToArray();
    }

    private static void AddEntry(Dictionary<long, List<int>> map, long node, int entry)
    {
        if (!map.TryGetValue(node, out var list))
            map[node] = list = [];
        list.Add(entry);
    }

    private static byte[] U32Prefix(string ascii)
    {
        var name = System.Text.Encoding.ASCII.GetBytes(ascii);
        var result = new byte[4 + name.Length];
        WriteU32(result, 0, (uint)name.Length);
        Buffer.BlockCopy(name, 0, result, 4, name.Length);
        return result;
    }

    private static void WriteU32(byte[] buf, int offset, uint value)
    {
        buf[offset + 0] = (byte)(value & 0xFF);
        buf[offset + 1] = (byte)((value >> 8) & 0xFF);
        buf[offset + 2] = (byte)((value >> 16) & 0xFF);
        buf[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    private static byte[] Varint(uint value)
    {
        var buf = new List<byte>(5);
        while (true)
        {
            var b = (byte)(value & 0x7F);
            value >>= 7;
            if (value != 0)
            {
                buf.Add((byte)(b | 0x80));
            }
            else
            {
                buf.Add(b);
                break;
            }
        }
        return buf.ToArray();
    }

    private static byte[] Proto0(uint f1Sum)
    {
        var buf = new List<byte> { 0x08 };
        buf.AddRange(Varint(f1Sum));
        buf.AddRange([0x10, 0x00, 0x20, 0x00]);
        return buf.ToArray();
    }

    private static byte[] Proto1(uint entryCount)
    {
        var buf = new List<byte> { 0x08, 0x06, 0x10, 0x00, 0x18, 0x00, 0x20, 0x01, 0x28 };
        buf.AddRange(Varint(entryCount));
        buf.AddRange([0x30, 0x00]);
        return buf.ToArray();
    }

    private static int CompareBytes(byte[] a, byte[] b)
    {
        var n = Math.Min(a.Length, b.Length);
        for (var i = 0; i < n; i++)
        {
            var d = a[i].CompareTo(b[i]);
            if (d != 0)
                return d;
        }
        return a.Length.CompareTo(b.Length);
    }
}
