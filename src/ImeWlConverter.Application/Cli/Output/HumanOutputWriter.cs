using ImeWlConverter.Abstractions.Models;

namespace ImeWlConverter.Application.Cli.Output;

/// <summary>
/// 人类可读输出（与历史 CLI 输出逐字节一致；不改动文案，脚本依赖）。
/// </summary>
public static class HumanOutputWriter
{
    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"错误: {message}");
        Console.ResetColor();
        Console.Error.WriteLine("使用 --help 查看帮助信息");
    }

    /// <summary>旧格式迁移提示（与历史输出逐字节一致，含颜色装饰）。</summary>
    public static void WriteLegacyMigrationHelp()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("错误: 检测到旧的参数格式");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("旧格式:");
        Console.WriteLine("  imewlconverter -i:scel input.scel -o:ggpy output.txt");
        Console.WriteLine();
        Console.WriteLine("新格式:");
        Console.WriteLine("  imewlconverter --input-format scel --output-format ggpy --output output.txt input.scel");
        Console.WriteLine("  或使用短选项:");
        Console.WriteLine("  imewlconverter -i scel -o ggpy -O output.txt input.scel");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("常用参数对照:");
        Console.WriteLine("  -i:<format>  →  --input-format <format>  或  -i <format>");
        Console.WriteLine("  -o:<format>  →  --output-format <format> 或  -o <format>");
        Console.WriteLine("  -c:<path>    →  --code-file <path>       或  -c <path>");
        Console.WriteLine("  -f:<spec>    →  --custom-format <spec>   或  -F <spec>");
        Console.WriteLine("  -ft:<filter> →  --filter <filter>        或  -f <filter>");
        Console.WriteLine("  -r:<type>    →  --rank-generator <type>  或  -r <type>");
        Console.WriteLine("  -ct:<type>   →  --code-type <type>       或  -t <type>");
        Console.WriteLine("  -os:<os>     →  --target-os <os>");
        Console.WriteLine("  -mc:<rules>  →  --multi-code <rules>     或  -m <rules>");
        Console.WriteLine();
        Console.WriteLine("查看完整帮助:");
        Console.WriteLine("  imewlconverter --help");
        Console.WriteLine();
        Console.WriteLine("详细迁移指南请参阅: MIGRATION.md");
    }

    public static void WriteSuccess(ImeWlConverter.Abstractions.Contracts.ConversionResult result)
    {
        Console.WriteLine($"转换完成: 导入 {result.ImportedCount} 条, " +
                         $"过滤 {result.FilteredCount} 条, " +
                         $"导出 {result.ExportedCount} 条");
    }

    /// <summary>进度输出到 stderr（\r 行内刷新，与历史行为一致）。</summary>
    public sealed class ConsoleProgress : IProgress<ProgressInfo>
    {
        public void Report(ProgressInfo value)
        {
            if (!string.IsNullOrEmpty(value.Message))
                Console.Error.Write($"\r{value.Message,-80}");
        }
    }
}
