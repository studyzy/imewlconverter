namespace ImeWlConverter.Formats.RimeUserDb;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Rime UserDb dictionary importer. Import only, format: "code\tword\trank".</summary>
[FormatPlugin("rimedb", "Rime UserDb 用户词典", 150)]
public sealed partial class RimeUserDbImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 2)
            yield break;

        var code = parts[0];
        var word = parts[1];
        // pinyin codes are space-separated
        var pinyinParts = code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
