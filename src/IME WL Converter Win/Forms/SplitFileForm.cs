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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ImeWlConverter.Application.MergeSplit;
using ImeWlConverter.Core.Helpers;

namespace Studyzy.IMEWLConverter;

/// <summary>
/// 文件分割窗口。分割算法在 <see cref="MergeSplitService"/>（三端共享），此处仅是 UI 壳。
/// </summary>
public partial class SplitFileForm : Form
{
    public SplitFileForm()
    {
        InitializeComponent();
    }

    private void btnSelectFile_Click(object sender, EventArgs e)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK) txbFilePath.Text = openFileDialog1.FileName;
    }

    private void btnSplit_Click(object sender, EventArgs e)
    {
        if (txbFilePath.Text == "")
        {
            MessageBox.Show("请先选择要分割的文件", "分割", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(txbFilePath.Text))
        {
            MessageBox.Show(
                txbFilePath.Text + "，该文件不存在",
                "分割",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        rtbLogs.Clear();

        var mode = rbtnSplitByLine.Checked ? SplitMode.ByLine
            : rbtnSplitBySize.Checked ? SplitMode.BySize
            : SplitMode.ByLength;
        var max = mode == SplitMode.ByLine ? (int)numdMaxLine.Value
            : mode == SplitMode.BySize ? (int)numdMaxSize.Value
            : (int)numdMaxLength.Value;

        try
        {
            var parts = MergeSplitService.SplitFile(txbFilePath.Text, new SplitOptions { Mode = mode, Max = max });
            foreach (var part in parts)
                rtbLogs.AppendText(part + "\r\n");
        }
        catch (InvalidDataException ex)
        {
            MessageBox.Show(ex.Message, "分割", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("恭喜你，文件分割完成!", "分割", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
