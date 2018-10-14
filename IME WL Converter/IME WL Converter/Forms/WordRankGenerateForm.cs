using System;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter
{
    public partial class WordRankGenerateForm : Form
    {
        private static IWordRankGenerater wordRankGenerater = new DefaultWordRankGenerater();

        public WordRankGenerateForm()
        {
            InitializeComponent();
        }

        public IWordRankGenerater SelectedWordRankGenerater
        {
            get { return wordRankGenerater; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbtnDefault.Checked)
            {
                wordRankGenerater = new DefaultWordRankGenerater {Rank = (int) numRank.Value};
            }
            else if (rbtnGoogle.Checked)
            {
                wordRankGenerater = new GoogleWordRankGenerater();
            }
            else if (rbtnBaidu.Checked)
            {
                wordRankGenerater = new BaiduWordRankGenerater();
            }
            else if (rbtnCalc.Checked)
            {
                wordRankGenerater = new CalcWordRankGenerater();
            }
            wordRankGenerater.ForceUse = cbxForceUseNewRank.Checked;
            DialogResult = DialogResult.OK;
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
            cbxForceUseNewRank.Checked = wordRankGenerater.ForceUse;
        }
    }
}