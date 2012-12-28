using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class RimeConfigForm : Form
    {
        public RimeConfigForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            switch (cbxCodeType.Text)
            {
                case "拼音":SelectedCodeType=CodeType.Pinyin;break;
                case "五笔": SelectedCodeType = CodeType.Wubi; break;
                case "注音": SelectedCodeType = CodeType.TerraPinyin; break;
                case "仓颉": SelectedCodeType = CodeType.Cangjie; break;
                case "其他": SelectedCodeType = CodeType.Unknown; break;
                default:SelectedCodeType=CodeType.Unknown;break;
            }
            DialogResult=DialogResult.OK;
        }

        public CodeType SelectedCodeType { get; set; }
    }
}
