using System;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public partial class ErbiTypeForm : Form
    {
        public ErbiTypeForm()
        {
            InitializeComponent();
        }

        public CodeType SelectedCodeType { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            switch (cbxCodeType.Text)
            {
                case "青松二笔":
                    SelectedCodeType = CodeType.QingsongErbi;
                    break;
                case "超强二笔":
                    SelectedCodeType = CodeType.ChaoqiangErbi;
                    break;
                case "超强音形":
                    SelectedCodeType = CodeType.ChaoqingYinxin;
                    break;
                case "现代二笔":
                    SelectedCodeType = CodeType.XiandaiErbi;
                    break;

                default:
                    SelectedCodeType = CodeType.Unknown;
                    break;
            }
            DialogResult = DialogResult.OK;
        }
    }
}