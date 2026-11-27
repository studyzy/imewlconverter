namespace ImeWlConverter.Formats.Rime;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Rime dictionary importer (text format). Format: word\tcode\trank</summary>
[FormatPlugin("rime", "Rime中州韵", 150)]
public sealed partial class RimeImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override bool IsContentLine(string line) =>
        !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#");

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        // Rime 允许「只有词条、没有编码」的行（由输入法自动注音）。这类行只有一个字段，
        // 早期实现直接丢弃导致词条静默丢失；此处保留词条并以空编码交给管道自动补拼音。
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
            yield break;

        // .dict.yaml 的 YAML 头（--- / name: ... / version: ... / ...）同样只有一列，
        // 必须排除，否则会被当成词条灌入词库。仅接受含汉字的单列行。
        if (parts.Length == 1 && !HasCjkCharacter(parts[0]))
            yield break;

        var word = parts[0];
        var code = parts.Length >= 2 ? parts[1] : "";
        var rank = parts.Length >= 3 && int.TryParse(parts[2], out var r) ? r : 0;
        var codeParts = code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(codeParts)
        };
    }

    private static bool HasCjkCharacter(string text)
    {
        foreach (var c in text)
        {
            if (c is >= '\u3400' and <= '\u9FFF' or >= '\uF900' and <= '\uFAFF' or '\u3007')
                return true;
        }
        return false;
    }
}
