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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImeWlConverter.Application.MergeSplit;
using ImeWlConverter.Core.Helpers;

namespace Studyzy.IMEWLConverter;

/// <summary>
/// 词库合并窗口。合并算法在 <see cref="MergeSplitService"/>（三端共享），此处仅是 UI 壳。
/// </summary>
public partial class MergeWLForm : Form
{
    public MergeWLForm()
    {
        InitializeComponent();
    }

    private void btnSelectMainWLFile_Click(object sender, EventArgs e)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK) txbMainWLFile.Text = openFileDialog1.FileName;
    }

    private void btnSelectUserWLFile_Click(object sender, EventArgs e)
    {
        if (openFileDialog2.ShowDialog() == DialogResult.OK)
            txbUserWLFiles.Text = string.Join(" | ", openFileDialog2.FileNames);
    }

    private void btnMergeWL_Click(object sender, EventArgs e)
    {
        var userFiles = txbUserWLFiles.Text.Split('|');
        var result = MergeSplitService.MergeFiles(txbMainWLFile.Text, userFiles, cbxSortByCode.Checked);

        richTextBox1.Text = result.Content;
        if (
            MessageBox.Show(
                "是否将合并的" + result.EntryCount + "条词库保存到本地硬盘上？",
                "是否保存",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) == DialogResult.Yes
        )
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                FileOperationHelper.WriteFile(
                    saveFileDialog1.FileName,
                    Encoding.Unicode,
                    result.Content
                );
    }

    private void MergeWLForm_Load(object sender, EventArgs e)
    {
        richTextBox1.Text =
            "请保证主词库和附加词库中每一行的格式为：\r\n编码 词1 词2 词3\r\n不要保留任何注释备注等。\r\n主词库只可选择一个，附加词库可多选";
    }
}
