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
    public partial class ErbiTypeForm : Form
    {
        public ErbiTypeForm()
        {
            InitializeComponent();
        }

        public CodeType SelectedCodeType { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            switch (cbxCodeType.Text)
            {
                case "青松二笔":
                    SelectedCodeType = CodeType.QingsongErbi;
                    break;
                case "超强二笔":
                    SelectedCodeType = CodeType.ChaoqiangErbi;
                    break;
                case "超强音形":
                    SelectedCodeType = CodeType.ChaoqingYinxin;
                    break;
                case "现代二笔":
                    SelectedCodeType = CodeType.XiandaiErbi;
                    break;

                default:
                    SelectedCodeType = CodeType.Unknown;
                    break;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
