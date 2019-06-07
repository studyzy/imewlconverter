using Studyzy.IMEWLConverter.Entities;
using System;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class PhraseFormatConfigForm : Form
    {
        private static int selectIndex;
        private static string userFormat = "编码<排序位置>短语";
        private static CodeType selectCodeType = CodeType.UserDefinePhrase;

        private string phraseFormat;

        public PhraseFormatConfigForm()
        {
            InitializeComponent();
        }

        public string PhraseFormat
        {
            get { return phraseFormat; }
        }

        public CodeType SelectedCodeType
        {
            get { return selectCodeType; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbtnSougouFormat.Checked)
            {
                phraseFormat = "{1},{2}={0}";
                selectIndex = 0;
            }
            else if (rbtnQQFormat.Checked)
            {
                phraseFormat = "{1}={2},{0}";
                selectIndex = 1;
            }
            else if (rbtnBaiduFormat.Checked)
            {
                phraseFormat = "{2},{1}={0}";
                selectIndex = 2;
            }
            else
            {
                phraseFormat = txbUserFormat.Text.Replace("短语", "{0}").Replace("编码", "{1}").Replace("排序位置", "{2}");
                selectIndex = 100;
            }
            switch (cbxCodeType.Text)
            {
                case "用户自定义短语":
                    selectCodeType = CodeType.UserDefinePhrase;break;
                case "拼音":
                    selectCodeType = CodeType.Pinyin;break;
                case "五笔":
                    selectCodeType = CodeType.Wubi98;break;
                default:
                    selectCodeType = CodeType.UserDefinePhrase; break;
            }
         
            userFormat = txbUserFormat.Text;
            DialogResult = DialogResult.OK;
        }

        private void PhraseFormatConfigForm_Load(object sender, EventArgs e)
        {
            switch (selectIndex)
            {
                case 0:
                {
                    rbtnSougouFormat.Checked = true;
                }
                    break;
                case 1:
                {
                    rbtnQQFormat.Checked = true;
                }
                    break;
                case 2:
                {
                    rbtnBaiduFormat.Checked = true;
                }
                    break;
                default:
                {
                    rbtnUserFormat.Checked = true;
                }
                    break;
            }
            txbUserFormat.Text = userFormat;
            switch (selectCodeType)
            {
                case CodeType.UserDefinePhrase:cbxCodeType.Text = "用户自定义短语";break;
                case CodeType.Pinyin:cbxCodeType.Text = "拼音";break;
                case CodeType.Wubi98:cbxCodeType.Text = "五笔";break;
                default: cbxCodeType.Text = "用户自定义短语"; break;
            }
        }
    }
}