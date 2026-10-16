namespace ImeWlConverter.Formats.MetasequoiaPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// 水杉输入法（MetasequoiaIME）全拼词库导出器。格式：词语\t全拼[\t权重]，
/// 与水杉「设置 → 词库 → 全拼 → 批量导入」及其自身导出一致（UTF-8 带 BOM，LF 换行）。
/// </summary>
[FormatPlugin("sspy", "水杉输入法（全拼）", 510)]
public sealed partial class MetasequoiaPinyinExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(true);

    protected override string LineEnding => "\n";

    protected override string? FormatEntry(WordEntry entry)
    {
        var word = entry.Word;
        // 水杉按 Tab 分列、按行读取，词面里出现这些字符会把行拆坏。
        if (string.IsNullOrWhiteSpace(word) || word.IndexOfAny(['\t', '\r', '\n']) >= 0)
            return null;

        var segments = entry.Code?.Segments;
        if (segments == null || segments.Count == 0)
            return null;

        var syllables = new List<string>(segments.Count);
        foreach (var segment in segments)
        {
            var syllable = segment.Count > 0 ? NormalizeSyllable(segment[0]) : "";
            // 水杉只接受完整音节的全拼（不收简拼、声调、空音节）。
            if (syllable.Length == 0 || !syllable.All(c => c is >= 'a' and <= 'z'))
                return null;
            syllables.Add(syllable);
        }

        // 水杉要求「词面码点数 == 音节数」（count_han_chars 实为码点计数），否则整行导入失败。
        if (word.EnumerateRunes().Count() != syllables.Count)
            return null;

        var pinyin = string.Join("'", syllables);
        // 词频缺失时管道会补 1（如搜狗 scel），因此 ≤1 视为无词频，写默认权重。
        var weight = entry.Rank > 1 ? entry.Rank : DefaultWeight;
        return $"{word}\t{pinyin}\t{weight}";
    }

    /// <summary>无词频词条导出的权重。</summary>
    public const int DefaultWeight = 100;

    private static string NormalizeSyllable(string syllable)
    {
        var s = syllable.Trim().ToLowerInvariant().Replace('ü', 'v');
        // 水杉以 lve/nve 存储 ü 韵母。
        return s switch
        {
            "lue" => "lve",
            "nue" => "nve",
            _ => s
        };
    }
}
