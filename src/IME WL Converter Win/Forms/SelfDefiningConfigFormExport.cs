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
