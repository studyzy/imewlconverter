using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class FilterConfigForm : Form
    {
        public FilterConfigForm()
        {
            InitializeComponent();
        }

        public int WordLengthFrom { get; set; }
        public int WordLengthTo { get; set; }

        public int WordRankFrom { get; set; }
        public int WordRankTo { get; set; }
        public bool IgnoreEnglish { get; set; }
        public bool IgnoreSpace { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            WordLengthFrom = Convert.ToInt32(numWordLengthFrom.Value);
            WordLengthTo = Convert.ToInt32(numWordLengthTo.Value);
            WordRankFrom = Convert.ToInt32(numWordRankFrom.Value);
            WordRankTo = Convert.ToInt32(numWordRankTo.Value);
            IgnoreEnglish = cbxFilterEnglish.Checked;
            IgnoreSpace = cbxFilterSpace.Checked;
            DialogResult=DialogResult.OK;
        }

        private void FilterConfigForm_Load(object sender, EventArgs e)
        {
            numWordLengthFrom.Value = WordLengthFrom;
            numWordLengthTo.Value = WordLengthTo;
            numWordRankFrom.Value = WordRankFrom;
            numWordRankTo.Value = WordRankTo;
            cbxFilterEnglish.Checked = IgnoreEnglish;
            cbxFilterSpace.Checked = IgnoreSpace;
        }
    }
}
