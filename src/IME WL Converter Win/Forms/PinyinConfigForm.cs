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
using System.Windows.Forms;
using ImeWlConverter.Abstractions.Enums;

namespace Studyzy.IMEWLConverter;

public partial class PinyinConfigForm : Form
{
    private static PinyinType pinyinType;
    private static CodeType codeType;

    public PinyinConfigForm()
    {
        InitializeComponent();
    }

    public PinyinType SelectedPinyinType => pinyinType;
    public CodeType SelectedCodeType => codeType;

    /// <summary>
    /// 双拼方案名（与 Shuangpin.txt 表头逐字一致）。「全拼」「用户自定义短语」不转换。
    /// 用下拉框文本而非 PinyinType 枚举——枚举顺序与码表列并不对应。
    /// </summary>
    public string? SelectedShuangpinScheme =>
        cbxPinyinType.Text is "全拼" or "用户自定义短语" ? null : cbxPinyinType.Text;

    private void BtnOK_Click(object sender, EventArgs e)
    {
        codeType = CodeType.Pinyin;
        switch (cbxPinyinType.Text)
        {
            case "全拼":
                pinyinType = PinyinType.FullPinyin;
                break;
            case "微软双拼":
                pinyinType = PinyinType.Microsoft;
                break;
            case "小鹤双拼":
                pinyinType = PinyinType.XiaoHe;
                break;
            case "智能ABC":
                pinyinType = PinyinType.ZhiNengABC;
                break;
            case "自然码":
                pinyinType = PinyinType.ZiRanMa;
                break;
            case "拼音加加":
                pinyinType = PinyinType.PinyinJiaJia;
                break;
            default:
                pinyinType = PinyinType.FullPinyin;
                codeType = CodeType.UserDefinePhrase;
                break;
        }

        DialogResult = DialogResult.OK;
    }
}
