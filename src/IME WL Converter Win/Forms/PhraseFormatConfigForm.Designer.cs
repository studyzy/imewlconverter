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

namespace Studyzy.IMEWLConverter
{
    partial class PhraseFormatConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.rbtnSougouFormat = new System.Windows.Forms.RadioButton();
            this.rbtnQQFormat = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.rbtnBaiduFormat = new System.Windows.Forms.RadioButton();
            this.rbtnUserFormat = new System.Windows.Forms.RadioButton();
            this.txbUserFormat = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbxCodeType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // rbtnSougouFormat
            // 
            this.rbtnSougouFormat.AutoSize = true;
            this.rbtnSougouFormat.Location = new System.Drawing.Point(28, 15);
            this.rbtnSougouFormat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbtnSougouFormat.Name = "rbtnSougouFormat";
            this.rbtnSougouFormat.Size = new System.Drawing.Size(209, 19);
            this.rbtnSougouFormat.TabIndex = 0;
            this.rbtnSougouFormat.Text = "搜狗：编码,排序位置=短语";
            this.toolTip1.SetToolTip(this.rbtnSougouFormat, "搜狗输入法自定义短语格式");
            this.rbtnSougouFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnQQFormat
            // 
            this.rbtnQQFormat.AutoSize = true;
            this.rbtnQQFormat.Location = new System.Drawing.Point(28, 56);
            this.rbtnQQFormat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbtnQQFormat.Name = "rbtnQQFormat";
            this.rbtnQQFormat.Size = new System.Drawing.Size(195, 19);
            this.rbtnQQFormat.TabIndex = 0;
            this.rbtnQQFormat.TabStop = true;
            this.rbtnQQFormat.Text = "QQ：编码=排序位置,短语";
            this.toolTip1.SetToolTip(this.rbtnQQFormat, "QQ输入法自定义短语格式");
            this.rbtnQQFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnBaiduFormat
            // 
            this.rbtnBaiduFormat.AutoSize = true;
            this.rbtnBaiduFormat.Location = new System.Drawing.Point(28, 99);
            this.rbtnBaiduFormat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbtnBaiduFormat.Name = "rbtnBaiduFormat";
            this.rbtnBaiduFormat.Size = new System.Drawing.Size(209, 19);
            this.rbtnBaiduFormat.TabIndex = 5;
            this.rbtnBaiduFormat.TabStop = true;
            this.rbtnBaiduFormat.Text = "百度：排序位置,编码=短语";
            this.toolTip1.SetToolTip(this.rbtnBaiduFormat, "QQ输入法自定义短语格式");
            this.rbtnBaiduFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnUserFormat
            // 
            this.rbtnUserFormat.AutoSize = true;
            this.rbtnUserFormat.Location = new System.Drawing.Point(28, 142);
            this.rbtnUserFormat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbtnUserFormat.Name = "rbtnUserFormat";
            this.rbtnUserFormat.Size = new System.Drawing.Size(111, 19);
            this.rbtnUserFormat.TabIndex = 2;
            this.rbtnUserFormat.TabStop = true;
            this.rbtnUserFormat.Text = "自定义格式:";
            this.rbtnUserFormat.UseVisualStyleBackColor = true;
            // 
            // txbUserFormat
            // 
            this.txbUserFormat.Location = new System.Drawing.Point(155, 141);
            this.txbUserFormat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txbUserFormat.Name = "txbUserFormat";
            this.txbUserFormat.Size = new System.Drawing.Size(185, 25);
            this.txbUserFormat.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(241, 266);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 29);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 214);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "默认编码：";
            // 
            // cbxCodeType
            // 
            this.cbxCodeType.FormattingEnabled = true;
            this.cbxCodeType.Items.AddRange(new object[] {
            "用户自定义短语",
            "拼音",
            "五笔",
            "拼音首字母"});
            this.cbxCodeType.Location = new System.Drawing.Point(155, 210);
            this.cbxCodeType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbxCodeType.Name = "cbxCodeType";
            this.cbxCodeType.Size = new System.Drawing.Size(185, 23);
            this.cbxCodeType.TabIndex = 8;
            this.cbxCodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            // 
            // PhraseFormatConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 328);
            this.Controls.Add(this.cbxCodeType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbtnBaiduFormat);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txbUserFormat);
            this.Controls.Add(this.rbtnUserFormat);
            this.Controls.Add(this.rbtnQQFormat);
            this.Controls.Add(this.rbtnSougouFormat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PhraseFormatConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自定义短语格式设置";
            this.Load += new System.EventHandler(this.PhraseFormatConfigForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbtnSougouFormat;
        private System.Windows.Forms.RadioButton rbtnQQFormat;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.RadioButton rbtnUserFormat;
        private System.Windows.Forms.TextBox txbUserFormat;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.RadioButton rbtnBaiduFormat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxCodeType;
    }
}