namespace ImeWlConverter.Formats.UserPhrase;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>User-defined phrase exporter. Export only, default format: "{code},{rank}={word}".</summary>
[FormatPlugin("dy", "用户自定义短语", 110)]
public sealed partial class UserDefinePhraseExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;
    protected override string? FormatEntry(WordEntry entry)
    {
        var code = entry.Code?.GetPrimaryCode("") ?? "";
        if (string.IsNullOrEmpty(code))
            return null;

        var rank = entry.Rank == 0 ? 1 : entry.Rank;
        return $"{code},{rank}={entry.Word}";
    }
}
