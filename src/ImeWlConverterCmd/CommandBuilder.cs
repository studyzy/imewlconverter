#nullable enable

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core;
using ImeWlConverter.Core.Helpers;
using ImeWlConverter.Formats;
using ImeWlConverter.Formats.SelfDefining;
using Microsoft.Extensions.DependencyInjection;

namespace Studyzy.IMEWLConverter;

public static class CommandBuilder
{
    public static RootCommand Build()
    {
        var rootCommand = new RootCommand("IME WL Converter - 深蓝词库转换\n" +
                                          "跨平台的输入法词库转换工具，支持 50+ 种输入法格式");

        var inputFormatOption = new Option<string>(
            aliases: new[] { "--input-format", "-i" },
            description: "输入词库格式代码 (例如: scel, ggpy, qqpy, rime, bdpy)")
        { IsRequired = false };
        rootCommand.AddOption(inputFormatOption);

        var outputFormatOption = new Option<string>(
            aliases: new[] { "--output-format", "-o" },
            description: "输出词库格式代码 (例如: ggpy, rime, self, qqpy)")
        { IsRequired = false };
        rootCommand.AddOption(outputFormatOption);

        var outputPathOption = new Option<string>(
            aliases: new[] { "--output", "-O" },
            description: "输出文件路径或目录路径（目录路径以 / 结尾）")
        { IsRequired = false };
        rootCommand.AddOption(outputPathOption);

        var inputFilesArgument = new Argument<List<string>>(
            name: "input-files",
            description: "输入词库文件路径（支持多个文件和通配符）")
        { Arity = ArgumentArity.ZeroOrMore };
        rootCommand.AddArgument(inputFilesArgument);

        var filterOption = new Option<string?>(
            aliases: new[] { "--filter", "-f" },
            description: "过滤条件 (例如: \"len:1-100|rm:eng|rm:num\")\n" +
                        "  len:1-100    - 保留字数 1-100 的词条\n" +
                        "  rank:2-9999  - 保留词频 2-9999 的词条\n" +
                        "  rm:eng       - 移除包含英文的词条\n" +
                        "  rm:num       - 移除包含数字的词条\n" +
                        "  rm:space     - 移除包含空格的词条\n" +
                        "  rm:pun       - 移除包含标点的词条");
        rootCommand.AddOption(filterOption);

        var customFormatOption = new Option<string?>(
            aliases: new[] { "--custom-format", "-F" },
            description: "自定义格式配置 (用于 self 格式)\n" +
                        "  格式: <顺序><拼音分隔符><字段分隔符><位置><显示>\n" +
                        "  示例: \"213 ,nyyy\" = 词语,拼音(空格分隔),词频");
        rootCommand.AddOption(customFormatOption);

        var codeTypeOption = new Option<string?>(
            aliases: new[] { "--code-type", "-t" },
            description: "编码类型 (pinyin=拼音, wubi=五笔, zhengma=郑码, cangjie=仓颉, zhuyin=注音, userdefine=自定义)");
        rootCommand.AddOption(codeTypeOption);

        var codeFileOption = new Option<string?>(
            aliases: new[] { "--code-file", "-c" },
            description: "自定义编码映射表文件路径（Tab分隔，格式：汉字\\t编码）");
        rootCommand.AddOption(codeFileOption);

        var multiCodeOption = new Option<string?>(
            aliases: new[] { "--multi-code", "-m" },
            description: "多字词编码规则（逗号分隔）\n" +
                        "  示例: \"code_e2=p11+p12+p21+p22,code_e3=p11+p21+p31+p32,code_a4=p11+p21+p31+n11\"");
        rootCommand.AddOption(multiCodeOption);

        var listFormatsOption = new Option<bool>(
            aliases: new[] { "--list-formats" },
            description: "显示所有支持的输入法格式列表");
        rootCommand.AddOption(listFormatsOption);

        rootCommand.SetHandler((context) =>
        {
            var listFormats = context.ParseResult.GetValueForOption(listFormatsOption);
            if (listFormats)
            {
                ShowSupportedFormats();
                context.ExitCode = 0;
                return;
            }

            var inputFormat = context.ParseResult.GetValueForOption(inputFormatOption);
            var outputFormat = context.ParseResult.GetValueForOption(outputFormatOption);
            var outputPath = context.ParseResult.GetValueForOption(outputPathOption);
            var inputFiles = context.ParseResult.GetValueForArgument(inputFilesArgument);

            if (string.IsNullOrEmpty(inputFormat))
            {
                PrintError("缺少必填选项 --input-format");
                context.ExitCode = 1;
                return;
            }
            if (string.IsNullOrEmpty(outputFormat))
            {
                PrintError("缺少必填选项 --output-format");
                context.ExitCode = 1;
                return;
            }
            if (string.IsNullOrEmpty(outputPath))
            {
                PrintError("缺少必填选项 --output");
                context.ExitCode = 1;
                return;
            }
            if (inputFiles == null || inputFiles.Count == 0)
            {
                PrintError("未指定输入文件");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var filter = context.ParseResult.GetValueForOption(filterOption);
                var codeType = context.ParseResult.GetValueForOption(codeTypeOption);
                var customFormat = context.ParseResult.GetValueForOption(customFormatOption);
                var codeFile = context.ParseResult.GetValueForOption(codeFileOption);
                var multiCode = context.ParseResult.GetValueForOption(multiCodeOption);
                ExecuteConversion(inputFormat, outputFormat, outputPath, inputFiles, filter, codeType, customFormat, codeFile, multiCode);
                context.ExitCode = 0;
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                context.ExitCode = 1;
            }
        });

        return rootCommand;
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAllFormats();
        services.AddImeWlConverterCore();
        return services.BuildServiceProvider();
    }

