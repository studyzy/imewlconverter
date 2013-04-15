
﻿using System;
using System.Collections.Generic;
﻿using System.Diagnostics;
﻿using System.IO;
﻿using System.Windows.Forms;
﻿using Studyzy.IMEWLConverter.Entities;
﻿using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter
{
    public partial class SelfDefiningConfigForm : Form
    {
        private readonly List<string> fromWords = new List<string>();

        private bool isImport = true;

        public SelfDefiningConfigForm()
        {
          
            InitializeComponent();
            InitParsePattern();
            IsImport = true;
        }
    

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
            SelectedParsePattern.CodeSplitString = GetSplitString(cbbxPinyinSplitString.Text);
            SelectedParsePattern.SplitString = GetSplitString(cbbxSplitString.Text);
            SelectedParsePattern.CodeSplitType = GetBuildType();
            SelectedParsePattern.Sort = GetSort();
            SelectedParsePattern.IsPinyinFormat = cbxCodeFormat.Text == "拼音格式";
            SelectedParsePattern.MutiWordCodeFormat = rtbCodeFormat.Text;
            if (!cbxIsPinyin.Checked)
            {
                if (string.IsNullOrEmpty(txbFilePath.Text) || !File.Exists(txbFilePath.Text))
                {
                    MessageBox.Show("未指定字符编码映射文件，无法对词库进行自定义编码的生成");
                    return false;
                }
                SelectedParsePattern.MappingTablePath = txbFilePath.Text;
            }
            return true;
        }
        public bool IsImport
        {
            get { return isImport; }
            set
            {
                isImport = value;

                btnParse.Visible = isImport;
                btnConvertTest.Visible = !isImport;
                lbFileSelect.Visible = !isImport;
                label3.Visible = !isImport;
                txbFilePath.Visible = !isImport;
                btnFileSelect.Visible = !isImport;
            }
        }

        public List<string> FromWords
        {
            get { return fromWords; }
        }

        /// <summary>
        /// 用户自定义的匹配模式
        /// </summary>
        public ParsePattern SelectedParsePattern { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(ReBuildUserPattern())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            if (SelectedParsePattern == null)
            {
                MessageBox.Show("请点击右上角按钮选择匹配规则");
                return;
            }
            rtbTo.Clear();
            try
            {
                string[] fromList = rtbFrom.Text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in fromList)
                {
                    string s = str.Trim();
                    WordLibrary wl = SelectedParsePattern.BuildWordLibrary(s);
                    rtbTo.AppendText(wl.ToDisplayString() + "\r\n");
                }
            }
            catch
            {
                MessageBox.Show("无法识别源内容，请确认源内容与自定义规则匹配！");
            }
        }

        //private void btnHelpBuild_Click(object sender, EventArgs e)
        //{
        //    var builder = new HelpBuildParsePatternForm();
        //    if (builder.ShowDialog() == DialogResult.OK)
        //    {
        //        SelectedParsePattern = builder.SelectedParsePattern;
        //        txbParsePattern.Text = SelectedParsePattern.BuildWLStringSample();
        //    }
        //}

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
            }
        }

        private void btnConvertTest_Click(object sender, EventArgs e)
        {
            if (!ReBuildUserPattern())
            {
                return;
            }
            rtbTo.Clear();
            string[] fromList = rtbFrom.Text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            SelfDefiningCodeGenerater generater=new SelfDefiningCodeGenerater();

            generater.MappingDictionary = UserCodingHelper.GetCodingDict(txbFilePath.Text);
            generater.MutiWordCodeFormat = SelectedParsePattern.MutiWordCodeFormat;
            foreach (string str in fromList)
            {
                string s = str.Trim();
                var code= generater.GetCodeOfString(s);
                string result = SelectedParsePattern.BuildWlString(s,code[0],1);
                rtbTo.AppendText(result + "\r\n");
            }
        }

        private void SelfDefiningConverterForm_Load(object sender, EventArgs e)
        {
            if (isImport)
            {
                rtbFrom.Text = @"shen,lan,ci,ku,zhuan,huan 深 1234
shen,lan,ci,ku,zhuan,huan 深蓝 1234
shen,lan,ci,ku,zhuan,huan 深蓝词 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库转 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库转换 1234";
            }
            else
            {
                rtbFrom.Text = "深\r\n深蓝\r\n深蓝词\r\n深蓝词库\r\n深蓝词库转\r\n深蓝词库转换";
            }
        }

        private void cbxIsPinyin_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxIsPinyin.Checked)
            {
                lbFileSelect.Visible = false;
                txbFilePath.Visible = false;
                btnFileSelect.Visible = false;
            }
            else
            {
                lbFileSelect.Visible = true;
                txbFilePath.Visible = true;
                btnFileSelect.Visible = true;
            }
            SelectedParsePattern.IsPinyin = cbxIsPinyin.Checked;
        }

        private void cbxIncludePinyin_CheckedChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.ContainCode = cbxIncludePinyin.Checked;
            numOrderPinyin.Visible = cbxIncludePinyin.Checked;
            ShowSample();
        }
        private void ShowSample()
        {
            this.rtbTo.Text = SelectedParsePattern.BuildWLStringSample();
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
                rtbCodeFormat.Text = @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            }
            if (cbxCodeFormat.Text == "拼音规则")
            {
                SelectedParsePattern.IsPinyinFormat = true;
            }
            else
            {
                SelectedParsePattern.IsPinyinFormat = false;
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

      
    }
}