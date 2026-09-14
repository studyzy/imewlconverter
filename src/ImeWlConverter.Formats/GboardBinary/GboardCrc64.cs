namespace ImeWlConverter.Formats.GboardBinary;

/// <summary>
/// absl CRC64(A1 变体), Gboard 用于 DA-TRIE 块和 P-TABLE 块的校验和。
///
/// ⚠️ 初值是 **多项式本身**, 不是 0 —— 用 0 会导致校验和不匹配,
/// Gboard 会拒绝整份词典(症状: 词典被清空/打不出字)。
/// 已用官方 real/full / real/words 交叉验证: init=poly 匹配, init=0 不匹配。
/// <code>poly = 0xAB0547580DAAC74E, init = 0xAB0547580DAAC74E</code>
/// </summary>
internal static class GboardCrc64
{
    private const ulong Polynomial = 0xAB0547580DAAC74EUL;
    private static readonly ulong[] Table = BuildTable();

    private static ulong[] BuildTable()
    {
        var table = new ulong[256];
        for (var i = 0; i < 256; i++)
        {
            var v = (ulong)i;
            for (var bit = 0; bit < 8; bit++)
                v = (v >> 1) ^ ((v & 1) != 0 ? Polynomial : 0UL);
            table[i] = v;
        }
        return table;
    }

    public static ulong Compute(ReadOnlySpan<byte> data)
    {
        var crc = Polynomial;      // ⚠️ 初值 = 多项式(不是 0)
        foreach (var b in data)
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc;
    }
}
