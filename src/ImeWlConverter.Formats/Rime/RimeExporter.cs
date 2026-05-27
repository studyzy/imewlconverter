namespace ImeWlConverter.Formats.Rime;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Formats.Shared;

/// <summary>Rime dictionary exporter (text format). Format: word\tcode\trank, sorted by rank descending.</summary>
[FormatPlugin("rime", "Rime中州韵", 150)]
public sealed partial class RimeExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    protected override string LineEnding => "\n";
    protected override string? FormatEntry(WordEntry entry)
    {
        var code = entry.Code?.GetPrimaryCode(" ") ?? "";
        if (string.IsNullOrEmpty(code))
            return null;
        return $"{entry.Word}\t{code}\t{entry.Rank}";
    }

    public override async Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        // Sort by rank descending before export
        var sorted = entries.OrderByDescending(e => e.Rank).ToList();
        return await base.ExportAsync(sorted, output, options, ct);
    }
}
