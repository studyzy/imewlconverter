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
using Studyzy.IMEWLConverter.Language;

namespace Studyzy.IMEWLConverter
{
    public partial class ChineseConverterSelectForm : Form
    {
        private static int selectedTranslateIndex;
        private static int selectedConverterIndex;

        public ChineseConverterSelectForm()
        {
            InitializeComponent();
            SelectedConverter = new SystemKernel();
            SelectedTranslate = ChineseTranslate.NotTrans;
            if (selectedConverterIndex == 1)
            {
                rbtnKernel.Checked = false;
                rbtnOffice.Checked = true;
            }
            if (selectedTranslateIndex == 1)
            {
                rbtnNotTrans.Checked = false;
                rbtnTransToChs.Checked = true;
                rbtnTransToCht.Checked = false;
            }
            else if (selectedTranslateIndex == 2)
            {
                rbtnNotTrans.Checked = false;
                rbtnTransToChs.Checked = false;
                rbtnTransToCht.Checked = true;
            }
        }

        public ChineseTranslate SelectedTranslate { get; set; }
        public IChineseConverter SelectedConverter { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbtnKernel.Checked)
            {
                selectedConverterIndex = 0;
                SelectedConverter = new SystemKernel();
            }
            else if (rbtnOffice.Checked)
            {
                selectedConverterIndex = 1;
                SelectedConverter = new OfficeComponent();
            }
            if (rbtnNotTrans.Checked)
            {
                selectedTranslateIndex = 0;
                SelectedTranslate = ChineseTranslate.NotTrans;
            }
            else if (rbtnTransToChs.Checked)
            {
                selectedTranslateIndex = 1;
                SelectedTranslate = ChineseTranslate.Trans2Chs;
            }
            else if (rbtnTransToCht.Checked)
            {
                selectedTranslateIndex = 2;
                SelectedTranslate = ChineseTranslate.Trans2Cht;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
