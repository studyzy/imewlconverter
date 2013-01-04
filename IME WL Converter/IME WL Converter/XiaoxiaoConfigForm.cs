using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class XiaoxiaoConfigForm : Form
    {
        public XiaoxiaoConfigForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            switch (cbxCodeType.Text)
            {
                case "拼音":SelectedCodeType=CodeType.Pinyin;break;
                case "五笔": SelectedCodeType = CodeType.Wubi; break;
                case "二笔": SelectedCodeType = CodeType.Erbi; break;
                case "英语": SelectedCodeType = CodeType.English; break;
                case "永码": SelectedCodeType = CodeType.Yong; break;
                case "郑码": SelectedCodeType = CodeType.Zhengma; break;
                case "内码": SelectedCodeType = CodeType.InnerCode; break;
                case "其他": SelectedCodeType = CodeType.Unknown; break;
                default:SelectedCodeType=CodeType.Unknown;break;
            }
            DialogResult=DialogResult.OK;
        }

        public CodeType SelectedCodeType { get; set; }
    }
}
