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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter;

/// <summary>
/// 控制台运行业务协调器 - 负责转换流程的协调，不再处理参数解析
/// </summary>
public class ConsoleRun
{
    private readonly List<ComboBoxShowAttribute> cbxExportItems = new();
    private readonly List<ComboBoxShowAttribute> cbxImportItems = new();

    private readonly IDictionary<string, IWordLibraryExport> exports =
        new Dictionary<string, IWordLibraryExport>();

    private readonly IDictionary<string, IWordLibraryImport> imports =
        new Dictionary<string, IWordLibraryImport>();

    [RequiresUnreferencedCode("Calls LoadImeList()")]
    public ConsoleRun()
    {
        LoadImeList();
    }

    /// <summary>
    /// 执行转换 - 新的入口点，接受解析后的命令行选项
    /// </summary>
    public void Execute(CommandLineOptions options)
    {
        // 验证选项
        ValidateOptions(options);

        // 获取 Import 和 Export 接口
        var wordLibraryImport = GetImportInterface(options.InputFormat);
        var wordLibraryExport = GetExportInterface(options.OutputFormat);

        // 创建解析模式对象
        var pattern = new ParsePattern
        {
            ContainCode = true,
            SplitString = " ",
            CodeSplitString = ",",
            CodeSplitType = BuildType.None,
            Sort = new List<int> { 2, 1, 3 },
            ContainRank = false
        };

        // 配置自定义格式
        if (!string.IsNullOrEmpty(options.CustomFormat))
        {
            ConfigureCustomFormat(options.CustomFormat, pattern);
        }

        // 配置编码文件
        if (!string.IsNullOrEmpty(options.CodeFile))
        {
            pattern.MappingTablePath = options.CodeFile;
            pattern.IsPinyinFormat = false;
            pattern.CodeType = CodeType.UserDefine;
        }

        // 配置编码类型
        if (!string.IsNullOrEmpty(options.CodeType))
        {
            ConfigureCodeType(options.CodeType, pattern);
        }

        // 配置目标操作系统
        if (!string.IsNullOrEmpty(options.TargetOS))
        {
            ConfigureTargetOS(options.TargetOS, pattern);
        }

        // 配置多字词编码规则
        if (!string.IsNullOrEmpty(options.MultiCode))
        {
            pattern.MutiWordCodeFormat = options.MultiCode.Replace(",", "\n");
        }

        // 应用模式到 Import/Export
        if (wordLibraryImport is SelfDefining selfDefImport)
        {
            selfDefImport.UserDefiningPattern = pattern;
        }

        if (wordLibraryExport is SelfDefining selfDefExport)
        {
            selfDefExport.UserDefiningPattern = pattern;
        }

        if (wordLibraryExport is Rime rimeExport)
        {
            rimeExport.CodeType = pattern.CodeType;
            rimeExport.OS = pattern.OS;
        }

        // 配置 Lingoes ld2 编码
        if (wordLibraryImport is LingoesLd2 ld2Import && !string.IsNullOrEmpty(options.Ld2Encoding))
        {
            ConfigureLd2Encoding(options.Ld2Encoding, ld2Import);
        }

        // 配置过滤器
        var filters = new List<ISingleFilter>();
        if (!string.IsNullOrEmpty(options.Filter))
        {
            ConfigureFilters(options.Filter, filters);
        }

        // 配置词频生成器
        IWordRankGenerater wordRankGenerater = new DefaultWordRankGenerater();
        if (!string.IsNullOrEmpty(options.RankGenerator))
        {
            wordRankGenerater = ConfigureRankGenerator(options.RankGenerator);
        }

        // 执行转换
        PerformConversion(
            options.InputFiles,
            options.OutputPath,
            wordLibraryImport,
            wordLibraryExport,
            filters,
            wordRankGenerater
        );
    }

