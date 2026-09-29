using ImeWlConverter.Abstractions.Contracts;

namespace ImeWlConverter.Application.Cli;

/// <summary>
/// CLI 参数校验：必填项、文件存在性、格式 ID 有效性。
/// 返回 CliError?（null = 通过），错误按用途分类并映射退出码。
/// </summary>
public static class CliValidator
{
    public static CliError? Validate(
        string? inputFormat, string? outputFormat, string? outputPath,
        IReadOnlyList<string> inputFiles,
        IReadOnlyList<IFormatImporter> importers,
        IReadOnlyList<IFormatExporter> exporters)
    {
        if (string.IsNullOrEmpty(inputFormat))
            return new CliError(CliError.MissingOption, "缺少必填选项 --input-format", "--input-format");
        if (string.IsNullOrEmpty(outputFormat))
            return new CliError(CliError.MissingOption, "缺少必填选项 --output-format", "--output-format");
        if (string.IsNullOrEmpty(outputPath))
            return new CliError(CliError.MissingOption, "缺少必填选项 --output", "--output");
        if (inputFiles.Count == 0)
            return new CliError(CliError.MissingOption, "未指定输入文件");

        var missingFile = inputFiles.FirstOrDefault(f => !File.Exists(f));
        if (missingFile is not null)
            return new CliError(CliError.InputNotFound, $"输入文件不存在: {missingFile}");

        if (importers.All(i => i.Metadata.Id != inputFormat))
            return new CliError(CliError.UnknownFormat, $"未知的输入格式: {inputFormat}", "--input-format");
        if (exporters.All(e => e.Metadata.Id != outputFormat))
            return new CliError(CliError.UnknownFormat, $"未知的输出格式: {outputFormat}", "--output-format");

        return null;
    }
}
