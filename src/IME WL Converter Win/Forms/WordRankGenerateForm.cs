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
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter;

public partial class WordRankGenerateForm : Form
{
    private static IWordRankGenerater wordRankGenerater = new DefaultWordRankGenerater();
    private static LlmConfig llmConfig = new LlmConfig();

    public WordRankGenerateForm()
    {
        InitializeComponent();
    }

    public IWordRankGenerater SelectedWordRankGenerater => wordRankGenerater;

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (rbtnDefault.Checked)
        {
            wordRankGenerater = new DefaultWordRankGenerater { Rank = (int)numRank.Value };
        }
        else if (rbtnLlm.Checked)
        {
            llmConfig.ApiEndpoint = txtLlmEndpoint.Text;
            llmConfig.ApiKey = txtLlmKey.Text;
            llmConfig.Model = txtLlmModel.Text;
            wordRankGenerater = new LlmWordRankGenerater(llmConfig);
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
        txtLlmEndpoint.Text = llmConfig.ApiEndpoint;
        txtLlmKey.Text = llmConfig.ApiKey;
        txtLlmModel.Text = llmConfig.Model;

        if (wordRankGenerater is DefaultWordRankGenerater)
            rbtnDefault.Checked = true;
        else if (wordRankGenerater is LlmWordRankGenerater)
            rbtnLlm.Checked = true;
        else if (wordRankGenerater is CalcWordRankGenerater)
            rbtnCalc.Checked = true;

        cbxForceUseNewRank.Checked = wordRankGenerater.ForceUse;
        UpdateLlmControls();
    }

    private void rbtnLlm_CheckedChanged(object sender, EventArgs e)
    {
        UpdateLlmControls();
    }

    private void UpdateLlmControls()
    {
        var enabled = rbtnLlm.Checked;
        txtLlmEndpoint.Enabled = enabled;
        txtLlmKey.Enabled = enabled;
        txtLlmModel.Enabled = enabled;
    }
}
