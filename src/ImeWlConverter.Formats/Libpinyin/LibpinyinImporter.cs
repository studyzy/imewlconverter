namespace ImeWlConverter.Formats.Libpinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Libpinyin dictionary importer (text format). Format: word pinyin</summary>
[FormatPlugin("libpy", "libpinyin", 175)]
public sealed partial class LibpinyinImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var sp = line.Split(' ');
        if (sp.Length < 2)
            yield break;

        var word = sp[0];
        var py = sp[1];
        var pinyinParts = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            Rank = 0,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
