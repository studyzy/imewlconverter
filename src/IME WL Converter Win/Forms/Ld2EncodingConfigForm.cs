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
