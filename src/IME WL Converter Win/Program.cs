/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)
 *
 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.CommandLine;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ImeWlConverter.Core;
using ImeWlConverter.Formats;
using Microsoft.Extensions.DependencyInjection;

namespace Studyzy.IMEWLConverter;

internal static class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);

    private const int ATTACH_PARENT_PROCESS = -1;

    [STAThread]
    private static int Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (args.Length > 0 && AttachConsole(ATTACH_PARENT_PROCESS))
        {
            // CLI 模式：已附着到父进程控制台，使输出在命令行中可见
            try
            {
                // 使用控制台当前代码页的编码，避免中文乱码
                var encoding = Console.OutputEncoding;
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), encoding) { AutoFlush = true });
                Console.SetError(new StreamWriter(Console.OpenStandardError(), encoding) { AutoFlush = true });

                if (CommandBuilder.IsLegacyArgFormat(args))
                {
                    return CommandBuilder.PrintLegacyArgHelp();
                }

                var rootCommand = CommandBuilder.Build();
                return rootCommand.Invoke(args);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.Error.WriteLine("错误: " + ex.Message);
                }
                catch
                {
                    // 控制台输出流不可用时忽略，避免二次异常
                }

                return 1;
            }
        }

        // GUI 模式。
        // 注意：带参数但 AttachConsole 失败时（无可用控制台，如从资源管理器拖放文件到 exe），
        // 也会落到这里回退为 GUI，避免无提示崩溃。
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.SetCompatibleTextRenderingDefault(false);

        var services = new ServiceCollection();
        services.AddAllFormats();
        services.AddImeWlConverterCore();
        var serviceProvider = services.BuildServiceProvider();

        Application.Run(new MainForm(serviceProvider));
        return 0;
    }
}
