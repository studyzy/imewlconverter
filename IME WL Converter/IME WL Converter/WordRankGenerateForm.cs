using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter
{
    public partial class WordRankGenerateForm : Form
    {
        public WordRankGenerateForm()
        {
            InitializeComponent();
        }

        private static IWordRankGenerater wordRankGenerater=new DefaultWordRankGenerater();
        public IWordRankGenerater SelectedWordRankGenerater{get { return wordRankGenerater; }}
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbtnDefault.Checked)
            {
                wordRankGenerater=new DefaultWordRankGenerater(){Rank = (int)numRank.Value};
            }
            else if (rbtnGoogle.Checked)
            {
                wordRankGenerater=new GoogleWordRankGenerater();
            }
            else if (rbtnBaidu.Checked)
            {
                wordRankGenerater=new BaiduWordRankGenerater();
            }
            DialogResult=DialogResult.OK;
        }

        private void WordRankGenerateForm_Load(object sender, EventArgs e)
        {
            if (wordRankGenerater is DefaultWordRankGenerater)
            {
                rbtnDefault.Checked = true;
            }
            else if (wordRankGenerater is GoogleWordRankGenerater)
            {
                rbtnGoogle.Checked = true;
            }
            else if (wordRankGenerater is BaiduWordRankGenerater)
            {
                rbtnBaidu.Checked = true;
            }
        }
    }
}
