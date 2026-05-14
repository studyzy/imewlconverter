namespace ImeWlConverter.Abstractions;

/// <summary>
/// Marks a class as an IME format plugin for compile-time registration via Source Generator.
/// The Source Generator reads these values to auto-generate the <c>Metadata</c> property.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FormatPluginAttribute : Attribute
{
    /// <summary>Short identifier code (e.g., "scel", "ggpy", "rime").</summary>
    public string Id { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Sort order for UI display.</summary>
    public int SortOrder { get; }

    /// <summary>Whether this format uses binary encoding (auto-detected from base class if not set).</summary>
    public bool IsBinary { get; set; }

    /// <summary>Default file extension including the leading dot (e.g., ".txt", ".scel"). Defaults to ".txt".</summary>
    public string FileExtension { get; set; } = ".txt";

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatPluginAttribute"/> class.
    /// </summary>
    /// <param name="id">Short identifier code.</param>
    /// <param name="displayName">Human-readable display name.</param>
    /// <param name="sortOrder">Sort order for UI display (default: 100).</param>
    public FormatPluginAttribute(string id, string displayName, int sortOrder = 100)
    {
        Id = id;
        DisplayName = displayName;
        SortOrder = sortOrder;
    }
}
