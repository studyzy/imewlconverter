namespace ImeWlConverter.Formats.ZiGuangPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>ZiGuang (Huayu) Pinyin dictionary importer (text format). Format: word\tpinyin</summary>
[FormatPlugin("zgpy", "华宇紫光拼音", 170)]
public sealed partial class ZiGuangPinyinImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 2)
            yield break;

        var word = parts[0];
        var py = parts[1];
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
