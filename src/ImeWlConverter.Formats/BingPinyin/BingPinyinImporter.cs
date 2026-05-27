namespace ImeWlConverter.Formats.BingPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Bing Pinyin dictionary importer (text format). Format: word py1 py2 ...</summary>
[FormatPlugin("bing", "必应输入法", 135)]
public sealed partial class BingPinyinImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override bool IsContentLine(string line) =>
        !string.IsNullOrWhiteSpace(line) && !line.StartsWith(";");

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var sp = line.Split(' ');
        if (sp.Length < 2)
            yield break;

        var word = sp[0];
        var py = new string[word.Length];
        for (var i = 0; i < word.Length && i + 1 < sp.Length; i++)
            py[i] = sp[i + 1];

        yield return new WordEntry
        {
            Word = word,
            Rank = 1,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(py)
        };
    }
}
