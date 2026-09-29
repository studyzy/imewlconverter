namespace ImeWlConverter.CodeData;

/// <summary>
/// 注音符号表（源自 Zhuyin.txt）：拼音 ↔ 注音 双向映射。
/// </summary>
public interface IZhuyinTable
{
    /// <summary>拼音（可含音调数字后缀）→ 注音符号；查不到返回 null。</summary>
    string? GetZhuyin(string pinyin);

    /// <summary>注音符号 → 不含音调的拼音；查不到返回 null。</summary>
    string? GetPinyin(string zhuyin);
}
