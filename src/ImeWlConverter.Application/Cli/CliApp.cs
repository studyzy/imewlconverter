using System.CommandLine;
using System.CommandLine.Invocation;
using ImeWlConverter.Application.Cli;
using ImeWlConverter.Application.Cli.Output;
using ImeWlConverter.Application.Legacy;

namespace ImeWlConverter.Application.Cli;

/// <summary>
/// CLI 统一入口：旧参数检测 → 根命令构建与调用。
/// ImeWlConverterCmd 与 WinForms GUI 的内嵌命令行模式共用。
/// </summary>
public static class CliApp
{
    public static int Run(string[] args)
    {
        if (LegacyArgSupport.IsLegacyArgFormat(args))
        {
            HumanOutputWriter.WriteLegacyMigrationHelp();
            return ExitCodes.UsageError;
        }

        var rootCommand = CliCommandFactory.Build();
        return rootCommand.Invoke(args);
    }

    /// <summary>异步版本（推荐：全链无同步阻塞）。</summary>
    public static Task<int> RunAsync(string[] args)
    {
        if (LegacyArgSupport.IsLegacyArgFormat(args))
        {
            HumanOutputWriter.WriteLegacyMigrationHelp();
            return Task.FromResult(ExitCodes.UsageError);
        }

        var rootCommand = CliCommandFactory.Build();
        return rootCommand.InvokeAsync(args);
    }
}
