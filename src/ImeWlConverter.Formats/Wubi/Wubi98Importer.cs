namespace ImeWlConverter.Formats.Wubi;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Wubi98 dictionary importer. Same format as Wubi86 but uses Wubi98 code type.</summary>
[FormatPlugin("wb98", "五笔98版", 220)]
public sealed partial class Wubi98Importer : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override bool IsContentLine(string line)
        => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#');

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        string[]? parts = null;

        if (line.Contains('\t'))
            parts = line.Split('\t');
        else if (line.Contains(' '))
            parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts == null || parts.Length < 2)
            yield break;

        var p0 = parts[0].Trim();
        var p1 = parts[1].Trim();

        string word, code;
        if (IsValidWubiCode(p0))
        {
            code = p0;
            word = p1;
        }
        else if (IsValidWubiCode(p1))
        {
            word = p0;
            code = p1;
        }
        else
        {
            yield break;
        }

        if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(code))
            yield break;

        yield return new WordEntry
        {
            Word = word,
            CodeType = CodeType.Wubi98,
            Code = WordCode.FromSingle(new[] { code })
        };
    }

    private static bool IsValidWubiCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length > 4)
            return false;
        foreach (var c in code)
        {
            if (c < 'a' || c > 'z')
                return false;
        }
        return true;
    }
}