    private void ValidateOptions(CommandLineOptions options)
    {
        // 验证输入格式是否有效
        if (!imports.ContainsKey(options.InputFormat))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"错误: 未知的输入格式 '{options.InputFormat}'");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("支持的输入格式:");
            foreach (var item in cbxImportItems)
            {
                Console.WriteLine($"  {item.ShortCode,-10} - {item.Name}");
            }
            throw new ArgumentException($"未知的输入格式: {options.InputFormat}");
        }

        // 验证输出格式是否有效
        if (!exports.ContainsKey(options.OutputFormat))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"错误: 未知的输出格式 '{options.OutputFormat}'");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("支持的输出格式:");
            foreach (var item in cbxExportItems)
            {
                Console.WriteLine($"  {item.ShortCode,-10} - {item.Name}");
            }
            throw new ArgumentException($"未知的输出格式: {options.OutputFormat}");
        }

        // 验证自定义格式的选项组合
        if (options.OutputFormat == "self" && string.IsNullOrEmpty(options.CustomFormat))
        {
            Console.WriteLine("警告: 输出为自定义格式 (self) 但未指定 --custom-format，将使用默认格式");
        }
    }

    private void ConfigureCustomFormat(string format, ParsePattern pattern)
    {
        if (format.Length < 9)
        {
            throw new ArgumentException($"自定义格式参数不正确。当前格式: '{format}'，至少需要9个字符，格式示例: '213, ,nyyy'");
        }

        var sort = new List<int>();
        for (var i = 0; i < 3; i++)
        {
            sort.Add(format[i] - '0');
        }

        pattern.Sort = sort;
        pattern.CodeSplitString = format[3].ToString();
        pattern.SplitString = format[4].ToString();

        var t = format[5].ToString().ToLower();
        pattern.CodeSplitType = t switch
        {
            "l" => BuildType.LeftContain,
            "r" => BuildType.RightContain,
            "b" => BuildType.FullContain,
            "n" => BuildType.None,
            _ => BuildType.None
        };

        pattern.ContainCode = format[6].ToString().ToLower() == "y";

        if (format.Length > 8)
        {
            pattern.ContainRank = format[8].ToString().ToLower() == "y";
        }
        else
        {
            pattern.ContainRank = false;
        }
    }

    private void ConfigureFilters(string filterString, IList<ISingleFilter> filters)
    {
        var lenRegex = new Regex(@"len:(\d+)-(\d+)");
        var rankRegex = new Regex(@"rank:(\d+)-(\d+)");
        var rmRegex = new Regex(@"rm:(\w+)");

        foreach (var filterStr in filterString.Split('|'))
        {
            if (lenRegex.IsMatch(filterStr))
            {
                var match = lenRegex.Match(filterStr);
                var from = Convert.ToInt32(match.Groups[1].Value);
                var to = Convert.ToInt32(match.Groups[2].Value);
                filters.Add(new LengthFilter { MinLength = from, MaxLength = to });
            }
            else if (rankRegex.IsMatch(filterStr))
            {
                var match = rankRegex.Match(filterStr);
                var from = Convert.ToInt32(match.Groups[1].Value);
                var to = Convert.ToInt32(match.Groups[2].Value);
                filters.Add(new RankFilter { MinLength = from, MaxLength = to });
            }
            else if (rmRegex.IsMatch(filterStr))
            {
                var match = rmRegex.Match(filterStr);
                var rmType = match.Groups[1].Value;
                ISingleFilter filter = rmType switch
                {
                    "eng" => new EnglishFilter(),
                    "num" => new NumberFilter(),
                    "space" => new SpaceFilter(),
                    "pun" => new EnglishPunctuationFilter(),
                    _ => throw new ArgumentException($"不支持的过滤器类型: {rmType}")
                };
                filters.Add(filter);
            }
        }
    }

    private void ConfigureCodeType(string codeType, ParsePattern pattern)
    {
        pattern.CodeType = codeType.ToLower() switch
        {
            "pinyin" => CodeType.Pinyin,
            "wubi" => CodeType.Wubi,
            "zhengma" => CodeType.Zhengma,
            "cangjie" => CodeType.Cangjie,
            "zhuyin" => CodeType.TerraPinyin,
            _ => CodeType.Pinyin
        };
    }

    private void ConfigureTargetOS(string os, ParsePattern pattern)
    {
        pattern.OS = os.ToLower() switch
        {
            "windows" => OperationSystem.Windows,
            "mac" or "macos" => OperationSystem.MacOS,
            "linux" or "unix" => OperationSystem.Linux,
            _ => OperationSystem.Windows
        };
    }

    private IWordRankGenerater ConfigureRankGenerator(string rankType)
    {
        return rankType.ToLower() switch
        {
            "baidu" => new BaiduWordRankGenerater(),
            "google" => new GoogleWordRankGenerater(),
            _ => new DefaultWordRankGenerater
            {
                ForceUse = true,
                Rank = Convert.ToInt32(rankType)
            }
        };
    }

    private void ConfigureLd2Encoding(string encodingString, LingoesLd2 ld2Import)
    {
        var arr = encodingString.Split(',');
        ld2Import.WordEncoding = Encoding.GetEncoding(arr[0]);
        if (arr.Length > 1)
        {
            ld2Import.XmlEncoding = Encoding.GetEncoding(arr[1]);
            ld2Import.IncludeMeaning = true;
        }
    }

    private void PerformConversion(
        List<string> inputFiles,
        string outputPath,
        IWordLibraryImport import,
        IWordLibraryExport export,
        IList<ISingleFilter> filters,
        IWordRankGenerater wordRankGenerater)
    {
        var mainBody = new MainBody
        {
            Export = export,
            Import = import,
            SelectedWordRankGenerater = wordRankGenerater,
            Filters = filters
        };

        mainBody.ProcessNotice += MainBody_ProcessNotice;

        Console.WriteLine("转换开始...");

        // 批量输出模式（输出路径以 / 结尾或为目录）
        if (outputPath.EndsWith("/") || outputPath.EndsWith("\\"))
        {
            mainBody.Convert(inputFiles, outputPath);
        }
        else
        {
            // 单文件输出模式
            var str = mainBody.Convert(inputFiles);
            FileOperationHelper.WriteFile(outputPath, export.Encoding, str);
        }

        Console.WriteLine($"转换完成，共转换 {mainBody.Count} 个词条");
    }

    private void MainBody_ProcessNotice(string message)
    {
        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t" + message);
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    private void LoadImeList()
    {
        var assembly = GetType().Assembly;
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.Namespace != null && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME"))
            {
                var attributes = type.GetCustomAttributes(typeof(ComboBoxShowAttribute), false);
                if (attributes.Length > 0)
                {
                    var cbxa = attributes[0] as ComboBoxShowAttribute;
                    Debug.WriteLine($"{cbxa.ShortCode} - Index: {cbxa.Index}");

                    if (type.GetInterface("IWordLibraryImport") != null)
                    {
                        Debug.WriteLine($"Import: {type.FullName}");
                        cbxImportItems.Add(cbxa);
                        imports.Add(
                            cbxa.ShortCode,
                            assembly.CreateInstance(type.FullName) as IWordLibraryImport
                        );
                    }

                    if (type.GetInterface("IWordLibraryExport") != null)
                    {
                        Debug.WriteLine($"Export: {type.FullName}");
                        cbxExportItems.Add(cbxa);
                        exports.Add(
                            cbxa.ShortCode,
                            assembly.CreateInstance(type.FullName) as IWordLibraryExport
                        );
                    }
                }
            }
        }

        cbxImportItems.Sort((a, b) => a.Index - b.Index);
        cbxExportItems.Sort((a, b) => a.Index - b.Index);
    }

    private IWordLibraryExport GetExportInterface(string str)
    {
        if (!exports.TryGetValue(str, out var export))
        {
            throw new ArgumentException($"导出词库的输入法格式错误: {str}");
        }
        return export;
    }

    private IWordLibraryImport GetImportInterface(string str)
    {
        if (!imports.TryGetValue(str, out var import))
        {
            throw new ArgumentException($"导入词库的输入法格式错误: {str}");
        }
        return import;
    }
}
