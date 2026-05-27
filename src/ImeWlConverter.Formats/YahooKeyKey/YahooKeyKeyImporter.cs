namespace ImeWlConverter.Formats.YahooKeyKey;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Yahoo KeyKey (雅虎奇摩) dictionary importer. Tab-separated: word, zhuyin, score1, score2.</summary>
[FormatPlugin("yahoo", "雅虎奇摩", 200)]
public sealed partial class YahooKeyKeyImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;
    protected override bool IsContentLine(string line)
        => line.Split('\t').Length == 4;

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 4)
            yield break;

        var word = parts[0];
        var zhuyin = parts[1];

        // Zhuyin codes are comma-separated
        var codes = zhuyin.Split(',', StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            CodeType = CodeType.Zhuyin,
            Code = WordCode.FromSingle(codes)
        };
    }
}
