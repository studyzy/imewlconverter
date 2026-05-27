namespace ImeWlConverter.Formats.FitInput;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>FIT Input dictionary exporter (text format). Format: pinyin,word</summary>
[FormatPlugin("fit", "FIT输入法", 140)]
public sealed partial class FitInputExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    protected override string LineEnding => "\n";
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{pinyin},{entry.Word}";
    }
}
