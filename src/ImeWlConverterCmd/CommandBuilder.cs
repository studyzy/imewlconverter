/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Studyzy.IMEWLConverter;

/// <summary>
/// 命令行参数构建器，使用 System.CommandLine 定义所有选项
/// </summary>
public static class CommandBuilder
{
    public static RootCommand Build()
    {
        var rootCommand = new RootCommand("IME WL Converter - 深蓝词库转换\n" +
                                          "跨平台的输入法词库转换工具，支持 20+ 种输入法格式");

        // 必填选项：输入格式
        var inputFormatOption = new Option<string>(
            aliases: new[] { "--input-format", "-i" },
            description: "输入词库格式代码 (例如: scel, ggpy, qqpy, rime, bdpy)")
        {
            IsRequired = false  // 改为非必填，在 handler 中检查
        };
        rootCommand.AddOption(inputFormatOption);

        // 必填选项：输出格式
        var outputFormatOption = new Option<string>(
            aliases: new[] { "--output-format", "-o" },
            description: "输出词库格式代码 (例如: ggpy, rime, self, qqpy)")
        {
            IsRequired = false  // 改为非必填
        };
        rootCommand.AddOption(outputFormatOption);

        // 必填选项：输出路径
        var outputPathOption = new Option<string>(
            aliases: new[] { "--output", "-O" },
            description: "输出文件路径或目录路径（目录路径以 / 结尾）")
        {
            IsRequired = false  // 改为非必填
        };
        rootCommand.AddOption(outputPathOption);

        // 必填位置参数：输入文件
        var inputFilesArgument = new Argument<List<string>>(
            name: "input-files",
            description: "输入词库文件路径（支持多个文件和通配符）")
        {
            Arity = ArgumentArity.ZeroOrMore  // 改为可选
        };
        rootCommand.AddArgument(inputFilesArgument);

        // 可选选项：编码文件
        var codeFileOption = new Option<string?>(
            aliases: new[] { "--code-file", "-c" },
            description: "自定义编码映射文件路径（用于自定义编码类型）");
        rootCommand.AddOption(codeFileOption);

        // 可选选项：过滤条件
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

        // 可选选项：自定义格式
        var customFormatOption = new Option<string?>(
            aliases: new[] { "--custom-format", "-F" },
            description: "自定义格式规范 (例如: \"213, nyyn\")\n" +
                        "  格式: <顺序><分隔符1><分隔符2><位置><显示>\n" +
                        "  213  - 顺序: 1=拼音 2=汉字 3=词频\n" +
                        "  ,    - 拼音分隔符\n" +
                        "  空格 - 字段分隔符\n" +
                        "  n    - 分隔符位置: l=左 r=右 b=两边 n=无\n" +
                        "  yyn  - 显示: y=显示 n=不显示 (拼音/汉字/词频)");
        rootCommand.AddOption(customFormatOption);

        // 可选选项：词频生成器
        var rankGeneratorOption = new Option<string?>(
            aliases: new[] { "--rank-generator", "-r" },
            description: "词频生成方式 (baidu=百度搜索结果数, google=谷歌搜索结果数, 或指定固定数字)");
        rootCommand.AddOption(rankGeneratorOption);

        // 可选选项：多字词编码规则
        var multiCodeOption = new Option<string?>(
            aliases: new[] { "--multi-code", "-m" },
            description: "多字词编码生成规则 (例如: \"code_e2=p11+p12+p21+p22,code_e3=p11+p21+p31+p32\")\n" +
                        "  p11 - 第1个字的第1码\n" +
                        "  p12 - 第1个字的第2码\n" +
                        "  n11 - 最后一个字的第1码");
        rootCommand.AddOption(multiCodeOption);

        // 可选选项：编码类型（用于 Rime）
        var codeTypeOption = new Option<string?>(
            aliases: new[] { "--code-type", "-t" },
            description: "编码类型 (pinyin=拼音, wubi=五笔, zhengma=郑码, cangjie=仓颉, zhuyin=注音)");
        rootCommand.AddOption(codeTypeOption);

        // 可选选项：目标操作系统
        var targetOSOption = new Option<string?>(
            aliases: new[] { "--target-os" },
            description: "目标操作系统 (windows, macos, linux) - 用于 Rime 等格式");
        rootCommand.AddOption(targetOSOption);

        // 可选选项：Lingoes ld2 编码
        var ld2EncodingOption = new Option<string?>(
            aliases: new[] { "--ld2-encoding" },
            description: "Lingoes ld2 文件编码设置 (例如: \"utf-8\" 或 \"gbk,utf-8\")");
        rootCommand.AddOption(ld2EncodingOption);

        // 添加 --list-formats 选项
        var listFormatsOption = new Option<bool>(
            aliases: new[] { "--list-formats" },
            description: "显示所有支持的输入法格式列表");
        rootCommand.AddOption(listFormatsOption);

        // 设置处理器 - 使用 context 方式处理参数
        rootCommand.SetHandler((context) =>
        {
            // 获取 --list-formats 选项
            var listFormats = context.ParseResult.GetValueForOption(listFormatsOption);
            if (listFormats)
            {
                ShowSupportedFormats();
                context.ExitCode = 0;
                return;
            }

            // 获取参数值
            var inputFormat = context.ParseResult.GetValueForOption(inputFormatOption);
            var outputFormat = context.ParseResult.GetValueForOption(outputFormatOption);
            var outputPath = context.ParseResult.GetValueForOption(outputPathOption);
            var inputFiles = context.ParseResult.GetValueForArgument(inputFilesArgument);

            // 检查必填参数
            if (string.IsNullOrEmpty(inputFormat))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("错误: 缺少必填选项 --input-format");
                Console.ResetColor();
                Console.Error.WriteLine("使用 --help 查看帮助信息");
                context.ExitCode = 1;
                return;
            }

            if (string.IsNullOrEmpty(outputFormat))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("错误: 缺少必填选项 --output-format");
                Console.ResetColor();
                Console.Error.WriteLine("使用 --help 查看帮助信息");
                context.ExitCode = 1;
                return;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("错误: 缺少必填选项 --output");
                Console.ResetColor();
                Console.Error.WriteLine("使用 --help 查看帮助信息");
                context.ExitCode = 1;
                return;
            }

            if (inputFiles == null || inputFiles.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("错误: 未指定输入文件");
                Console.ResetColor();
                Console.Error.WriteLine("使用 --help 查看帮助信息");
                context.ExitCode = 1;
                return;
            }

            // 构建选项对象
            var options = new CommandLineOptions
            {
                InputFormat = inputFormat,
                OutputFormat = outputFormat,
                OutputPath = outputPath,
                InputFiles = inputFiles,
                CodeFile = context.ParseResult.GetValueForOption(codeFileOption),
                Filter = context.ParseResult.GetValueForOption(filterOption),
                CustomFormat = context.ParseResult.GetValueForOption(customFormatOption),
                RankGenerator = context.ParseResult.GetValueForOption(rankGeneratorOption),
                MultiCode = context.ParseResult.GetValueForOption(multiCodeOption),
                CodeType = context.ParseResult.GetValueForOption(codeTypeOption),
                TargetOS = context.ParseResult.GetValueForOption(targetOSOption),
                Ld2Encoding = context.ParseResult.GetValueForOption(ld2EncodingOption)
            };

            // 调用转换逻辑
            try
            {
                ExecuteConversion(options);
                context.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"错误: {ex.Message}");
                Console.ResetColor();
                context.ExitCode = 1;
            }
        });

        return rootCommand;
    }

    private static void ShowSupportedFormats()
    {
        Console.WriteLine("支持的输入法格式：");
        Console.WriteLine();

        // TODO: 这里应该从 ConsoleRun 获取实际的格式列表
        // 暂时硬编码一些常用格式作为示例
        var formats = new Dictionary<string, string>
        {
            { "scel", "搜狗拼音细胞词库 (.scel)" },
            { "ggpy", "谷歌拼音" },
            { "qqpy", "QQ 拼音文本格式" },
            { "qpyd", "QQ 拼音分类词库 (.qpyd)" },
            { "qcel", "QQ 拼音细胞词库 (.qcel)" },
            { "rime", "Rime 输入法" },
            { "bdpy", "百度拼音" },
            { "bdict", "百度拼音二进制格式 (.bdict)" },
            { "self", "自定义格式" },
            { "pyjj", "拼音加加" },
            { "zgpy", "紫光拼音" },
            { "libpy", "libpinyin (Linux)" },
            { "fit", "FIT 输入法 (Mac)" },
            { "plist", "macOS 系统拼音" },
        };

        foreach (var (code, name) in formats.OrderBy(f => f.Key))
        {
            Console.WriteLine($"  {code,-10} - {name}");
        }

        Console.WriteLine();
        Console.WriteLine("使用 --help 查看完整帮助信息");
    }

    private static void ExecuteConversion(CommandLineOptions options)
    {
        // 创建 ConsoleRun 实例并执行转换
        var consoleRun = new ConsoleRun();
        consoleRun.Execute(options);
    }
}
