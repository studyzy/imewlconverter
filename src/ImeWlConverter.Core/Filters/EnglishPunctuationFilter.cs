using System.Text.RegularExpressions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;

namespace ImeWlConverter.Core.Filters;

/// <summary>
/// 英文标点过滤器：过滤掉包含英文标点符号的词条。
/// </summary>
public sealed partial class EnglishPunctuationFilter : IWordFilter
{
    // 正则逐项分析（C# 字符串中 \\ 在正则中为 \，如 \\^ → \^）：
    //
    //   [-,~.?:;'"!`(){}\[\]\^]  — 字符类，匹配单个英文标点（含括号类）
    //   |(-{2})                   — 双连字符 "--"
    //   |(\.{3})                  — 省略号 "..."
    [GeneratedRegex("[-,~.?:;'\"!`(){}\\[\\]\\^]|(-{2})|(\\.{3})", RegexOptions.Compiled)]
    private static partial Regex EnglishPunctuationRegex();

    /// <summary>
    /// 判断词条是否应保留。不含英文标点的词条返回 true，含英文标点的返回 false。
    /// </summary>
    public bool ShouldKeep(WordEntry entry) =>
        !EnglishPunctuationRegex().IsMatch(entry.Word);
}

/// <summary>
/// 英文标点移除变换器：移除词条中的英文标点符号。
/// </summary>
public sealed partial class EnglishPunctuationRemoveTransform : IWordTransform
{
    [GeneratedRegex("[-,~.?:;'\"!`(){}\\[\\]\\^]|(-{2})|(\\.{3})", RegexOptions.Compiled)]
    private static partial Regex EnglishPunctuationRegex();

    /// <summary>
    /// 移除词条中的英文标点。移除后词条为空则返回 null。
    /// </summary>
    public WordEntry? Transform(WordEntry entry)
    {
        var result = EnglishPunctuationRegex().Replace(entry.Word, "");
        return string.IsNullOrEmpty(result) ? null : entry with { Word = result };
    }
}
