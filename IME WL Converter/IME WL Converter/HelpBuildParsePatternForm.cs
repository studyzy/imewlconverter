using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter
{
    public partial class HelpBuildParsePatternForm : Form
    {
        public HelpBuildParsePatternForm()
        {
            InitializeComponent();
            InitParsePattern();
        }

        public ParsePattern SelectedParsePattern { get; set; }

        private void InitParsePattern()
        {
            SelectedParsePattern = new ParsePattern();
            SelectedParsePattern.ContainRank = true;
            SelectedParsePattern.ContainCode = true;
            SelectedParsePattern.CodeSplitString = ",";
            SelectedParsePattern.CodeSplitType = BuildType.None;
            SelectedParsePattern.Sort = new List<int> {1, 2, 3};
            SelectedParsePattern.SplitString = " ";


            ShowSample();
        }

        private void HelpBuildParsePatternForm_Load(object sender, EventArgs e)
        {
        }

        private void cbxIncludePinyin_CheckedChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.ContainCode = cbxIncludePinyin.Checked;
            numOrderPinyin.Visible = cbxIncludePinyin.Checked;
            ShowSample();
        }

        private void cbxIncludeCipin_CheckedChanged(object sender, EventArgs e)
        {
            SelectedParsePattern.ContainRank = cbxIncludeCipin.Checked;
            numOrderCipin.Visible = cbxIncludeCipin.Checked;
            ShowSample();
        }

        private void ShowSample()
        {
            txbSample.Text = SelectedParsePattern.BuildWLStringSample();
        }

        private void cbbxPinyinSplitString_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = "";
            if (cbbxPinyinSplitString.Text == "空格")
            {
                str = " ";
            }
            else if (cbbxPinyinSplitString.Text.ToLower() == "tab")
            {
                str = "\t";
            }
            else if (cbbxPinyinSplitString.Text == "无")
            {
                str = "";
            }
            else
            {
                str = cbbxPinyinSplitString.Text;
            }
            SelectedParsePattern.CodeSplitString = str;
            ShowSample();
        }

        private void cbbxSplitString_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = "";
            if (cbbxSplitString.Text == "空格")
            {
                str = " ";
            }
            else if (cbbxSplitString.Text.ToLower() == "tab")
            {
                str = "\t";
            }
            else if (cbbxSplitString.Text == "无")
            {
                str = "";
            }
            else
            {
                str = cbbxSplitString.Text;
            }
            SelectedParsePattern.SplitString = str;
            ShowSample();
        }

        private void cbxPinyinSplitBefore_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxPinyinSplitBefore.Checked && cbxPinyinSplitBehind.Checked)
            {
                SelectedParsePattern.CodeSplitType = BuildType.FullContain;
            }
            else if (cbxPinyinSplitBefore.Checked)
            {
                SelectedParsePattern.CodeSplitType = BuildType.LeftContain;
            }
            else if (cbxPinyinSplitBehind.Checked)
            {
                SelectedParsePattern.CodeSplitType = BuildType.RightContain;
            }
            else
            {
                SelectedParsePattern.CodeSplitType = BuildType.None;
            }
            ShowSample();
        }

        private void numOrderPinyin_ValueChanged(object sender, EventArgs e)
        {
            var sort = new List<int>();
            int a = (int) numOrderPinyin.Value*10;
            int b = (int) numOrderHanzi.Value*10 + 1; //善重复键值问题
            int c = (int) numOrderCipin.Value*10 + 2; //善重复键值问题
            sort.Add(a);
            sort.Add(b);
            sort.Add(c);

            SelectedParsePattern.Sort = sort;
            ShowSample();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
           
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cbxCodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxCodeFormat.Text == "五笔格式")
            {
                rtbCodeFormat.Text = @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            }
            if (cbxCodeFormat.Text == "拼音格式")
            {
                SelectedParsePattern.IsPinyinFormat = true;

            }
            else
            {

                SelectedParsePattern.IsPinyinFormat = false;
                SelectedParsePattern.MutiWordCodeFormat = rtbCodeFormat.Text;
            }
            ShowSample();
        }
      
    }
}