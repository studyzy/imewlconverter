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
    public partial class ErrorLogForm : Form
    {
        public ErrorLogForm(string message)
        {
            InitializeComponent();
            this.richTextBox1.Text = message;
        }
    }
}
