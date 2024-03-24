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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    internal static class Program
    {
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                var consoleRun = new ConsoleRun(args, Help);
                consoleRun.Run();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }

        private static void Help(List<ComboBoxShowAttribute> cbxImportItems)
        {
            Console.WriteLine("-i:输入的词库类型 词库路径1 词库路径2 词库路径3 -o:输出的词库类型 输出词库路径 -c:编码文件路径");
            Console.WriteLine("输入和输出的词库类型如下：");
            var defaultBColor = Console.BackgroundColor;
            var defaultFColor = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (ComboBoxShowAttribute comboBoxShowAttribute in cbxImportItems)
            {
                Console.WriteLine(
                    comboBoxShowAttribute.ShortCode + "\t" + comboBoxShowAttribute.Name
                );
            }

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("例如要将C:\\test.scel和C:\\a.scel的搜狗细胞词库转换为D:\\gg.txt的谷歌拼音词库，命令为：");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "深蓝词库转换.exe -i:"
                    + ConstantString.SOUGOU_XIBAO_SCEL_C
                    + " C:\\test.scel C:\\a.scel -o:"
                    + ConstantString.GOOGLE_PINYIN_C
                    + " D:\\gg.txt"
            );

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                "例如要将C:\\test.scel和C:\\a.scel的搜狗细胞词库转换为D:\\temp文件夹下的谷歌拼音词库test.txt和a.txt，命令为："
            );
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "深蓝词库转换.exe -i:"
                    + ConstantString.SOUGOU_XIBAO_SCEL_C
                    + " C:\\test.scel C:\\a.scel -o:"
                    + ConstantString.GOOGLE_PINYIN_C
                    + " D:\\temp\\*"
            );
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("例如要将C:\\test\\*.scel的搜狗细胞词库转换为D:\\temp文件夹下的谷歌拼音词库，命令为：");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "深蓝词库转换.exe -i:"
                    + ConstantString.SOUGOU_XIBAO_SCEL_C
                    + " C:\\test\\*.scel -o:"
                    + ConstantString.GOOGLE_PINYIN_C
                    + " D:\\temp\\*"
            );

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                "对于导出词库为Rime输入法的，可以通过-ct:pinyin/wubi/zhengma设置编码，也可通过-os:windows/macos/linux设置适用的操作系统"
            );

            Console.WriteLine("自定义格式的参数如下:");
            Console.WriteLine("-f:213,|byyn");
            Console.WriteLine("213 这里是设置拼音、汉字和词频的顺序，213表示1汉字2拼音3词频，必须要有3个");
            Console.WriteLine(", 这里是设置拼音之间的分隔符，用逗号分割");
            Console.WriteLine("| 这里是设置汉字拼音词频之间的分隔符，用|分割");
            Console.WriteLine("b 这里是设置拼音分隔符的位置，有lrbn四个选项，l表示左包含，r表示右包含，b表示两边都包含，n表示两边都不包含");
            Console.WriteLine("yyn 这里是设置拼音汉字词频这3个是否显示，y表示显示，b表示不显示，这里yyn表示显示拼音和汉字，不显示词频");
            Console.WriteLine(
                "例如要将一个qpyd词库转换为自定义格式的文本词库，拼音之间逗号分割，拼音和词之间空格分割，不显示词频，同时使用自定义的编码文件code.txt命令如下："
            );
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "深蓝词库转换.exe -i:qpyd D:\\a.qpyd -o:self D:\\zy.txt \"-f:213, nyyn\" -c:D:\\code.txt"
            );
            Console.ForegroundColor = defaultFColor;
            Console.BackgroundColor = defaultBColor;
        }
    }
}
