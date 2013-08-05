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
            rtbFrom.Text = @"shen,lan,ci,ku,zhuan,huan 深 1234
shen,lan,ci,ku,zhuan,huan 深蓝 1234
shen,lan,ci,ku,zhuan,huan 深蓝词 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库转 1234
shen,lan,ci,ku,zhuan,huan 深蓝词库转换 1234";
        }
    }
}
