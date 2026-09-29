namespace ImeWlConverter.Application.Mapping;

/// <summary>
/// 自定义格式 spec（如 "213 ,nyyy"）解析结果。
/// 语义：3 位字段顺序 + 拼音分隔符 + 字段分隔符 + 分隔符位置 + 显示开关。
/// </summary>
public sealed record SelfDefiningSpec(
    string OrderSpec,
    char PinyinSeparator,
    char FieldSeparator,
    bool ShowPinyin,
    bool ShowWord,
    bool ShowRank);

/// <summary>
/// 自定义格式 spec 解析器（Try 模式）。
/// </summary>
public static class SelfDefiningSpecParser
{
    public static bool TryParse(string? spec, out SelfDefiningSpec? value, out string? error)
    {
        value = null;
        error = null;
        if (string.IsNullOrEmpty(spec)) return true;

        if (spec.Length < 7)
        {
            error = $"无效的自定义格式: \"{spec}\"（至少需要 7 个字符，如 \"213 ,nyyy\"）";
            return false;
        }

        value = new SelfDefiningSpec(
            OrderSpec: spec[..3],
            PinyinSeparator: spec[3],
            FieldSeparator: spec[4],
            // spec[5] = position indicator (l/r/b/n) - 两侧包含语义，导入/导出实现各自解释
            ShowPinyin: spec.Length > 6 && spec[6] == 'y',
            ShowWord: spec.Length > 7 && spec[7] == 'y',
            ShowRank: spec.Length > 8 && spec[8] == 'y');
        return true;
    }
}