    private static void ShowSupportedFormats()
    {
        using var sp = BuildServiceProvider();
        var importers = sp.GetServices<IFormatImporter>().OrderBy(i => i.Metadata.SortOrder).ToList();
        var exporters = sp.GetServices<IFormatExporter>().OrderBy(e => e.Metadata.SortOrder).ToList();

        Console.WriteLine("支持的输入格式：");
        foreach (var imp in importers)
            Console.WriteLine($"  {imp.Metadata.Id,-15} {imp.Metadata.DisplayName}");

        Console.WriteLine();
        Console.WriteLine("支持的输出格式：");
        foreach (var exp in exporters)
            Console.WriteLine($"  {exp.Metadata.Id,-15} {exp.Metadata.DisplayName}");
    }

    private static void ExecuteConversion(
        string inputFormat, string outputFormat, string outputPath,
        List<string> inputFiles, string? filter, string? codeType, string? customFormat,
        string? codeFile, string? multiCode)
    {
        using var sp = BuildServiceProvider();

        var filterConfig = ParseFilterConfig(filter);
        var targetCodeType = ParseCodeType(codeType);

        var importers = sp.GetServices<IFormatImporter>().ToList();
        var exporters = sp.GetServices<IFormatExporter>().ToList();

        // Configure self-defining format if -F is provided
        if (!string.IsNullOrEmpty(customFormat))
        {
            foreach (var imp in importers.OfType<SelfDefiningImporter>())
                ConfigureSelfDefining(imp, customFormat);
            foreach (var exp in exporters.OfType<SelfDefiningExporter>())
                ConfigureSelfDefining(exp, customFormat);
        }

        // If code file is provided, use UserDefine code type
        if (!string.IsNullOrEmpty(codeFile))
            targetCodeType = CodeType.UserDefine;

        // Auto-detect code type from output format when not explicitly specified
        if (targetCodeType == CodeType.NoCode)
            targetCodeType = InferCodeTypeFromOutputFormat(outputFormat, customFormat);

        var pipeline = sp.GetRequiredService<IConversionPipeline>();

        var request = new ConversionRequest
        {
            InputFormatId = inputFormat,
            OutputFormatId = outputFormat,
            InputPaths = inputFiles,
            OutputPath = outputPath,
            FilterConfig = filterConfig,
            Options = new ConversionOptions
            {
                CodeGeneration = new CodeGenerationOptions
                {
                    TargetCodeType = targetCodeType,
                    CodeFilePath = codeFile,
                    MultiCodeFormat = multiCode
                }
            }
        };

        var result = pipeline.ExecuteAsync(request, new ConsoleProgress()).GetAwaiter().GetResult();
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error);

