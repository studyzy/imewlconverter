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
            this.rbtnSougouFormat.Location = new System.Drawing.Point(21, 12);
            this.rbtnSougouFormat.Name = "rbtnSougouFormat";
            this.rbtnSougouFormat.Size = new System.Drawing.Size(167, 16);
            this.rbtnSougouFormat.TabIndex = 0;
            this.rbtnSougouFormat.Text = "搜狗：编码,排序位置=短语";
            this.toolTip1.SetToolTip(this.rbtnSougouFormat, "搜狗输入法自定义短语格式");
            this.rbtnSougouFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnQQFormat
            // 
            this.rbtnQQFormat.AutoSize = true;
            this.rbtnQQFormat.Location = new System.Drawing.Point(21, 45);
            this.rbtnQQFormat.Name = "rbtnQQFormat";
            this.rbtnQQFormat.Size = new System.Drawing.Size(155, 16);
            this.rbtnQQFormat.TabIndex = 0;
            this.rbtnQQFormat.TabStop = true;
            this.rbtnQQFormat.Text = "QQ：编码=排序位置,短语";
            this.toolTip1.SetToolTip(this.rbtnQQFormat, "QQ输入法自定义短语格式");
            this.rbtnQQFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnBaiduFormat
            // 
            this.rbtnBaiduFormat.AutoSize = true;
            this.rbtnBaiduFormat.Location = new System.Drawing.Point(21, 79);
            this.rbtnBaiduFormat.Name = "rbtnBaiduFormat";
            this.rbtnBaiduFormat.Size = new System.Drawing.Size(167, 16);
            this.rbtnBaiduFormat.TabIndex = 5;
            this.rbtnBaiduFormat.TabStop = true;
            this.rbtnBaiduFormat.Text = "百度：排序位置,编码=短语";
            this.toolTip1.SetToolTip(this.rbtnBaiduFormat, "QQ输入法自定义短语格式");
            this.rbtnBaiduFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnUserFormat
            // 
            this.rbtnUserFormat.AutoSize = true;
            this.rbtnUserFormat.Location = new System.Drawing.Point(21, 114);
            this.rbtnUserFormat.Name = "rbtnUserFormat";
            this.rbtnUserFormat.Size = new System.Drawing.Size(89, 16);
            this.rbtnUserFormat.TabIndex = 2;
            this.rbtnUserFormat.TabStop = true;
            this.rbtnUserFormat.Text = "自定义格式:";
            this.rbtnUserFormat.UseVisualStyleBackColor = true;
            // 
            // txbUserFormat
            // 
            this.txbUserFormat.Location = new System.Drawing.Point(116, 113);
            this.txbUserFormat.Name = "txbUserFormat";
            this.txbUserFormat.Size = new System.Drawing.Size(140, 21);
            this.txbUserFormat.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(181, 213);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 171);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "默认编码：";
            // 
            // cbxCodeType
            // 
            this.cbxCodeType.FormattingEnabled = true;
            this.cbxCodeType.Items.AddRange(new object[] {
            "用户自定义短语",
            "拼音",
            "五笔"});
            this.cbxCodeType.Location = new System.Drawing.Point(116, 168);
            this.cbxCodeType.Name = "cbxCodeType";
            this.cbxCodeType.Size = new System.Drawing.Size(140, 20);
            this.cbxCodeType.TabIndex = 8;
            // 
            // PhraseFormatConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.cbxCodeType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbtnBaiduFormat);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txbUserFormat);
            this.Controls.Add(this.rbtnUserFormat);
            this.Controls.Add(this.rbtnQQFormat);
            this.Controls.Add(this.rbtnSougouFormat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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