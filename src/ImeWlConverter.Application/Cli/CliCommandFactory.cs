using System.CommandLine;
using System.CommandLine.Invocation;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Application.Cli.Output;
using ImeWlConverter.Application.Requests;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Formats;
using Microsoft.Extensions.DependencyInjection;

namespace ImeWlConverter.Application.Cli;

/// <summary>
/// CLI 根命令装配与转换执行（System.CommandLine 前端，唯一入口 CliApp.Run）。
/// 职责链：CliOptions 定义 → CliValidator 校验 → ConversionRequestFactory 组装 → 管道执行 → Writer 输出。
/// </summary>
public static class CliCommandFactory
{
    public static RootCommand Build()
    {
        var rootCommand = new RootCommand("IME WL Converter - 深蓝词库转换\n" +
                                          "跨平台的输入法词库转换工具，支持 50+ 种输入法格式");
        CliOptions.AddTo(rootCommand);

        rootCommand.SetHandler(async (context) =>
        {
            var json = context.ParseResult.GetValueForOption(CliOptions.Json);
            var verbose = context.ParseResult.GetValueForOption(CliOptions.Verbose);
            context.ExitCode = await RunAsync(context, json, verbose);
        });

        return rootCommand;
    }

    private static async Task<int> RunAsync(InvocationContext context, bool json, bool verbose)
    {
        var parseResult = context.ParseResult;

        // 1. 列出格式（人类 / JSON 双模式）
        if (parseResult.GetValueForOption(CliOptions.ListFormats))
        {
            using var sp = ImeWlConverterBootstrapper.CreateServiceProvider();
            var listImporters = sp.GetServices<IFormatImporter>().ToList();
            var listExporters = sp.GetServices<IFormatExporter>().ToList();
            if (json)
                FormatListWriter.WriteJson(listImporters, listExporters);
            else
                FormatListWriter.WriteHuman(listImporters, listExporters);
            return ExitCodes.Success;
        }

        // 2. 读取参数
        var options = new Requests.CliConversionOptions
        {
            InputFormatId = parseResult.GetValueForOption(CliOptions.InputFormat) ?? "",
            OutputFormatId = parseResult.GetValueForOption(CliOptions.OutputFormat) ?? "",
            OutputPath = parseResult.GetValueForOption(CliOptions.OutputPath) ?? "",
            InputFiles = parseResult.GetValueForArgument(CliOptions.InputFiles) ?? [],
            FilterSpec = parseResult.GetValueForOption(CliOptions.Filter),
            CodeType = parseResult.GetValueForOption(CliOptions.CodeType),
            CustomFormat = parseResult.GetValueForOption(CliOptions.CustomFormat),
            CodeFile = parseResult.GetValueForOption(CliOptions.CodeFile),
            MultiCode = parseResult.GetValueForOption(CliOptions.MultiCode),
            DictId = parseResult.GetValueForOption(CliOptions.DictId),
            DictName = parseResult.GetValueForOption(CliOptions.DictName),
            DictCategory = parseResult.GetValueForOption(CliOptions.DictCategory),
            DictDescription = parseResult.GetValueForOption(CliOptions.DictDescription),
        };

        using var serviceProvider = ImeWlConverterBootstrapper.CreateServiceProvider();
        var importers = serviceProvider.GetServices<IFormatImporter>().ToList();
        var exporters = serviceProvider.GetServices<IFormatExporter>().ToList();

        // 3. 校验（用法/输入错误）
        var validationError = CliValidator.Validate(
            options.InputFormatId, options.OutputFormatId, options.OutputPath,
            options.InputFiles, importers, exporters);
        if (validationError is not null)
            return ReportError(validationError, json);

        // 4. 组装请求（filter/spec 语法错误 → 用法错误）
        var requestResult = ConversionRequestFactory.Create(options, importers, exporters);
        if (requestResult.IsFailure)
        {
            return ReportError(new CliError(
                options.FilterSpec is not null ? CliError.InvalidFilter : CliError.InvalidSpec,
                requestResult.Error), json);
        }

        // 5. 执行转换
        var pipeline = serviceProvider.GetRequiredService<IConversionPipeline>();
        IProgress<ImeWlConverter.Abstractions.Models.ProgressInfo>? progress = null;
        if (!json || verbose)
            progress = new HumanOutputWriter.ConsoleProgress(explicitVerbose: json && verbose);

        ImeWlConverter.Abstractions.Results.Result<ImeWlConverter.Abstractions.Contracts.ConversionResult> result;
        try
        {
            result = await pipeline.ExecuteAsync(requestResult.Value, progress);
        }
        catch (OperationCanceledException)
        {
            return ReportError(new CliError(CliError.ConversionFailed, "转换已取消"), json);
        }
        catch (Exception ex)
        {
            // 默认只输出错误消息；IMEWL_DEBUG=1 时输出完整堆栈便于诊断
            var message = Environment.GetEnvironmentVariable("IMEWL_DEBUG") == "1"
                ? ex.ToString()
                : ex.Message;
            if (json)
                JsonOutputWriter.WriteInternalError(message);
            else
                HumanOutputWriter.WriteError(message);
            return ExitCodes.InternalError;
        }

        if (result.IsFailure)
        {
            return ReportError(new CliError(CliError.ConversionFailed, result.Error), json);
        }

        // 6. 结果与退出码
        var value = result.Value;
        if (value.Errors.Count > 0)
        {
            // 部分失败：有成功输出但也有文件失败
            if (json)
                JsonOutputWriter.WriteSuccess(value, options.OutputPath);
            else
                HumanOutputWriter.WriteError(value.ErrorMessages!.TrimEnd());
            return ExitCodes.PartialSuccess;
        }

        if (json)
            JsonOutputWriter.WriteSuccess(value, options.OutputPath);
        else
            HumanOutputWriter.WriteSuccess(value);
        return ExitCodes.Success;
    }

    private static int ReportError(CliError error, bool json)
    {
        if (json)
            JsonOutputWriter.WriteError(error);
        else
            HumanOutputWriter.WriteError(error.Message);
        return error.ExitCode;
    }
}
