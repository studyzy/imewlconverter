namespace ImeWlConverter.Formats.ShouXinPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>ShouXin Pinyin dictionary importer (text format). Format: word\tpinyin\trank</summary>
[FormatPlugin("sxpy", "手心输入法", 180)]
public sealed partial class ShouXinPinyinImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 3)
            yield break;

        var word = parts[0];
        var rank = int.TryParse(parts[2], out var r) ? r : 0;
        var pinyinParts = parts[1].Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
