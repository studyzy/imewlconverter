namespace ImeWlConverter.Application.Cli;

/// <summary>
/// CLI 结构化错误：机器可读的 error.code + 可选的目标参数 + 人读消息。
/// </summary>
public sealed record CliError(string Code, string Message, string? Target = null)
{
    public const string MissingOption = "missing-option";
    public const string UnknownFormat = "unknown-format";
    public const string InvalidFilter = "invalid-filter";
    public const string InvalidSpec = "invalid-spec";
    public const string InvalidRank = "invalid-rank";
    public const string InputNotFound = "input-not-found";
    public const string ConversionFailed = "conversion-failed";
    public const string InternalError = "internal-error";

    /// <summary>错误码对应的退出码（用法类 → 1，输入类 → 2）。</summary>
    public int ExitCode => Code is ConversionFailed or InputNotFound
        ? ExitCodes.InputError
        : Code == InternalError
            ? ExitCodes.InternalError
            : ExitCodes.UsageError;
}
