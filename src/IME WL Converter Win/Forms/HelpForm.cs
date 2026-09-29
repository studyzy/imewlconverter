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
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter;

public partial class HelpForm : Form
{
    public HelpForm()
    {
        InitializeComponent();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void HelpForm_Load(object sender, EventArgs e)
    {
        var helpString =
            "深蓝词库转换 - 版本更新记录\r\n\r\n";
        helpString += "注意：该小工具基于C#开发，需要.NET 10.0运行时环境，支持Windows、Linux和macOS平台。\r\n\r\n";
        helpString += "1.1版支持搜狗的细胞词库（scel格式）的转换，您可以到搜狗网站下载细胞词库导入到您其他输入法或者手机输入法中！\r\n";
        helpString += "1.2版支持了紫光拼音输入法和拼音加加输入法的词库导入导出功能。增加了批量导入的功能。修复了有些scel格式词库导入时报错。\r\n";
        helpString += "1.3版增强了多音字注音功能和外挂多音字注音词库功能，另外还提供了直接导出而不显示转换结果的选项。\r\n";
        helpString += "1.4版增加了对触宝输入法的支持，增加了拖拽功能。\r\n";
        helpString += "1.5版增加了百度分类词库bdict格式的转换，增加了命令行调用功能。\r\n";
        helpString += "1.6版修改了搜狗细胞词库解析和QQ手机词库解析的函数，支持最新格式。\r\n";
        helpString += "1.7版增加了QQ分类词库（qpyd格式）的转换，调整了下拉列表的顺序，增加了拖拽文件时的文件类型自动识别等功能。\r\n";
        helpString += "1.8版增加了自定义编码的输出，增强了命令行功能。实现了百度手机分类词库（bcd格式）、小小输入法和微软拼音输入法的词库功能。\r\n";
        helpString += "1.9版增加了微软英库拼音输入法、FIT输入法、搜狗Bin格式备份词库、中州韵（小狼毫、鼠须管）、各种常用五笔输入法的支持，增加词库文件分割功能。\r\n";
        helpString += "2.0版支持多种编码的Rime输入法和小小输入法，增加了灵格斯ld2格式、简繁体转换、雅虎奇摩、仓颉平台支持，增强了五笔和郑码输入法支持。\r\n";
        helpString += "2.1版修复了自定义导出Bug，增加了超音速写、手心输入法等支持，增加了批量转换一对一和词库合并功能。升级为.NET 3.5。\r\n";
        helpString += "2.2版全面支持Win10微软拼音输入法词库导入导出。\r\n";
        helpString += "2.3版增加了文件夹作为源的功能，支持将文件夹中所有词库文件批量转换。\r\n";
        helpString += "2.4版增加了最新搜狗输入法备份词库的解析。\r\n";
        helpString += "2.5版增加了Win10微软五笔支持，增加了Linux和macOS命令行模式。\r\n";
        helpString += "2.6版增加了对Emoji颜文字的支持，微软拼音自定义短语支持小鹤双拼编码。\r\n";
        helpString += "2.7版增加了QQ拼音新细胞词库qcel格式和MacOS原生拼音自定义短语plist支持。升级dotnet到3.1。\r\n";
        helpString += "2.8版增加了微软拼音自学习词库的导入导出功能，增强双拼支持。\r\n";
        helpString += "2.9版增加了对GBoard手机输入法的词库导入导出功能。\r\n";
        helpString += "3.0版增加了新世纪五笔的支持，升级dotnet到6.0。\r\n";
        helpString += "3.1版增加了对rime userdb的支持，修复了搜狗备份词库解析问题，升级.NET到8.0。\r\n";
        helpString += "3.2版增加了百度拼音备份词库、LibIME拼音词库的支持，修复搜狗细胞词库和微软自学习词汇索引溢出问题。\r\n";
        helpString += "3.3版增加了macOS GUI应用（基于Avalonia UI），修复了自定义编码、Rime拼音码表、新世纪五笔生成器等问题。\r\n";
        helpString += "3.4版升级.NET到10.0，支持导出搜狗细胞词库scel格式，增加LLM词频生成功能，重构命令行参数为GNU风格，增加集成测试框架，修复多个Bug。\r\n";
        helpString += "3.5版修复了百度bdict词库旧版头部解析问题（此前官网下载及部分词库导入为空），新增Windows GUI自动化转换矩阵测试，覆盖35条转换路径，接入搜狗备份、百度bcd/bdict、紫光uwl、Gboard等多种真实词库测试样本。\r\n";
        helpString += "\r\n";
        helpString += "关于各种输入法的词库转换操作方法或提交新的Issue，请前往项目网站：\r\nhttps://github.com/studyzy/imewlconverter/\r\n\r\n";
        helpString += "有任何问题和建议请联系我：studyzy@163.com\r\n";

        richTextBox1.Text = helpString;
    }
}
