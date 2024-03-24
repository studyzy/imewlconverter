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
using System.Reflection;
using System.Text;

namespace Studyzy.IMEWLConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Console.WriteLine("Hello World!");
            //var cj = Helpers.DictionaryHelper.GetResourceContent("Cangjie5.txt");
            //Console.WriteLine(cj);
            var consoleRun = new ConsoleRun(args, Help);
            consoleRun.Run();
        }

        private static void Help(List<ComboBoxShowAttribute> cbxImportItems)
        {
            Console.WriteLine("当前版本：V" + ConstantString.VERSION);
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
            Console.WriteLine("例如要将./test.scel和./a.scel的搜狗细胞词库转换为./gg.txt的谷歌拼音词库，命令为：");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "dotnet ImeWlConverterCmd.dll -i:"
                    + ConstantString.SOUGOU_XIBAO_SCEL_C
                    + " ./test.scel ./a.scel -o:"
                    + ConstantString.GOOGLE_PINYIN_C
                    + " ./gg.txt"
            );

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                "例如要将./test.scel和./a.scel的搜狗细胞词库转换为./temp文件夹下的谷歌拼音词库test.txt和a.txt，命令为："
            );
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "dotnet ImeWlConverterCmd.dll -i:"
                    + ConstantString.SOUGOU_XIBAO_SCEL_C
                    + " ./test.scel ./a.scel -o:"
                    + ConstantString.GOOGLE_PINYIN_C
                    + " ./temp/*"
            );
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("例如要将./test/*.scel的搜狗细胞词库转换为./temp文件夹下的谷歌拼音词库，命令为：");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "dotnet ImeWlConverterCmd.dll -i:"
                    + ConstantString.SOUGOU_XIBAO_SCEL_C
                    + " ./test/*.scel -o:"
                    + ConstantString.GOOGLE_PINYIN_C
                    + " ./temp/*"
            );
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("对于导入词库不包含词频，而导出时需要指定词频，可以通过-r:命令指定词频的生成方式，支持的有：");
            Console.WriteLine("-r:baidu  根据该词语在百度搜索的结果数量决定词频");
            Console.WriteLine("-r:google  根据该词语在Google搜索的结果数量决定词频(需翻墙)");
            Console.WriteLine("-r:数字  指定一个固定数字的词频");
            Console.WriteLine("");
            Console.WriteLine(
                "对于导出词库为Rime输入法的，可以通过-ct:pinyin/wubi/zhengma设置编码，也可通过-os:windows/macos/linux设置适用的操作系统"
            );
            Console.WriteLine("");
            Console.WriteLine("使用-ft:可以设置词条的过滤条件，如果不设置则不过滤任何词条。-ft:后面可以设置的过滤条件包括：");
            Console.WriteLine("len:1-100 保留字数为1到100的词条");
            Console.WriteLine("rank:2-9999 保留词频在2到9999的词条");
            Console.WriteLine("rm:eng 移除包含英文字母的词条");
            Console.WriteLine("rm:num 移除包含数字的词条");
            Console.WriteLine("rm:space 移除包含空格的词条");
            Console.WriteLine("rm:pun 移除包含标点符号的词条");
            Console.WriteLine("以上过滤条件可以组合，同时起作用，用竖线分开即可：");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-ft:\"len:1-100|rank:2-9999|rm:eng|rm:num|rm:space|rm:pun\"");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
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
                "dotnet ImeWlConverterCmd.dll -i:qpyd ./a.qpyd -o:self ./zy.txt \"-f:213, nyyn\" -c:./code.txt"
            );
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("其中-c:./code.txt指定的编码文件格式为：“汉字<Tab键>编码”每行一个。");
            Console.ForegroundColor = defaultFColor;
            Console.BackgroundColor = defaultBColor;
            Console.WriteLine("");
            Console.WriteLine("最后，如果这款软件帮助到了您，您可以通过捐赠表示感谢，捐赠作者支付宝地址：studyzy@163.com 曾毅");
        }
    }
}
