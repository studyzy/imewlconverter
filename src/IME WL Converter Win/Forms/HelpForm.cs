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

namespace Studyzy.IMEWLConverter
{
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
            string helpString =
                "1.1版支持搜狗的细胞词库（scel格式）的转换，您可以到搜狗网站下载细胞词库导入到您其他输入法或者手机输入法中！\r\nQQ的分类词库格式还没有研究出来怎么解析。\r\n";
            helpString += "1.2版支持了紫光拼音输入法和拼音加加输入法的词库导入导出功能。增加了批量导入的功能。修复了有些scel格式词库导入时报错\r\n";
            helpString += "1.3版改进汉字自动注音功能，可以对纯汉字的词库进行注音和转换；并可设置不显示转换结果而直接导出结果以提高超大词库的转换效率\r\n";
            helpString += "1.4版增加了对触宝输入法的支持，增加了拖拽功能。\r\n";
            helpString += "1.5版增加了百度分类词库bdict格式的转换，增加了命令行调用功能。\r\n";
            helpString += "1.6版修改了搜狗细胞词库解析和QQ手机词库解析的函数，支持最新格式。\r\n";
            helpString += "1.7版增加了对QQ输入法分类词库(qpyd格式)的解析，可像搜狗细胞词库一样的将QQ分类词库导为文本了！\r\n";
            helpString +=
                "1.8版增加了自定义编码的输出，增强了命令行功能。实现了百度手机分类词库（bcd格式）、小小输入法和微软拼音输入法的词库功能，但是由于输入法的原因，MS拼音可能会导入失败。\r\n";
            helpString +=
                "1.9版增加了微软英库拼音输入法、FIT输入法、搜狗Bin格式备份词库、中州韵（小狼毫、鼠须管）、各种常用五笔输入法的支持，增加词库文件分割功能。\r\n";
            helpString +=
                "2.0版增加了简繁体转换功能、支持多种五笔、郑码、仓颉、注音、二笔等输入法词库、百度拼音PC版、灵格斯ld2格式等，增加对仓颉平台、雅虎奇摩输入法的支持。加强了Rime输入法和小小输入法\r\n";
            helpString += "2.1版修复了自定义转换时的Bug，增加了超音速录、手心输入法等的支持。升级为.Net 3.5只要在Vista以上操作系统不需要安装.Net。";
            helpString += "2.2支持手心输入法和最新版Win10微软拼音（用户自定义短语）\r\n";
            helpString += "2.3支持Win10微软拼音（用户自定义短语）对其他输入法编码的支持\r\n";
            helpString +=
                "2.4版增加了最新搜狗输入法备份词库的解析，目前解析后只有词语和词频，没有拼音，所以拼音是根据词语重新生成的。（感谢GitHub上的h4x3rotab提供python版的解析算法，感谢tmxkn1提供了C#版的实现）\r\n";
            helpString += "2.5版支持微软五笔，支持Linux和macOS和更多命令行功能\r\n";
            helpString += "2.6版增加了对Emoji颜文字的支持，微软拼音自定义短语支持小鹤双拼编码\r\n";
            helpString +=
                "2.7版增加了对QQ拼音新细胞词库qcel格式的支持，增加了MacOS原生拼音自定义短语plist的支持。升级dotnet core到3.1。\r\n";
            helpString += "2.8版增加了对微软拼音自学习词库的导入导出功能，增强了微软拼音自定义短语对双拼的支持，增加错误日志输出，启用新的CI和CD。\r\n";
            helpString += "2.9版增加了对GBoard手机输入法的词库导入导出功能。\r\n";
            helpString += "3.0版增加了新世纪五笔的支持，并升级了依赖库版本和dotnet到6.0，修复了多个发现的Bug。\r\n";
            helpString +=
                "关于各种输入法的词库转换操作方法或提交新的Issue，请前往项目网站（https://github.com/studyzy/imewlconverter/）\r\n";
            helpString +=
                "如果您觉得深蓝词库转换能够给您的生活带来了极大的方便，可以通过微信或者支付宝捐赠该软件(https://github.com/studyzy/imewlconverter/wiki/Donate)。\r\n";
            helpString += "有任何问题和建议请联系我：studyzy@163.com\r\n";

            richTextBox1.Text = helpString;
        }
    }
}
