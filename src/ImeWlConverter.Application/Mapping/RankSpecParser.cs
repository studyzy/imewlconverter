namespace ImeWlConverter.Application.Mapping;

/// <summary>
/// 解析 CLI -r/--rank-generator 词频选项。
/// 旧版 -r:数字 语义：指定固定词频并强制覆盖所有词条（DefaultWordRankGenerator.ForceOverride=true）。
/// 旧版的 baidu/google 在线词频生成器已随重构移除，仅保留固定数字词频。
/// </summary>
public static class RankSpecParser
{
    public static bool TryParse(string? spec, out int fixedRank, out string? error)
    {
        fixedRank = 0;
        error = null;

        if (string.IsNullOrWhiteSpace(spec))
        {
            error = "无效的词频生成器: \"\"（请指定固定词频数字，例如 -r 100）";
            return false;
        }

        var trimmed = spec.Trim();
        if (!int.TryParse(trimmed, out fixedRank) || fixedRank <= 0)
        {
            error = $"无效的词频生成器: \"{trimmed}\"" +
                    "（百度/谷歌在线词频生成器已移除，当前仅支持固定词频数字，例如 -r 100）";
            return false;
        }

        return true;
    }
}
