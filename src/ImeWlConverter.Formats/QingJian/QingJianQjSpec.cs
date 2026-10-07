namespace ImeWlConverter.Formats.QingJian;

using System.Buffers.Binary;
using System.Text;

/// <summary>
/// 青简 .qj 二进制容器（Dictionary kind）的字节级规格。
/// 依据 qingjian 源码 crates/qingjian-format 与 crates/qingjian-dictionary：
/// 文件 = 32 字节头 + 分节表（每节 24 字节）+ 各分节正文（8 字节对齐，填充全零），全部小端。
/// </summary>
internal static class QingJianQjSpec
{
    public static readonly byte[] Magic = "QINGJIAN"u8.ToArray();

    /// <summary>格式版本，读方精确匹配，不兼容时上游加一。</summary>
    public const ushort FormatVersion = 1;

    /// <summary>数据种类 1 = 拼音词库（Kind::Dictionary）。</summary>
    public const ushort KindDictionary = 1;

    public const int HeaderSize = 32;
    public const int SectionEntrySize = 24;
    public const int SectionAlign = 8;

    public static readonly byte[] MetaTag = "META"u8.ToArray();
    public static readonly byte[] TextTag = "TEXT"u8.ToArray();
    public static readonly byte[] KeysTag = "KEYS"u8.ToArray();
    public static readonly byte[] IndexTag = "INDX"u8.ToArray();
    public static readonly byte[] SlotsTag = "SLOT"u8.ToArray();

    public const int SlotSize = 12;   // text_start u32 + frequency u32 + text_len u16 + reserved u16
    public const int KeyIndexSize = 16; // key_start u32 + first_slot u32 + slot_count u32 + key_len u16 + reserved u16

    /// <summary>单个拼音音节的规范形式：青简以 v 表示 ü，lue/nue 统一为 lve/nve（按音节精确匹配）。</summary>
    public static string CanonicalizeSyllable(string syllable) => syllable switch
    {
        "lue" => "lve",
        "nue" => "nve",
        _ => syllable
    };

    /// <summary>青简词库键的规范形式：v 表示 ü，lue/nue 统一为 lve/nve（TSV 与 .qj 通用）。</summary>
    public static string CanonicalizePinyin(string pinyin)
    {
        var syllables = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < syllables.Length; i++)
            syllables[i] = CanonicalizeSyllable(syllables[i]);
        return string.Join(' ', syllables);
    }

    /// <summary>TOML 基本字符串转义（META 分节用）。</summary>
    public static string EscapeToml(string value) => value
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t");

    /// <summary>从分节表定位分节；缺失或越界视为文件损坏。</summary>
    public static (int Offset, int Length) FindSection(
        IReadOnlyDictionary<string, (int Offset, int Length)> sections, byte[] tag)
    {
        var key = Encoding.ASCII.GetString(tag);
        if (!sections.TryGetValue(key, out var section))
            throw new InvalidDataException($".qj 文件缺少分节 {key}，文件已损坏或不完整");
        return section;
    }

    public static uint ReadUInt32(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
}
