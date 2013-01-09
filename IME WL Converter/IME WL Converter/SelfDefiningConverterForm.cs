
﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
﻿using Studyzy.IMEWLConverter.Entities;
﻿using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter
{
    public partial class SelfDefiningConverterForm : Form
    {
        private readonly List<string> fromWords = new List<string>();

        private bool isImport = true;

        public SelfDefiningConverterForm()
        {
            InitializeComponent();
        }

        public bool IsImport
        {
            get { return isImport; }
            set
            {
                isImport = value;

                btnParse.Visible = isImport;
                btnConvertTest.Visible = !isImport;
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
            DialogResult = DialogResult.OK;
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

        private void btnHelpBuild_Click(object sender, EventArgs e)
        {
            var builder = new HelpBuildParsePatternForm();
            if (builder.ShowDialog() == DialogResult.OK)
            {
                SelectedParsePattern = builder.SelectedParsePattern;
                txbParsePattern.Text = SelectedParsePattern.BuildWLStringSample();
            }
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
            }
        }

        private void btnConvertTest_Click(object sender, EventArgs e)
        {
            if (SelectedParsePattern == null)
            {
                MessageBox.Show("请点击右上角按钮选择匹配规则");
                return;
            }

            if (string.IsNullOrEmpty(txbFilePath.Text))
            {
                //不指定编码文件，那么必然是拼音
                //if (!SelectedParsePattern.IsPinyinFormat)
                //{
                //    MessageBox.Show("不是拼音编码，那么必须指定编码文件");
                //    return;
                //}
            }
            else
            {
                UserCodingHelper.FilePath = txbFilePath.Text;
            }
            rtbTo.Clear();
            string[] fromList = rtbFrom.Text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in fromList)
            {
                string s = str.Trim();
                var wl = new WordLibrary {Word = s};
                string result = SelectedParsePattern.BuildWLString(wl);
                rtbTo.AppendText(result + "\r\n");
            }
        }

        private void SelfDefiningConverterForm_Load(object sender, EventArgs e)
        {
            rtbFrom.Text = "深\r\n深蓝\r\n深蓝词\r\n深蓝词库\r\n深蓝词库转\r\n深蓝词库转换";
        }
    }
}