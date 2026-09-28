using ImeWlConverter.Abstractions.Enums;

namespace ImeWlConverter.Abstractions.Options;

/// <summary>Options for format export operations.</summary>
public sealed class ExportOptions
{
    /// <summary>The target code type for the output.</summary>
    public CodeType TargetCodeType { get; init; } = CodeType.Pinyin;

    /// <summary>Text encoding name for text-based formats.</summary>
    public string? EncodingName { get; init; }

    /// <summary>Sort type for the output.</summary>
    public SortType SortType { get; init; } = SortType.Default;

    /// <summary>Whether to sort descending.</summary>
    public bool SortDescending { get; init; }

    /// <summary>词库编号（如 scel 格式内嵌的文件 ID，不设置则随机生成）。</summary>
    public string? DictionaryId { get; init; }

    /// <summary>词库名称（如 scel 格式内嵌的元数据，不设置则使用格式默认值）。</summary>
    public string? DictionaryName { get; init; }

    /// <summary>词库类别（如 scel 格式内嵌的元数据，不设置则使用格式默认值）。</summary>
    public string? DictionaryCategory { get; init; }

    /// <summary>词库描述（如 scel 格式内嵌的元数据，不设置则使用格式默认值）。</summary>
    public string? DictionaryDescription { get; init; }
}
