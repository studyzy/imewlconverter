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
        public Ld2EncodingConfigForm(Encoding encoding)
        {
            InitializeComponent();
            SelectedEncoding = encoding;

        }

        public Encoding SelectedEncoding { get { return cbxEncoding.SelectedEncoding; }
        set { cbxEncoding.SelectedEncoding = value; }}
        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
