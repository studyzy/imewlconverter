using System;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public partial class RimeConfigForm : Form
    {
        public RimeConfigForm()
        {
            InitializeComponent();
        }

        public CodeType SelectedCodeType { get; set; }
        public OperationSystem SelectedOS { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            switch (cbxCodeType.Text)
            {
                case "拼音":
                    SelectedCodeType = CodeType.Pinyin;
                    break;
                case "五笔":
                    SelectedCodeType = CodeType.Wubi;
                    break;
                case "注音":
                    SelectedCodeType = CodeType.TerraPinyin;
                    break;
                case "仓颉":
                    SelectedCodeType = CodeType.Cangjie;
                    break;
                case "其他":
                    SelectedCodeType = CodeType.Unknown;
                    break;
                default:
                    SelectedCodeType = CodeType.Unknown;
                    break;
            }
            if (rbWin.Checked)
            {
                SelectedOS = OperationSystem.Windows;
            }
            else if (rbMac.Checked)
            {
                SelectedOS = OperationSystem.MacOS;
            }
            else
            {
                SelectedOS = OperationSystem.Linux;
            }
            DialogResult = DialogResult.OK;
        }
    }
}