        Console.WriteLine($"转换完成: 导入 {result.Value.ImportedCount} 条, " +
                         $"过滤 {result.Value.FilteredCount} 条, " +
                         $"导出 {result.Value.ExportedCount} 条");
    }

    private static void ConfigureSelfDefining(SelfDefiningImporter importer, string spec)
    {
        if (spec.Length < 7) return;
        importer.OrderSpec = spec[..3];
        importer.PinyinSeparator = spec[3];
        importer.FieldSeparator = spec[4];
        // spec[5] = position indicator (l/r/b/n) - not used for import
        importer.ShowPinyin = spec.Length > 6 && spec[6] == 'y';
        importer.ShowWord = spec.Length > 7 && spec[7] == 'y';
        importer.ShowRank = spec.Length > 8 && spec[8] == 'y';
    }

    private static void ConfigureSelfDefining(SelfDefiningExporter exporter, string spec)
    {
        if (spec.Length < 7) return;
        exporter.OrderSpec = spec[..3];
        exporter.PinyinSeparator = spec[3];
        exporter.FieldSeparator = spec[4];
        // spec[5] = position indicator (l/r/b/n) - not used for export
        exporter.ShowPinyin = spec.Length > 6 && spec[6] == 'y';
        exporter.ShowWord = spec.Length > 7 && spec[7] == 'y';
        exporter.ShowRank = spec.Length > 8 && spec[8] == 'y';
    }

    private static FilterConfig? ParseFilterConfig(string? filterStr)
    {
        if (string.IsNullOrEmpty(filterStr)) return null;

        var config = new FilterConfig();

        foreach (var part in filterStr.Split('|'))
        {
            if (part.StartsWith("len:"))
            {
                var range = part[4..].Split('-');
                config.WordLengthFrom = int.Parse(range[0]);
                config.WordLengthTo = range.Length > 1 ? int.Parse(range[1]) : 9999;
            }
            else if (part.StartsWith("rank:"))
            {
                var range = part[5..].Split('-');
                config.WordRankFrom = int.Parse(range[0]);
                config.WordRankTo = range.Length > 1 ? int.Parse(range[1]) : 999999;
            }
            else if (part == "rm:eng") config.IgnoreEnglish = true;
            else if (part == "rm:num") config.IgnoreNumber = true;
            else if (part == "rm:space") config.IgnoreSpace = true;
            else if (part == "rm:pun") config.IgnorePunctuation = true;
        }

        return config;
    }

    private static CodeType InferCodeTypeFromOutputFormat(string outputFormat, string? customFormat)
    {
        // Self-defining: check if pinyin display is requested
        if (outputFormat == "self" && !string.IsNullOrEmpty(customFormat) && customFormat.Length > 6 && customFormat[6] == 'y')
            return CodeType.Pinyin;

        return CodeTypeInference.InferFromOutputFormat(outputFormat);
    }

    private static CodeType ParseCodeType(string? codeType)
    {
        return codeType?.ToLowerInvariant() switch
        {
            "pinyin" => CodeType.Pinyin,
            "wubi" or "wubi86" => CodeType.Wubi86,
            "wubi98" => CodeType.Wubi98,
            "wubinage" or "wubi_newage" => CodeType.WubiNewAge,
            "zhengma" => CodeType.Zhengma,
            "cangjie" or "cangjie5" => CodeType.Cangjie5,
            "zhuyin" => CodeType.Zhuyin,
            "terra" or "terra_pinyin" => CodeType.TerraPinyin,
            "userdefine" or "user_define" or "custom" => CodeType.UserDefine,
            _ => CodeType.NoCode
        };
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"错误: {message}");
        Console.ResetColor();
        Console.Error.WriteLine("使用 --help 查看帮助信息");
    }

    private sealed class ConsoleProgress : IProgress<ProgressInfo>
    {
        public void Report(ProgressInfo value)
        {
            if (!string.IsNullOrEmpty(value.Message))
                Console.Error.Write($"\r{value.Message,-80}");
        }
    }
}
