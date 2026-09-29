namespace ImeWlConverter.Abstractions.Results;

/// <summary>
/// 转换过程中的单条结构化错误（如单个输入文件导入失败）。
/// 供 CLI/GUI 展示与机器可读输出（--json）消费。
/// </summary>
public sealed record ConversionError(
    string FilePath,
    string Message,
    string? ExceptionType = null);
