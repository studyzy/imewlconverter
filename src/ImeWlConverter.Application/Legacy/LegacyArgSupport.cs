namespace ImeWlConverter.Application.Legacy;

/// <summary>
/// 旧版冒号风格 CLI 参数（如 -i:scel）的检测。
/// CLI 入口与 WinForm GUI 的内嵌命令行模式共用；迁移提示文案见 Cli/Output/HumanOutputWriter。
/// </summary>
public static class LegacyArgSupport
{
    /// <summary>检测旧版冒号风格参数（如 -i:scel）。</summary>
    public static bool IsLegacyArgFormat(string[] args)
    {
        return args.Any(arg => arg.StartsWith("-i:") || arg.StartsWith("-o:") ||
                               arg.StartsWith("-c:") || arg.StartsWith("-f:") ||
                               arg.StartsWith("-ft:") || arg.StartsWith("-r:") ||
                               arg.StartsWith("-ct:") || arg.StartsWith("-os:") ||
                               arg.StartsWith("-mc:") || arg.StartsWith("-ld2:"));
    }
}
