namespace ImeWlConverter.Formats.FitInput;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>FIT Input dictionary importer (text format). Format: pinyin,word</summary>
[FormatPlugin("fit", "FIT输入法", 140)]
public sealed partial class FitInputImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split(',');
        if (parts.Length < 2)
            yield break;

        var py = parts[0];
        var word = parts[1];
        var pinyinParts = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            Rank = 1,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
