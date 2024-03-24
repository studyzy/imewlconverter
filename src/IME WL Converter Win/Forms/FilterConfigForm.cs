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
    public partial class FilterConfigForm : Form
    {
        private static FilterConfig filterConfig;

        public FilterConfigForm()
        {
            InitializeComponent();
            if (filterConfig == null)
            {
                filterConfig = new FilterConfig();
            }
        }

        public FilterConfig FilterConfig
        {
            get { return filterConfig; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            filterConfig.WordLengthFrom = Convert.ToInt32(numWordLengthFrom.Value);
            filterConfig.WordLengthTo = Convert.ToInt32(numWordLengthTo.Value);
            filterConfig.WordRankFrom = Convert.ToInt32(numWordRankFrom.Value);
            filterConfig.WordRankTo = Convert.ToInt32(numWordRankTo.Value);
            filterConfig.WordRankPercentage = Convert.ToInt32(numWordRankPercentage.Value);
            filterConfig.IgnoreEnglish = cbxFilterEnglish.Checked;
            filterConfig.IgnoreSpace = cbxFilterSpace.Checked;
            filterConfig.IgnorePunctuation = cbxFilterPunctuation.Checked;
            filterConfig.IgnoreNumber = cbxFilterNumber.Checked;
            filterConfig.IgnoreNoAlphabetCode = cbxFilterNoAlphabetCode.Checked;
            filterConfig.NoFilter = cbxNoFilter.Checked;

            filterConfig.ReplaceNumber = cbxReplaceNumber.Checked;
            filterConfig.ReplaceEnglish = cbxReplaceEnglish.Checked;
            filterConfig.ReplaceSpace = cbxReplaceSpace.Checked;
            filterConfig.ReplacePunctuation = cbxReplacePunctuation.Checked;

            filterConfig.KeepEnglish = cbxKeepEnglish.Checked;
            filterConfig.KeepNumber = cbxKeepNumber.Checked;
            filterConfig.KeepPunctuation = cbxKeepPunctuation.Checked;

            filterConfig.KeepEnglish_ = cbxKeepEnglish_.Checked;
            filterConfig.KeepNumber_ = cbxKeepNumber_.Checked;

            filterConfig.KeepPunctuation_ = cbxKeepPunctuation_.Checked;

            filterConfig.FullWidth = cbxFullWidth.Checked;
            filterConfig.ChsNumber = cbxChsNumber.Checked;
            filterConfig.PrefixEnglish = cbxPrefixEnglish.Checked;

            filterConfig.IgnoreFirstCJK = cbxFilterFirstCJK.Checked;

            filterConfig.KeepSpace_ = cbxKeepSpace_.Checked;
            filterConfig.KeepSpace = cbxKeepSpace.Checked;

            DialogResult = DialogResult.OK;
        }

        private void FilterConfigForm_Load(object sender, EventArgs e)
        {
            numWordLengthFrom.Value = filterConfig.WordLengthFrom;
            numWordLengthTo.Value = filterConfig.WordLengthTo;
            numWordRankFrom.Value = filterConfig.WordRankFrom;
            numWordRankTo.Value = filterConfig.WordRankTo;
            numWordRankPercentage.Value = filterConfig.WordRankPercentage;
            cbxFilterEnglish.Checked = filterConfig.IgnoreEnglish;
            cbxFilterSpace.Checked = filterConfig.IgnoreSpace;
            cbxFilterPunctuation.Checked = filterConfig.IgnorePunctuation;
            cbxNoFilter.Checked = filterConfig.NoFilter;
            cbxFilterNumber.Checked = filterConfig.IgnoreNumber;
            cbxReplaceEnglish.Checked = filterConfig.ReplaceEnglish;
            cbxReplaceNumber.Checked = filterConfig.ReplaceNumber;
            cbxReplacePunctuation.Checked = filterConfig.ReplacePunctuation;
            cbxReplaceSpace.Checked = filterConfig.ReplaceSpace;
            cbxFilterNoAlphabetCode.Checked = filterConfig.IgnoreNoAlphabetCode;
            cbxKeepEnglish.Checked = filterConfig.KeepEnglish;
            cbxKeepNumber.Checked = filterConfig.KeepNumber;
            cbxKeepEnglish_.Checked = filterConfig.KeepEnglish_;
            cbxKeepNumber_.Checked = filterConfig.KeepNumber_;
            cbxFilterFirstCJK.Checked = filterConfig.IgnoreFirstCJK;
            cbxKeepPunctuation.Checked = filterConfig.KeepPunctuation;
            cbxKeepPunctuation_.Checked = filterConfig.KeepPunctuation_;
            cbxFullWidth.Checked = filterConfig.FullWidth;
            cbxChsNumber.Checked = filterConfig.ChsNumber;
            cbxPrefixEnglish.Checked = filterConfig.PrefixEnglish;
            cbxKeepSpace_.Checked = filterConfig.KeepSpace_;
            cbxKeepSpace.Checked = filterConfig.KeepSpace;
        }

        private void cbxKeepNumber_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxKeepNumber.Checked)
            {
                cbxKeepNumber_.Checked = false;
                cbxChsNumber.Checked = false;
            }
        }

        private void cbxKeepNumber__CheckedChanged(object sender, EventArgs e)
        {
            if (cbxKeepNumber_.Checked)
            {
                cbxKeepNumber.Checked = false;
                cbxChsNumber.Checked = false;
            }
        }

        private void cbxKeepEnglish_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxKeepEnglish.Checked)
            {
                cbxKeepEnglish_.Checked = false;
            }
        }

        private void cbxKeepEnglish__CheckedChanged(object sender, EventArgs e)
        {
            if (cbxKeepEnglish_.Checked)
            {
                cbxKeepEnglish.Checked = false;
                cbxPrefixEnglish.Checked = false;
            }
        }

        private void cbxPrefixEnglish_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxPrefixEnglish.Checked)
            {
                cbxKeepEnglish_.Checked = false;
                cbxKeepEnglish.Checked = true;
            }
        }

        private void cbxKeepPunctuator_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxKeepPunctuation.Checked)
            {
                cbxKeepPunctuation_.Checked = false;
            }
        }

        private void cbxKeepPunctuation__CheckedChanged(object sender, EventArgs e)
        {
            if (cbxKeepPunctuation_.Checked)
            {
                cbxKeepPunctuation.Checked = false;
            }
        }

        private void cbxChsNumber_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxChsNumber.Checked)
            {
                cbxKeepNumber.Checked = false;
                cbxKeepNumber_.Checked = false;
            }
        }
    }
}
