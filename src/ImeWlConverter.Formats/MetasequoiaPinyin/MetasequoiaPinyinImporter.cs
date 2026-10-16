namespace ImeWlConverter.Formats.MetasequoiaPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// 水杉输入法（MetasequoiaIME）全拼词库导入器，读取水杉「导出」得到的
/// 词语\t全拼[\t权重] 文本（如「水杉IME-拼音用户词库.txt」）。
/// </summary>
[FormatPlugin("sspy", "水杉输入法（全拼）", 510)]
public sealed partial class MetasequoiaPinyinImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;

    protected override bool IsContentLine(string line) =>
        !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#');

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        // 与水杉一致：只接受 2 列或 3 列，各列仅去除首尾空格。
        var parts = line.Split('\t');
        if (parts.Length is not (2 or 3))
            yield break;

        var word = parts[0].Trim(' ');
        var pinyinParts = parts[1].Trim(' ').ToLowerInvariant()
            .Split('\'', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (word.Length == 0 || pinyinParts.Length == 0)
            yield break;

        var rank = parts.Length == 3 && int.TryParse(parts[2].Trim(' '), out var r) && r >= 0 ? r : 0;

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
