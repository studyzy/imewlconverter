namespace ImeWlConverter.Application.Requests;

/// <summary>
/// CLI 端的转换参数 DTO（与 System.CommandLine 解耦，便于测试与复用）。
/// </summary>
public sealed record CliConversionOptions
{
    public required string InputFormatId { get; init; }
    public required string OutputFormatId { get; init; }
    public required string OutputPath { get; init; }
    public required IReadOnlyList<string> InputFiles { get; init; }
    public string? FilterSpec { get; init; }
    public string? CodeType { get; init; }
    public string? CustomFormat { get; init; }
    public string? CodeFile { get; init; }
    public string? MultiCode { get; init; }
    public string? DictId { get; init; }
    public string? DictName { get; init; }
    public string? DictCategory { get; init; }
    public string? DictDescription { get; init; }
}
