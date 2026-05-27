namespace ImeWlConverter.Formats.Wubi;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Wubi98 dictionary exporter. Format: "code word".</summary>
[FormatPlugin("wb98", "五笔98版", 220)]
public sealed partial class Wubi98Exporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override string? FormatEntry(WordEntry entry)
    {
        var code = entry.Code?.GetPrimaryCode("") ?? "";
        if (string.IsNullOrEmpty(code))
            return null;

        return $"{code} {entry.Word}";
    }
}
