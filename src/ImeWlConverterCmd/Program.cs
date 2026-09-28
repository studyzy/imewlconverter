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
using System.Text;

namespace Studyzy.IMEWLConverter;

internal class Program
{
    private static int Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 检测旧格式参数（包含冒号）
        if (CommandBuilder.IsLegacyArgFormat(args))
        {
            return CommandBuilder.PrintLegacyArgHelp();
        }

        // 使用新的命令行解析系统
        var rootCommand = CommandBuilder.Build();
        return rootCommand.Invoke(args);
    }
}
