namespace ImeWlConverter.Abstractions.Models;

/// <summary>
/// Metadata describing an IME dictionary format.
/// </summary>
/// <param name="Id">Short identifier code (e.g., "scel", "ggpy").</param>
/// <param name="DisplayName">Human-readable display name.</param>
/// <param name="SortOrder">Sort order for UI display.</param>
/// <param name="SupportsImport">Whether this format supports importing.</param>
/// <param name="SupportsExport">Whether this format supports exporting.</param>
/// <param name="IsBinary">Whether this format uses binary encoding.</param>
/// <param name="FileExtension">Default file extension including the leading dot (e.g., ".txt", ".scel").</param>
public sealed record FormatMetadata(
    string Id,
    string DisplayName,
    int SortOrder,
    bool SupportsImport,
    bool SupportsExport,
    bool IsBinary = false,
    string FileExtension = ".txt");
