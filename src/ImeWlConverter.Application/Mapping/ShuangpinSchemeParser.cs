namespace ImeWlConverter.Application.Mapping;

/// <summary>
/// 解析 CLI --shuangpin 选项：把 ASCII 别名或中文方案名规范化为
/// Shuangpin.txt 表头名；「全拼」/空表示关闭双拼转换。
/// </summary>
public static class ShuangpinSchemeParser
{
    /// <summary>Shuangpin.txt 表头中的方案名（第 1 列「全拼」除外）。</summary>
    public static readonly string[] Schemes =
    [
        "小鹤双拼", "自然码", "微软双拼", "智能ABC", "紫光拼音",
        "拼音加加", "星空键道", "大牛双拼", "小浪双拼"
    ];

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["full"] = "",
        ["quanpin"] = "",
        ["全拼"] = "",
        ["xiaohe"] = "小鹤双拼",
        ["小鹤"] = "小鹤双拼",
        ["ziranma"] = "自然码",
        ["自然码"] = "自然码",
        ["microsoft"] = "微软双拼",
        ["ms"] = "微软双拼",
        ["微软"] = "微软双拼",
        ["abc"] = "智能ABC",
        ["zhinengabc"] = "智能ABC",
        ["智能ABC"] = "智能ABC",
        ["ziguang"] = "紫光拼音",
        ["pyjj"] = "拼音加加",
        ["xingkong"] = "星空键道",
        ["daniu"] = "大牛双拼",
        ["xiaolang"] = "小浪双拼"
    };

    /// <summary>
    /// 解析方案名。<paramref name="scheme"/> 为 null 表示关闭（不转换），
    /// 非 null 为 Shuangpin.txt 表头名；返回 false 时 <paramref name="error"/> 给出可读提示。
    /// </summary>
    public static bool TryParse(string? spec, out string? scheme, out string? error)
    {
        scheme = null;
        error = null;

        if (string.IsNullOrWhiteSpace(spec))
            return true;

        var trimmed = spec.Trim();
        if (Aliases.TryGetValue(trimmed, out var mapped))
        {
            scheme = mapped.Length == 0 ? null : mapped;
            return true;
        }

        // 允许直接传表头名（GUI 下拉框文本即为表头名）
        if (Array.IndexOf(Schemes, trimmed) >= 0)
        {
            scheme = trimmed;
            return true;
        }

        error = $"未知双拼方案: \"{trimmed}\"（可用: full, xiaohe, ziranma, microsoft, abc, " +
                "ziguang, pyjj, xingkong, daniu, xiaolang，或 Shuangpin.txt 表头中文名）";
        return false;
    }
}
