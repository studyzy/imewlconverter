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
            codeType = CodeType.Pinyin;
            switch (cbxPinyinType.Text)
            {
                case "全拼":pinyinType = PinyinType.FullPinyin;
                    break;
                case "微软双拼":
                    pinyinType = PinyinType.MsShuangpin; 
                    break;
                case "小鹤双拼":
                    pinyinType = PinyinType.XiaoheShuangpin;
                    break;
                case "智能ABC":
                    pinyinType = PinyinType.SmartABCShuangpin;
                    break;
                case "自然码":
                    pinyinType = PinyinType.NatureCode;
                    break;
                case "拼音加加":
                    pinyinType = PinyinType.PinyinJiajia;
                    break;
                case "星空键道":
                    pinyinType = PinyinType.XingkongJiandao;
                    break;
                case "大牛双拼":
                    pinyinType = PinyinType.DaniuShuangpin;
                    break;
                case "小浪双拼":
                    pinyinType = PinyinType.XiaolangShuangpin;
                    break;
                case "紫光拼音":
                    pinyinType = PinyinType.ZiguangShuangpin;
                    break;
                default: pinyinType = PinyinType.FullPinyin; codeType = CodeType.UserDefinePhrase; break;
            }
            this.DialogResult= DialogResult.OK;
        }
    }
}
