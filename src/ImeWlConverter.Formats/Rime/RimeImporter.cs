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
        if (parts.Length < 2)
            yield break;

        var word = parts[0];
        var code = parts[1];
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
}
