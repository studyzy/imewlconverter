using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public partial class DonateForm : Form
    {
        public DonateForm()
        {
            InitializeComponent();
        }

        private void DonateForm_Load(object sender, EventArgs e)
        {
            this.richTextBox1.Text = @"很高兴深蓝词库转换这个小工具能够帮助你，同时也感谢您的支持。
您可以通过小额捐赠的方式支持这款小工具的开发工作。
通过支付宝捐赠
支付宝收款账户：曾毅 studyzy@163.com
也可通过扫码捐赠:";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
