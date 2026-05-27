using System.Text;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

namespace ImeWlConverter.Abstractions.Contracts;

/// <summary>
/// Contract for exporting word entries to an IME dictionary format.
/// </summary>
public interface IFormatExporter
{
    /// <summary>Metadata describing this format.</summary>
    FormatMetadata Metadata { get; }

    /// <summary>The encoding used when writing to a stream.</summary>
    Encoding OutputEncoding => Encoding.UTF8;

    /// <summary>Export word entries to a stream.</summary>
    Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries,
        Stream output,
        ExportOptions? options = null,
        CancellationToken ct = default);
}
