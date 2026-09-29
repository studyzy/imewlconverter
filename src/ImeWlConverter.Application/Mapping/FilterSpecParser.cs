using ImeWlConverter.Abstractions.Options;

namespace ImeWlConverter.Application.Mapping;

/// <summary>
/// 过滤串解析器："-f \"len:1-100|rm:eng|rm:num\"" → FilterConfig。
/// Try 模式：非法输入返回 false 并给出错误说明（不抛异常）。
/// </summary>
public static class FilterSpecParser
{
    public static bool TryParse(string? spec, out FilterConfig? config, out string? error)
    {
        config = null;
        error = null;
        if (string.IsNullOrEmpty(spec)) return true;

        var result = new FilterConfig();

        foreach (var part in spec.Split('|'))
        {
            if (part.StartsWith("len:"))
            {
                var range = part[4..].Split('-');
                if (!TryParseRange(range, part, 9999, out var length, out error))
                    return false;
                (result.WordLengthFrom, result.WordLengthTo) = length;
            }
            else if (part.StartsWith("rank:"))
            {
                var range = part[5..].Split('-');
                if (!TryParseRange(range, part, 999999, out var rank, out error))
                    return false;
                (result.WordRankFrom, result.WordRankTo) = rank;
            }
            else if (part == "rm:eng") result.IgnoreEnglish = true;
            else if (part == "rm:num") result.IgnoreNumber = true;
            else if (part == "rm:space") result.IgnoreSpace = true;
            else if (part == "rm:pun") result.IgnorePunctuation = true;
            else
            {
                error = $"无效的过滤参数: \"{part}\"（支持 len:min-max、rank:min-max、rm:eng、rm:num、rm:space、rm:pun）";
                return false;
            }
        }

        config = result;
        return true;
    }

    /// <summary>解析 "min" 或 "min-max" 形式的数字区间。</summary>
    private static bool TryParseRange(string[] range, string originalPart, int defaultMax,
        out (int Min, int Max) value, out string? error)
    {
        value = default;
        error = null;
        if (!int.TryParse(range[0], out var min))
        {
            error = $"无效的过滤参数: \"{originalPart}\"（应为 min 或 min-max 形式的数字区间）";
            return false;
        }

        var max = defaultMax;
        if (range.Length > 1 && !int.TryParse(range[1], out max))
        {
            error = $"无效的过滤参数: \"{originalPart}\"（应为 min 或 min-max 形式的数字区间）";
            return false;
        }

        value = (min, max);
        return true;
    }
}
