using System;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class PhraseFormatConfigForm : Form
    {
        private static int selectIndex;
        private static string userFormat = "编码<排序位置>短语";
        private static int selectRank = 2;
        private int defaultRank = 2;
        private string phraseFormat;

        public PhraseFormatConfigForm()
        {
            InitializeComponent();
        }

        public string PhraseFormat
        {
            get { return phraseFormat; }
        }

        public int DefaultRank
        {
            get { return defaultRank; }
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

            defaultRank = (int) numRank.Value;

            selectRank = defaultRank;
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
            numRank.Value = selectRank;
        }
    }
}