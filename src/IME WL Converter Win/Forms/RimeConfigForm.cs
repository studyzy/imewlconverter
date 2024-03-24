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
    public partial class RimeConfigForm : Form
    {
        public RimeConfigForm()
        {
            InitializeComponent();
        }

        public CodeType SelectedCodeType { get; set; }
        public OperationSystem SelectedOS { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            switch (cbxCodeType.Text)
            {
                case "拼音":
                    SelectedCodeType = CodeType.Pinyin;
                    break;
                case "五笔":
                    SelectedCodeType = CodeType.Wubi;
                    break;
                case "注音":
                    SelectedCodeType = CodeType.TerraPinyin;
                    break;
                case "仓颉":
                    SelectedCodeType = CodeType.Cangjie;
                    break;
                case "其他":
                    SelectedCodeType = CodeType.Unknown;
                    break;
                default:
                    SelectedCodeType = CodeType.Unknown;
                    break;
            }
            if (rbWin.Checked)
            {
                SelectedOS = OperationSystem.Windows;
            }
            else if (rbMac.Checked)
            {
                SelectedOS = OperationSystem.MacOS;
            }
            else
            {
                SelectedOS = OperationSystem.Linux;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
