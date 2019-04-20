using System;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public partial class FilterConfigForm : Form
    {
        private static FilterConfig filterConfig;

        public FilterConfigForm()
        {
            InitializeComponent();
            if (filterConfig == null)
            {
                filterConfig = new FilterConfig();
            }
        }

        public FilterConfig FilterConfig
        {
            get { return filterConfig; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            filterConfig.WordLengthFrom = Convert.ToInt32(numWordLengthFrom.Value);
            filterConfig.WordLengthTo = Convert.ToInt32(numWordLengthTo.Value);
            filterConfig.WordRankFrom = Convert.ToInt32(numWordRankFrom.Value);
            filterConfig.WordRankTo = Convert.ToInt32(numWordRankTo.Value);
            filterConfig.WordRankPercentage = Convert.ToInt32(numWordRankPercentage.Value);
            filterConfig.IgnoreEnglish = cbxFilterEnglish.Checked;
            filterConfig.IgnoreSpace = cbxFilterSpace.Checked;
            filterConfig.IgnorePunctuation = cbxFilterPunctuation.Checked;
            filterConfig.IgnoreNumber = cbxFilterNumber.Checked;
            filterConfig.NoFilter = cbxNoFilter.Checked;

            filterConfig.ReplaceNumber = cbxReplaceNumber.Checked;
            filterConfig.ReplaceEnglish = cbxReplaceEnglish.Checked;
            filterConfig.ReplaceSpace = cbxReplaceSpace.Checked;
            filterConfig.ReplacePunctuation = cbxReplacePunctuation.Checked;
            DialogResult = DialogResult.OK;
        }

        private void FilterConfigForm_Load(object sender, EventArgs e)
        {
            numWordLengthFrom.Value = filterConfig.WordLengthFrom;
            numWordLengthTo.Value = filterConfig.WordLengthTo;
            numWordRankFrom.Value = filterConfig.WordRankFrom;
            numWordRankTo.Value = filterConfig.WordRankTo;
            numWordRankPercentage.Value = filterConfig.WordRankPercentage;
            cbxFilterEnglish.Checked = filterConfig.IgnoreEnglish;
            cbxFilterSpace.Checked = filterConfig.IgnoreSpace;
            cbxFilterPunctuation.Checked = filterConfig.IgnorePunctuation;
            cbxNoFilter.Checked = filterConfig.NoFilter;
            cbxFilterNumber.Checked = filterConfig.IgnoreNumber;
            cbxReplaceEnglish.Checked = filterConfig.ReplaceEnglish;
            cbxReplaceNumber.Checked = filterConfig.ReplaceNumber;
            cbxReplacePunctuation.Checked = filterConfig.ReplacePunctuation;
            cbxReplaceSpace.Checked = filterConfig.ReplaceSpace;
        }
    }
}