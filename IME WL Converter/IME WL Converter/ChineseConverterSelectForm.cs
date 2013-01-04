using System;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Language;

namespace Studyzy.IMEWLConverter
{
    public partial class ChineseConverterSelectForm : Form
    {
        private static int selectedTranslateIndex;
        private static int selectedConverterIndex;

        public ChineseConverterSelectForm()
        {
            InitializeComponent();
            SelectedConverter = new SystemKernel();
            SelectedTranslate = ChineseTranslate.NotTrans;
            if (selectedConverterIndex == 1)
            {
                rbtnKernel.Checked = false;
                rbtnOffice.Checked = true;
            }
            if (selectedTranslateIndex == 1)
            {
                rbtnNotTrans.Checked = false;
                rbtnTransToChs.Checked = true;
                rbtnTransToCht.Checked = false;
            }
            else if (selectedTranslateIndex == 2)
            {
                rbtnNotTrans.Checked = false;
                rbtnTransToChs.Checked = false;
                rbtnTransToCht.Checked = true;
            }
        }

        public ChineseTranslate SelectedTranslate { get; set; }
        public IChineseConverter SelectedConverter { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbtnKernel.Checked)
            {
                selectedConverterIndex = 0;
                SelectedConverter = new SystemKernel();
            }
            else if (rbtnOffice.Checked)
            {
                selectedConverterIndex = 1;
                SelectedConverter = new OfficeComponent();
            }
            if (rbtnNotTrans.Checked)
            {
                selectedTranslateIndex = 0;
                SelectedTranslate = ChineseTranslate.NotTrans;
            }
            else if (rbtnTransToChs.Checked)
            {
                selectedTranslateIndex = 1;
                SelectedTranslate = ChineseTranslate.Trans2Chs;
            }
            else if (rbtnTransToCht.Checked)
            {
                selectedTranslateIndex = 2;
                SelectedTranslate = ChineseTranslate.Trans2Cht;
            }
            DialogResult = DialogResult.OK;
        }
    }
}