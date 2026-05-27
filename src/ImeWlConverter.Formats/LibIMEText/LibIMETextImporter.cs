namespace ImeWlConverter.Formats.LibIMEText;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>LibIME Text dictionary importer. Format: pinyin word rank</summary>
[FormatPlugin("libimetxt", "LibIME 拼音词库（文本格式）", 500)]
public sealed partial class LibIMETextImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split(' ');
        if (parts.Length < 3)
            yield break;

        var pinyinParts = parts[0].Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
        var word = parts[1];
        var rank = int.TryParse(parts[2], out var r) ? r : 0;

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
