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
using System.CommandLine;
using System.Linq;
using System.Text;

namespace Studyzy.IMEWLConverter;

internal class Program
{
    private static int Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 检测旧格式参数（包含冒号）
        if (args.Any(arg => arg.Contains(":") && (arg.StartsWith("-i:") || arg.StartsWith("-o:") ||
                                                   arg.StartsWith("-c:") || arg.StartsWith("-f:") ||
                                                   arg.StartsWith("-ft:") || arg.StartsWith("-r:") ||
                                                   arg.StartsWith("-ct:") || arg.StartsWith("-os:") ||
                                                   arg.StartsWith("-mc:") || arg.StartsWith("-ld2:"))))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("错误: 检测到旧的参数格式");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("命令行参数格式已更新为 GNU 风格。请更新您的命令：");
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
            return 1;
        }

        // 使用新的命令行解析系统
        var rootCommand = CommandBuilder.Build();
        return rootCommand.Invoke(args);
    }
}
