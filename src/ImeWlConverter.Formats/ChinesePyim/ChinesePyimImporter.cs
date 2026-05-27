namespace ImeWlConverter.Formats.ChinesePyim;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Chinese-pyim dictionary importer (text format). Format: pin-yin word1 word2 ...</summary>
[FormatPlugin("pyim", "Chinese-pyim", 177)]
public sealed partial class ChinesePyimImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;
    protected override bool IsContentLine(string line) =>
        !string.IsNullOrWhiteSpace(line) && !line.StartsWith(";");

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var array = line.Split(' ');
        if (array.Length < 2)
            yield break;

        var py = array[0].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 1; i < array.Length; i++)
        {
            var word = array[i];
            if (string.IsNullOrWhiteSpace(word))
                continue;

            yield return new WordEntry
            {
                Word = word,
                Rank = 1,
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(py)
            };
        }
    }
}
