using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class Ld2EncodingConfigForm : Form
    {
        public Ld2EncodingConfigForm()
        {
            InitializeComponent();
            if(selectedEncoding==null)
            {
                selectedEncoding = Encoding.UTF8;
            }

        }

        private static Encoding selectedEncoding=null;

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
