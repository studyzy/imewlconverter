using ImeWlConverter.Abstractions.Models;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// 词条编码有效性判定（管道内部共享谓词）。
/// 合并导出与逐文件导出两条路径必须使用同一判定，避免行为分歧。
/// </summary>
internal static class CodePredicates
{
    /// <summary>
    /// 判断词条是否拥有有效编码：必须有 Code，至少一个 segment，
    /// 且至少一个 segment 中存在非空编码串。
    /// </summary>
    public static bool HasValidCode(WordEntry entry) =>
        entry.Code is not null &&
        entry.Code.Segments.Count > 0 &&
        entry.Code.Segments.Any(s => s.Count > 0 && s.Any(c => !string.IsNullOrEmpty(c)));
}
