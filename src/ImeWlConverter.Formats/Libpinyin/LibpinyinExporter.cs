namespace ImeWlConverter.Formats.Libpinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Libpinyin dictionary exporter (text format). Format: word pinyin</summary>
[FormatPlugin("libpy", "libpinyin", 175)]
public sealed partial class LibpinyinExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    protected override string LineEnding => "\n";
    protected override string? FormatEntry(WordEntry entry)
    {
        // Libpinyin format excludes single-character entries
        if (entry.Word.Length < 2)
            return null;
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{entry.Word} {pinyin}";
    }
}
