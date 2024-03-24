/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
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
            this.richTextBox1.Text =
                @"很高兴深蓝词库转换这个小工具能够帮助你，同时也感谢您的支持。
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
