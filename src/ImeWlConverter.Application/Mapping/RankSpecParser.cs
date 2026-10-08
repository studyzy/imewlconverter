namespace ImeWlConverter.Application.Mapping;

/// <summary>词频生成器模式。</summary>
public enum RankGeneratorMode
{
    /// <summary>固定词频数字，强制覆盖所有词条（DefaultWordRankGenerator.ForceOverride=true）。</summary>
    FixedRank,

    /// <summary>调用 LLM 在线生成词频（LlmWordRankGenerator，需提供 API Key）。</summary>
    Llm
}

/// <summary>
/// 解析 CLI -r/--rank-generator 词频选项。
/// 旧版 -r:数字 语义：指定固定词频并强制覆盖所有词条（DefaultWordRankGenerator.ForceOverride=true）。
/// 旧版的 baidu/google 在线词频生成器已随重构移除；现支持固定数字与 llm（LLM 词频生成）。
/// </summary>
public static class RankSpecParser
{
    public static bool TryParse(string? spec, out RankGeneratorMode mode, out int fixedRank, out string? error)
    {
        mode = RankGeneratorMode.FixedRank;
        fixedRank = 0;
        error = null;

        if (string.IsNullOrWhiteSpace(spec))
        {
            error = "无效的词频生成器: \"\"（请指定固定词频数字（例如 -r 100）或 llm）";
            return false;
        }

        var trimmed = spec.Trim();
        if (string.Equals(trimmed, "llm", StringComparison.OrdinalIgnoreCase))
        {
            mode = RankGeneratorMode.Llm;
            return true;
        }

        if (!int.TryParse(trimmed, out fixedRank) || fixedRank <= 0)
        {
            error = $"无效的词频生成器: \"{trimmed}\"" +
                    "（当前支持固定词频数字（例如 -r 100）或 llm（LLM 生成词频，需配合 --llm-key））";
            return false;
        }

        return true;
    }
}
