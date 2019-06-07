using Studyzy.IMEWLConverter.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class PinyinConfigForm : Form
    {
        public PinyinConfigForm()
        {
            InitializeComponent();
        }
        private static PinyinType pinyinType;
        private static CodeType codeType;
        public PinyinType SelectedPinyinType { get { return pinyinType; } }
        public CodeType SelectedCodeType { get { return codeType; } }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            switch (cbxPinyinType.Text)
            {
                case "全拼":pinyinType = PinyinType.FullPinyin;
                    codeType = CodeType.Pinyin;
                    break;
                case "小鹤双拼":pinyinType = PinyinType.XiaoheShuangpin; codeType = CodeType.Pinyin; break;
                default: pinyinType = PinyinType.FullPinyin; codeType = CodeType.UserDefinePhrase; break;
            }
            this.DialogResult= DialogResult.OK;
        }
    }
}
