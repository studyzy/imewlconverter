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
using ImeWlConverter.Abstractions.Options;

namespace Studyzy.IMEWLConverter;

public partial class FilterConfigForm : Form
{
    private static FilterConfig filterConfig = new();

    // UI-only state (not used by conversion pipeline)
    private static bool _keepEnglish = true;
    private static bool _keepNumber;
    private static bool _keepPunctuation;
    private static bool _keepSpace;
    private static bool _keepEnglishAlt;
    private static bool _keepNumberAlt = true;
    private static bool _keepPunctuationAlt = true;
    private static bool _keepSpaceAlt = true;
    private static bool _fullWidth = true;
    private static bool _chsNumber;
    private static bool _prefixEnglish = true;

    public FilterConfigForm()
    {
        InitializeComponent();
    }

    public FilterConfig FilterConfig => filterConfig;

    /// <summary>
    /// 编码生成选项（由 UI 上的 KeepEnglish/KeepNumber/PrefixEnglish 等 checkbox 决定）。
    /// </summary>
    public CodeGenerationOptions CodeGenerationOptions { get; private set; } = new();

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

        filterConfig.IgnoreFirstCJK = cbxFilterFirstCJK.Checked;

        // UI-only state
        _keepEnglish = cbxKeepEnglish.Checked;
        _keepNumber = cbxKeepNumber.Checked;
        _keepPunctuation = cbxKeepPunctuation.Checked;
        _keepEnglishAlt = cbxKeepEnglish_.Checked;
        _keepNumberAlt = cbxKeepNumber_.Checked;
        _keepPunctuationAlt = cbxKeepPunctuation_.Checked;
        _fullWidth = cbxFullWidth.Checked;
        _chsNumber = cbxChsNumber.Checked;
        _prefixEnglish = cbxPrefixEnglish.Checked;
        _keepSpaceAlt = cbxKeepSpace_.Checked;
        _keepSpace = cbxKeepSpace.Checked;

        CodeGenerationOptions = new CodeGenerationOptions
        {
            KeepEnglishInCode = _keepEnglish,
            KeepNumberInCode = _keepNumber,
            KeepPunctuationInCode = _keepPunctuation,
            PrefixEnglishWithUnderscore = _prefixEnglish,
            TranslateNumbersToChinese = _chsNumber,
            ConvertFullWidth = _fullWidth
        };

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
        cbxFilterFirstCJK.Checked = filterConfig.IgnoreFirstCJK;

        // UI-only state
        cbxKeepEnglish.Checked = _keepEnglish;
        cbxKeepNumber.Checked = _keepNumber;
        cbxKeepEnglish_.Checked = _keepEnglishAlt;
        cbxKeepNumber_.Checked = _keepNumberAlt;
        cbxKeepPunctuation.Checked = _keepPunctuation;
        cbxKeepPunctuation_.Checked = _keepPunctuationAlt;
        cbxFullWidth.Checked = _fullWidth;
        cbxChsNumber.Checked = _chsNumber;
        cbxPrefixEnglish.Checked = _prefixEnglish;
        cbxKeepSpace_.Checked = _keepSpaceAlt;
        cbxKeepSpace.Checked = _keepSpace;
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
        if (cbxKeepEnglish.Checked) cbxKeepEnglish_.Checked = false;
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
        if (cbxKeepPunctuation.Checked) cbxKeepPunctuation_.Checked = false;
    }

    private void cbxKeepPunctuation__CheckedChanged(object sender, EventArgs e)
    {
        if (cbxKeepPunctuation_.Checked) cbxKeepPunctuation.Checked = false;
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
