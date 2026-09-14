namespace ImeWlConverter.Formats.GboardBinary;

using System.Text;

/// <summary>
/// Gboard 二进制用户词典读取器 —— 反向解析 <c>user_dict_3_3</c>。
///
/// 查找链(与 Gboard 一致):
/// <code>
/// KEY 路径 → DA-trie 走到终端 → base/6 = F1 行号
///          → slot = (val&gt;&gt;1) | (slot_hi&lt;&lt;15), 长度 = kind
///          → P-TABLE[slot..slot+kind-1] → 条目序号 × 14 = FPT2 偏移
/// </code>
/// </summary>
internal static class GboardBinaryReader
{
    /// <summary>读出的一个词: 词面 + 每个汉字的拼音音节。</summary>
    internal sealed record DecodedWord(string Word, List<string> Pinyins, int Rank);

    public static List<DecodedWord> Read(byte[] d)
    {
        var da = IndexOf(d, "DA-TRIE\0"u8);
        if (da < 0)
            throw new InvalidDataException("不是 Gboard 词典: 找不到 DA-TRIE 块");

        var nodeCount = ReadU32(d, da + 28);
        var nodeBase = da + 16 + 536;

        var f1 = IndexOf(d, "FixedPhraseTable"u8);
        if (f1 < 0)
            throw new InvalidDataException("不是 Gboard 词典: 找不到 FPT1 块");
        var f1Count = ReadU32(d, f1 + 20);
        var f1Base = f1 + 28;

        var pt = IndexOf(d, "P-TABLE\0"u8);
        if (pt < 0)
            throw new InvalidDataException("不是 Gboard 词典: 找不到 P-TABLE 块");
        var slotCount = ReadU32(d, pt + 20);

        var f2 = IndexOf(d, "FixedPhraseTable"u8, f1 + 1);
        if (f2 < 0)
            throw new InvalidDataException("不是 Gboard 词典: 找不到 FPT2 块");
        var f2Count = ReadU32(d, f2 + 20);
        var f2Base = f2 + 28;

        // 一个词在 FPT2 里有多条(变体 + KEY + VAL), 必须取 KEY 条目的拼音。
        // 注意顺序: 变体条目通常排在 KEY 前面, 所以不能"首次见到就收下"。
        var pinyinOf = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var rankOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var order = new List<string>();

        for (var e = 0; e < f2Count; e++)
        {
            var off = f2Base + e * 14;
            var keyNode = ReadU32(d, off);
            var valNode = ReadU32(d, off + 4);
            var flags = ReadU32(d, off + 8) >> 24;

            if ((flags & 0x08) != 0)
                continue;                                    // VAL 条目, 不是词

            var valPath = PathOf(d, nodeBase, nodeCount, (int)valNode);
            if (valPath is null || valPath.Length == 0)
                continue;
            var word = DecodeText(valPath);
            if (string.IsNullOrEmpty(word))
                continue;

            if (!pinyinOf.ContainsKey(word))
            {
                pinyinOf[word] = [];
                rankOf[word] = 0;
                order.Add(word);
            }

            // KEY 条目的路径才是这个词的拼音(变体条目是缩写码或部分码),
            // 其 F1 字段即该词的词频(Gboard 用它决定候选顺序)。
            if ((flags & 0x40) != 0)
            {
                var keyPath = PathOf(d, nodeBase, nodeCount, (int)keyNode);
                if (keyPath is not null)
                    pinyinOf[word] = DecodePinyin(keyPath);
                var freq = (int)(ReadU32(d, off + 8) & 0xFFFFFF);
                if (freq > rankOf[word])
                    rankOf[word] = freq;
            }
        }

        var result = order
            .Select(w => new DecodedWord(w, pinyinOf[w], rankOf[w]))
            .ToList();

        _ = f1Count;
        _ = slotCount;
        _ = f1Base;
        return result;
    }

    /// <summary>沿 check 指针回到根, 还原节点路径。</summary>
    private static byte[]? PathOf(byte[] d, int nodeBase, uint nodeCount, int target)
    {
        var path = new List<byte>();
        var i = target;
        var guard = 0;

        while (i != 1 && guard++ < 4096)
        {
            if (i < 0 || i >= nodeCount)
                return null;
            var parent = ReadI32(d, nodeBase + i * 8 + 4);
            if (parent < 0 || parent >= nodeCount)
                return null;
            var parentBase = ReadI32(d, nodeBase + parent * 8);
            var ch = i - parentBase;
            if (ch is < 0 or > 255)
                return null;
            path.Add((byte)ch);
            i = parent;
        }

        if (i != 1)
            return null;
        path.Reverse();
        return path.ToArray();
    }

    /// <summary>VAL 路径 → 词面(UTF-8, 去掉结尾 0x00)。</summary>
    private static string DecodeText(byte[] path)
    {
        var len = path.Length;
        if (len > 0 && path[len - 1] == 0)
            len--;
        return Encoding.UTF8.GetString(path, 0, len);
    }

    /// <summary>KEY 路径 → 每个汉字的拼音音节(跳过 c0 缩写段与 ASCII 字面量)。</summary>
    private static List<string> DecodePinyin(byte[] path)
    {
        var result = new List<string>();
        var i = 0;

        while (i < path.Length)
        {
            var b = path[i];
            if (b == 0)
                break;
            if (b < 128)
            {
                i++;
                continue;
            }
            if (b != 0xEE || i + 2 >= path.Length)
                return result;
            if (path[i + 2] == 0x80)
            {
                i += 3;                                      // c0 缩写形式
                continue;
            }
            try
            {
                result.Add(GboardPuaCodec.Decode(path.AsSpan(i, 3)));
            }
            catch (ArgumentException)
            {
                return result;
            }
            i += 3;
        }

        return result;
    }

    private static int IndexOf(byte[] haystack, ReadOnlySpan<byte> needle, int start = 0)
    {
        for (var i = start; i + needle.Length <= haystack.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return i;
        }
        return -1;
    }

    private static uint ReadU32(byte[] d, int offset)
        => (uint)(d[offset] | (d[offset + 1] << 8) | (d[offset + 2] << 16) | (d[offset + 3] << 24));

    private static int ReadI32(byte[] d, int offset) => unchecked((int)ReadU32(d, offset));
}
