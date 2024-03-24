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
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public partial class PinyinConfigForm : Form
    {
        public PinyinConfigForm()
        {
            InitializeComponent();
        }

        private static PinyinType pinyinType;
        private static CodeType codeType;
        public PinyinType SelectedPinyinType
        {
            get { return pinyinType; }
        }
        public CodeType SelectedCodeType
        {
            get { return codeType; }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            codeType = CodeType.Pinyin;
            switch (cbxPinyinType.Text)
            {
                case "全拼":
                    pinyinType = PinyinType.FullPinyin;
                    break;
                case "微软双拼":
                    pinyinType = PinyinType.MsShuangpin;
                    break;
                case "小鹤双拼":
                    pinyinType = PinyinType.XiaoheShuangpin;
                    break;
                case "智能ABC":
                    pinyinType = PinyinType.SmartABCShuangpin;
                    break;
                case "自然码":
                    pinyinType = PinyinType.NatureCode;
                    break;
                case "拼音加加":
                    pinyinType = PinyinType.PinyinJiajia;
                    break;
                case "星空键道":
                    pinyinType = PinyinType.XingkongJiandao;
                    break;
                case "大牛双拼":
                    pinyinType = PinyinType.DaniuShuangpin;
                    break;
                case "小浪双拼":
                    pinyinType = PinyinType.XiaolangShuangpin;
                    break;
                case "紫光拼音":
                    pinyinType = PinyinType.ZiguangShuangpin;
                    break;
                default:
                    pinyinType = PinyinType.FullPinyin;
                    codeType = CodeType.UserDefinePhrase;
                    break;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
