using System;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class Ld2EncodingConfigForm : Form
    {
        private static Encoding selectedEncoding;

        public Ld2EncodingConfigForm()
        {
            InitializeComponent();
            if (selectedEncoding == null)
            {
                selectedEncoding = Encoding.UTF8;
            }
        }

        public Encoding SelectedEncoding
        {
            get { return cbxEncoding.SelectedEncoding; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            selectedEncoding = cbxEncoding.SelectedEncoding;
            DialogResult = DialogResult.OK;
        }

        private void Ld2EncodingConfigForm_Load(object sender, EventArgs e)
        {
            cbxEncoding.SelectedEncoding = selectedEncoding;
        }
    }
}