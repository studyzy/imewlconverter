namespace ImeWlConverter.Formats.SougouPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Sougou Pinyin dictionary importer (text format).</summary>
[FormatPlugin("sgpy", "搜狗拼音txt", 10)]
public sealed partial class SougouPinyinImporter : TextFormatImporter
{

    protected override Encoding FileEncoding => Encoding.GetEncoding("GBK");
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        if (line.IndexOf("'") != 0)
            yield break;

        var parts = line.Split(' ');
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
