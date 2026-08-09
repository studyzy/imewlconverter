using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

namespace ImeWlConverter.Abstractions.Contracts;

/// <summary>
/// Orchestrates the full conversion pipeline: Import → Filter → Transform → Export.
/// </summary>
public interface IConversionPipeline
{
    /// <summary>Execute the conversion pipeline.</summary>
    Task<Result<ConversionResult>> ExecuteAsync(
        ConversionRequest request,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken ct = default);
}

/// <summary>A conversion request specifying input/output and options.</summary>
public sealed record ConversionRequest
{
    /// <summary>Input format identifier (e.g., "scel", "ggpy").</summary>
    public required string InputFormatId { get; init; }

    /// <summary>Output format identifier.</summary>
    public required string OutputFormatId { get; init; }

    /// <summary>Input file paths.</summary>
    public required IReadOnlyList<string> InputPaths { get; init; }

    /// <summary>Output file path. When null, output is written to OutputStream.</summary>
    public string? OutputPath { get; init; }

    /// <summary>Output stream for GUI scenarios (preview before save). Mutually exclusive with OutputPath.</summary>
    public Stream? OutputStream { get; init; }

    /// <summary>Whether to merge all input files into one output (default true).</summary>
    public bool MergeToOneFile { get; init; } = true;

    /// <summary>Output directory for per-file export mode (when MergeToOneFile is false).</summary>
    public string? OutputDirectory { get; init; }

    /// <summary>Conversion options.</summary>
    public ConversionOptions Options { get; init; } = new();

    /// <summary>Filter configuration. When set, overrides any externally provided FilterPipeline.</summary>
    public FilterConfig? FilterConfig { get; init; }
}

/// <summary>Result of a complete conversion.</summary>
public sealed record ConversionResult
{
    /// <summary>Total entries imported.</summary>
    public int ImportedCount { get; init; }

    /// <summary>Total entries exported after filtering.</summary>
    public int ExportedCount { get; init; }

    /// <summary>Entries filtered out.</summary>
    public int FilteredCount { get; init; }

    /// <summary>Export content as string (for GUI display). Only populated when OutputStream is used.</summary>
    public string? ExportContent { get; init; }

    /// <summary>Export data for binary formats. Only populated when OutputStream is used.</summary>
    public byte[]? ExportData { get; init; }

    /// <summary>Accumulated error messages from individual file processing.</summary>
    public string? ErrorMessages { get; init; }
}
