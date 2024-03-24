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
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter
{
    public partial class WordRankGenerateForm : Form
    {
        private static IWordRankGenerater wordRankGenerater = new DefaultWordRankGenerater();

        public WordRankGenerateForm()
        {
            InitializeComponent();
        }

        public IWordRankGenerater SelectedWordRankGenerater
        {
            get { return wordRankGenerater; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbtnDefault.Checked)
            {
                wordRankGenerater = new DefaultWordRankGenerater { Rank = (int)numRank.Value };
            }
            else if (rbtnGoogle.Checked)
            {
                wordRankGenerater = new GoogleWordRankGenerater();
            }
            else if (rbtnBaidu.Checked)
            {
                wordRankGenerater = new BaiduWordRankGenerater();
            }
            else if (rbtnCalc.Checked)
            {
                wordRankGenerater = new CalcWordRankGenerater();
            }
            wordRankGenerater.ForceUse = cbxForceUseNewRank.Checked;
            DialogResult = DialogResult.OK;
        }

        private void WordRankGenerateForm_Load(object sender, EventArgs e)
        {
            if (wordRankGenerater is DefaultWordRankGenerater)
            {
                rbtnDefault.Checked = true;
            }
            else if (wordRankGenerater is GoogleWordRankGenerater)
            {
                rbtnGoogle.Checked = true;
            }
            else if (wordRankGenerater is BaiduWordRankGenerater)
            {
                rbtnBaidu.Checked = true;
            }
            cbxForceUseNewRank.Checked = wordRankGenerater.ForceUse;
        }
    }
}
