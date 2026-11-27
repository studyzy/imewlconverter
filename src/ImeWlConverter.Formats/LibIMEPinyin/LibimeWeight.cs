namespace ImeWlConverter.Formats.LibIMEPinyin;

using System.Globalization;

/// <summary>
/// libime 词库权重 ↔ 本工具正整数词频的换算。
///
/// libime 的权重是对数尺度的浮点得分（直接加到解码器的语言模型得分上，通常仅为个位数，
/// 可以为负），而 Rime 等格式的词频是「上屏次数」正整数（可达数十万）。
/// 直接把词频写成权重会碾压用户词、造成词序抢占（issue #436）。
///
/// 采用固定的绝对映射，不按整份词库的 min/max 动态归一化：
///   weight = log10(max(rank, 1)) - log10(<see cref="BaselineRank"/>)
/// 这样拆分/合并词库时结果稳定；低于 <see cref="BaselineRank"/> 的低频词权重为负，
/// 即使全新安装、没有输入历史也不容易抢占词序。
/// </summary>
internal static class LibimeWeight
{
    /// <summary>权重为 0 对应的源词频；低于它的词条映射为负权重。</summary>
    public const int BaselineRank = 10;

    /// <summary>权重上下限，防止离群词频或原生词库中的异常权重失控。</summary>
    public const float MinWeight = -6f;
    public const float MaxWeight = 6f;

    /// <summary>正整数词频 → libime 浮点权重。</summary>
    public static float FromRank(int rank)
    {
        var weight = (float)(Math.Log10(Math.Max(rank, 1)) - Math.Log10(BaselineRank));
        return float.IsFinite(weight) ? Math.Clamp(weight, MinWeight, MaxWeight) : 0f;
    }

    /// <summary>libime 浮点权重 → 正整数词频（<see cref="FromRank"/> 的逆）。</summary>
    public static int ToRank(float weight)
    {
        if (!float.IsFinite(weight))
            return 0;

        var rank = Math.Pow(10, weight + Math.Log10(BaselineRank));
        return (int)Math.Clamp(Math.Round(rank), 1, int.MaxValue);
    }

    /// <summary>按 libime 文本格式输出权重（float32 最短可往返、区域无关）。</summary>
    public static string Format(float weight) => weight.ToString("R", CultureInfo.InvariantCulture);

    /// <summary>解析 libime 文本权重；非有限值视为无效（与 libime loadTextImpl 跳过非法行一致）。</summary>
    public static bool TryParse(string text, out float weight) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out weight)
        && float.IsFinite(weight);
}
