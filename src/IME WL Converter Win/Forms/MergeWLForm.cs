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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter
{
    public partial class MergeWLForm : Form
    {
        public MergeWLForm()
        {
            InitializeComponent();
        }

        //private Dictionary<string,List<string>> main
        private void btnSelectMainWLFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txbMainWLFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnSelectUserWLFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                txbUserWLFiles.Text = string.Join(" | ", openFileDialog2.FileNames);
            }
        }

        private void btnMergeWL_Click(object sender, EventArgs e)
        {
            string mainWL = FileOperationHelper.ReadFile(txbMainWLFile.Text);
            Dictionary<string, List<string>> mainDict = ConvertTxt2Dictionary(mainWL);
            string[] userFiles = txbUserWLFiles.Text.Split('|');
            foreach (string userFile in userFiles)
            {
                string filePath = userFile.Trim();
                string userTxt = FileOperationHelper.ReadFile(filePath);
                Dictionary<string, List<string>> userDict = ConvertTxt2Dictionary(userTxt);
                Merge2Dict(mainDict, userDict);
            }
            if (cbxSortByCode.Checked)
            {
                var keys = new List<string>(mainDict.Keys);
                keys.Sort();
                var sortedDict = new Dictionary<string, List<string>>();
                foreach (string key in keys)
                {
                    sortedDict.Add(key, mainDict[key]);
                }
                mainDict = sortedDict;
            }
            string result = Dict2String(mainDict);
            richTextBox1.Text = result;
            if (
                MessageBox.Show(
                    "是否将合并的" + mainDict.Count + "条词库保存到本地硬盘上？",
                    "是否保存",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) == DialogResult.Yes
            )
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    FileOperationHelper.WriteFile(
                        saveFileDialog1.FileName,
                        Encoding.Unicode,
                        result
                    );
                }
            }
        }

        private static Dictionary<string, List<string>> ConvertTxt2Dictionary(string txt)
        {
            string[] lines = txt.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var mainDict = new Dictionary<string, List<string>>();
            foreach (string line in lines)
            {
                string[] array = line.Split(' ');
                string key = array[0];
                if (!mainDict.ContainsKey(key))
                {
                    mainDict.Add(key, []);
                }
                for (int i = 1; i < array.Length; i++)
                {
                    string word = array[i];
                    mainDict[key].Add(word);
                }
            }
            return mainDict;
        }

        private static void Merge2Dict(
            Dictionary<string, List<string>> d1,
            Dictionary<string, List<string>> d2
        )
        {
            foreach (var pair in d2)
            {
                if (!d1.TryGetValue(pair.Key, out List<string> v))
                {
                    d1.Add(pair.Key, pair.Value);
                }
                else
                {
                    foreach (string word in pair.Value)
                    {
                        if (!v.Contains(word))
                        {
                            v.Add(word);
                        }
                    }
                }
            }
        }

        private void ShowMessage(string message)
        {
            richTextBox1.AppendText(message + "\r\n");
        }

        private static string Dict2String(Dictionary<string, List<string>> dictionary)
        {
            var sb = new StringBuilder();
            foreach (var pair in dictionary)
            {
                sb.Append(pair.Key);
                if (pair.Value != null && pair.Value.Count > 0)
                {
                    sb.Append(' ');
                    sb.Append(string.Join(" ", [.. pair.Value]));
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        private void MergeWLForm_Load(object sender, EventArgs e)
        {
            richTextBox1.Text =
                "请保证主词库和附加词库中每一行的格式为：\r\n编码 词1 词2 词3\r\n不要保留任何注释备注等。\r\n主词库只可选择一个，附加词库可多选";
        }
    }
}
