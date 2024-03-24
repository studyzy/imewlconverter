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
using System.Collections.Generic;
using System.Text;

namespace Studyzy.IMEWLConverter.Forms
{
    class SelfDefiningConfigFormExport : SelfDefiningConfigForm
    {
        public SelfDefiningConfigFormExport()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // SelfDefiningConfigFormExport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.ClientSize = new System.Drawing.Size(551, 491);
            this.Name = "SelfDefiningConfigFormExport";
            this.Load += new System.EventHandler(this.SelfDefiningConfigFormExport_Load);
            this.ResumeLayout(false);
        }

        private void SelfDefiningConfigFormExport_Load(object sender, EventArgs e)
        {
            this.Text = "导出词库自定义设置";
            rtbFrom.Text = "深\r\n深蓝\r\n深蓝词\r\n深蓝词库\r\n深蓝词库转\r\n深蓝词库转换";
        }
    }
}
