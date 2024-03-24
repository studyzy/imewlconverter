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
    class SelfDefiningConfigFormImport : SelfDefiningConfigForm
    {
        public SelfDefiningConfigFormImport()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // SelfDefiningConfigFormImport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.ClientSize = new System.Drawing.Size(551, 491);
            this.Name = "SelfDefiningConfigFormImport";
            this.Load += new System.EventHandler(this.SelfDefiningConfigFormImport_Load);
            this.ResumeLayout(false);
        }

        private void SelfDefiningConfigFormImport_Load(object sender, EventArgs e)
        {
            Text = "导入词库自定义设置";
            rtbFrom.Text =
                @"shen,lan,ci,ku,zhuan,huan 深 1234
shen,lan,ci,ku,zhuan,huan 深蓝 1234
shen,lan,ci,ku,zhuan,huan 深蓝词 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库转 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库转换 1234";
        }
    }
}
