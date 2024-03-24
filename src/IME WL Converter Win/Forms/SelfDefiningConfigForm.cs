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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter
{
    public partial class SelfDefiningConfigForm : Form
    {
        private readonly List<string> fromWords = new List<string>();

        private readonly SelfDefining ime = new SelfDefining();

        //private bool isImport = true;

        public SelfDefiningConfigForm()
        {
            InitializeComponent();
            InitParsePattern();
            //IsImport = true;
        }

        /// <summary>
        ///     用户自定义的匹配模式
        /// </summary>
        public ParsePattern SelectedParsePattern { get; set; }

        private void InitParsePattern()
        {
            SelectedParsePattern = new ParsePattern();
            SelectedParsePattern.ContainRank = true;
            SelectedParsePattern.ContainCode = true;
            SelectedParsePattern.CodeSplitString = ",";
            SelectedParsePattern.CodeSplitType = BuildType.None;
            SelectedParsePattern.Sort = new List<int> { 1, 2, 3 };
            SelectedParsePattern.SplitString = " ";

            ShowSample();
        }

        private bool ReBuildUserPattern()
        {
            SelectedParsePattern.ContainCode = cbxIncludePinyin.Checked;
            SelectedParsePattern.ContainRank = cbxIncludeCipin.Checked;
            SelectedParsePattern.Sort = GetSort();

            SelectedParsePattern.CodeSplitString = GetSplitString(cbbxPinyinSplitString.Text);
            SelectedParsePattern.CodeSplitType = GetBuildType();

            SelectedParsePattern.LineSplitString = GetLineSplitString();
            SelectedParsePattern.SplitString = GetSplitString(cbbxSplitString.Text);

            SelectedParsePattern.CodeType =
                cbxCodeType.Text == "拼音编码" ? CodeType.Pinyin : CodeType.UserDefine;
            if (cbxCodeType.Text != "拼音编码")
            {
                if (string.IsNullOrEmpty(txbFilePath.Text) || !File.Exists(txbFilePath.Text))
                {
                    MessageBox.Show(
                        "未指定字符编码映射文件，无法对词库进行自定义编码的生成",
                        "自定义编码",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }
                SelectedParsePattern.TextEncoding = cbxTextEncoding.SelectedEncoding;
                SelectedParsePattern.MappingTablePath = txbFilePath.Text;
            }
            SelectedParsePattern.IsPinyinFormat = cbxCodeFormat.Text == "拼音规则";
            SelectedParsePattern.MutiWordCodeFormat = rtbCodeFormat.Text;
            return true;
        }

        private string GetLineSplitString()
        {
            switch (cbxLineSplitString.Text)
            {
                case "\\r\\n":
                    return "\r\n";
                case "\\r":
                    return "\r";
                case "\\n":
                    return "\n";
                default:
                    return cbxLineSplitString.Text;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ReBuildUserPattern())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SelfDefiningConverterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void btnFileSelect_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txbFilePath.Text = openFileDialog1.FileName;
                SelectedParsePattern.MappingTablePath = openFileDialog1.FileName;
                ShowSample();
            }
        }

        private void cbxIncludePinyin_CheckedChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.ContainCode = cbxIncludePinyin.Checked;
            numOrderPinyin.Visible = cbxIncludePinyin.Checked;
            ShowSample();
        }

        private void ShowSample()
        {
            if (ReBuildUserPattern())
            {
                ime.UserDefiningPattern = SelectedParsePattern;
                rtbTo.Text = ime.Export(SampleWL())[0];
            }
        }

        private WordLibraryList SampleWL()
        {
            var list = new WordLibraryList();
            string[] lines = rtbFrom.Text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries
            );
            foreach (string line in lines)
            {
                list.Add(
                    new WordLibrary
                    {
                        Word = line,
                        Rank = 1234,
                        CodeType = CodeType.NoCode
                    }
                );
            }
            return list;
        }

        private void cbxIncludeCipin_CheckedChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.ContainRank = cbxIncludeCipin.Checked;
            numOrderCipin.Visible = cbxIncludeCipin.Checked;
            ShowSample();
        }

        private string GetSplitString(string selectText)
        {
            string str = "";
            if (selectText == "空格")
            {
                str = " ";
            }
            else if (selectText.ToLower() == "tab")
            {
                str = "\t";
            }
            else if (selectText == "无")
            {
                str = "";
            }
            else
            {
                str = selectText;
            }
            return str;
        }

        private BuildType GetBuildType()
        {
            if (cbxPinyinSplitBefore.Checked && cbxPinyinSplitBehind.Checked)
            {
                return BuildType.FullContain;
            }
            if (cbxPinyinSplitBefore.Checked)
            {
                return BuildType.LeftContain;
            }
            if (cbxPinyinSplitBehind.Checked)
            {
                return BuildType.RightContain;
            }
            return BuildType.None;
        }

        private void cbbxPinyinSplitString_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.CodeSplitString = GetSplitString(cbbxPinyinSplitString.Text);
            ShowSample();
        }

        private void cbbxSplitString_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.SplitString = GetSplitString(cbbxSplitString.Text);
            ShowSample();
        }

        private void cbxPinyinSplit_CheckedChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.CodeSplitType = GetBuildType();
            ShowSample();
        }

        private List<int> GetSort()
        {
            var sort = new List<int>();
            int a = (int)numOrderPinyin.Value * 10;
            int b = (int)numOrderHanzi.Value * 10 + 1; //善重复键值问题
            int c = (int)numOrderCipin.Value * 10 + 2; //善重复键值问题
            sort.Add(a);
            sort.Add(b);
            sort.Add(c);
            return sort;
        }

        private void numOrderPinyin_ValueChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.Sort = GetSort();
            ShowSample();
        }

        private void cbxCodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxCodeFormat.Text == "五笔规则")
            {
                rtbCodeFormat.Text =
                    @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            }
            if (cbxCodeFormat.Text == "拼音规则")
            {
                SelectedParsePattern.IsPinyinFormat = true;
                rtbCodeFormat.Visible = false;
            }
            else
            {
                SelectedParsePattern.IsPinyinFormat = false;
                rtbCodeFormat.Visible = true;
            }
            SelectedParsePattern.MutiWordCodeFormat = rtbCodeFormat.Text;
            ShowSample();
        }

        private void rtbCodeFormat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedParsePattern.MutiWordCodeFormat = rtbCodeFormat.Text;
                ShowSample();
            }
            catch
            {
                Debug.WriteLine("输入格式不正确");
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            ShowSample();
        }

        //private IWordCodeGenerater pyFactory = new PinyinGenerater();
        //private SelfDefiningCodeGenerater selfFactory = new SelfDefiningCodeGenerater();
        //private void GenerateCode( WordLibrary wl)
        //{
        //    var word = wl.Word;
        //    if (SelectedParsePattern.IsPinyin&&SelectedParsePattern.IsPinyinFormat)
        //    {
        //        var py = pyFactory.GetCodeOfString(word, SelectedParsePattern.CodeSplitString);
        //        wl.PinYin = CollectionHelper.ToArray(py);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(SelectedParsePattern.MappingTablePath))
        //        {
        //            SelectedParsePattern.MappingTable = UserCodingHelper.GetCodingDict(SelectedParsePattern.MappingTablePath);
        //        }
        //        selfFactory.MappingDictionary = SelectedParsePattern.MappingTable;
        //        selfFactory.Is1Char1Code = SelectedParsePattern.IsPinyinFormat;
        //        selfFactory.MutiWordCodeFormat = SelectedParsePattern.MutiWordCodeFormat;
        //        wl.SetCode(CodeType.UserDefine, selfFactory.GetCodeOfString(word, SelectedParsePattern.CodeSplitString));
        //    }

        //}

        private void cbxCodeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxCodeType.Text == "拼音编码")
            {
                lbFileSelect.Visible = false;
                txbFilePath.Visible = false;
                btnFileSelect.Visible = false;
                cbxTextEncoding.Visible = false;
                SelectedParsePattern.CodeType = CodeType.Pinyin;
            }
            else
            {
                lbFileSelect.Visible = true;
                txbFilePath.Visible = true;
                btnFileSelect.Visible = true;
                cbxTextEncoding.Visible = true;
                SelectedParsePattern.CodeType = CodeType.UserDefine;
            }
        }
    }
}
