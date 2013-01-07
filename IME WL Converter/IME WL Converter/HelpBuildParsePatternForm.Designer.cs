namespace Studyzy.IMEWLConverter
{
    partial class HelpBuildParsePatternForm
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
            this.cbxIncludePinyin = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbbxPinyinSplitString = new System.Windows.Forms.ComboBox();
            this.cbxIncludeCipin = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numOrderPinyin = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numOrderCipin = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numOrderHanzi = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.cbbxSplitString = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txbSample = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbxPinyinSplitBefore = new System.Windows.Forms.CheckBox();
            this.cbxPinyinSplitBehind = new System.Windows.Forms.CheckBox();
            this.rtbCodeFormat = new System.Windows.Forms.RichTextBox();
            this.cbxCodeFormat = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderPinyin)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderCipin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderHanzi)).BeginInit();
            this.SuspendLayout();
            // 
            // cbxIncludePinyin
            // 
            this.cbxIncludePinyin.AutoSize = true;
            this.cbxIncludePinyin.Checked = true;
            this.cbxIncludePinyin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIncludePinyin.Location = new System.Drawing.Point(14, 14);
            this.cbxIncludePinyin.Name = "cbxIncludePinyin";
            this.cbxIncludePinyin.Size = new System.Drawing.Size(72, 16);
            this.cbxIncludePinyin.TabIndex = 0;
            this.cbxIncludePinyin.Text = "包含编码";
            this.cbxIncludePinyin.UseVisualStyleBackColor = true;
            this.cbxIncludePinyin.CheckedChanged += new System.EventHandler(this.cbxIncludePinyin_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "每个编码之间的分隔符:";
            // 
            // cbbxPinyinSplitString
            // 
            this.cbbxPinyinSplitString.FormattingEnabled = true;
            this.cbbxPinyinSplitString.Items.AddRange(new object[] {
            "空格",
            "\'",
            "|",
            ",",
            "Tab",
            "无"});
            this.cbbxPinyinSplitString.Location = new System.Drawing.Point(196, 40);
            this.cbbxPinyinSplitString.Name = "cbbxPinyinSplitString";
            this.cbbxPinyinSplitString.Size = new System.Drawing.Size(86, 20);
            this.cbbxPinyinSplitString.TabIndex = 2;
            this.cbbxPinyinSplitString.Text = ",";
            this.cbbxPinyinSplitString.SelectedIndexChanged += new System.EventHandler(this.cbbxPinyinSplitString_SelectedIndexChanged);
            // 
            // cbxIncludeCipin
            // 
            this.cbxIncludeCipin.AutoSize = true;
            this.cbxIncludeCipin.Checked = true;
            this.cbxIncludeCipin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIncludeCipin.Location = new System.Drawing.Point(196, 14);
            this.cbxIncludeCipin.Name = "cbxIncludeCipin";
            this.cbxIncludeCipin.Size = new System.Drawing.Size(72, 16);
            this.cbxIncludeCipin.TabIndex = 3;
            this.cbxIncludeCipin.Text = "包含词频";
            this.cbxIncludeCipin.UseVisualStyleBackColor = true;
            this.cbxIncludeCipin.CheckedChanged += new System.EventHandler(this.cbxIncludeCipin_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "编码:";
            // 
            // numOrderPinyin
            // 
            this.numOrderPinyin.Location = new System.Drawing.Point(49, 17);
            this.numOrderPinyin.Name = "numOrderPinyin";
            this.numOrderPinyin.Size = new System.Drawing.Size(60, 21);
            this.numOrderPinyin.TabIndex = 5;
            this.numOrderPinyin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numOrderPinyin.ValueChanged += new System.EventHandler(this.numOrderPinyin_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numOrderCipin);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numOrderHanzi);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numOrderPinyin);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(14, 95);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(118, 100);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "词条排序";
            // 
            // numOrderCipin
            // 
            this.numOrderCipin.Location = new System.Drawing.Point(49, 71);
            this.numOrderCipin.Name = "numOrderCipin";
            this.numOrderCipin.Size = new System.Drawing.Size(60, 21);
            this.numOrderCipin.TabIndex = 9;
            this.numOrderCipin.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numOrderCipin.ValueChanged += new System.EventHandler(this.numOrderPinyin_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "词频:";
            // 
            // numOrderHanzi
            // 
            this.numOrderHanzi.Location = new System.Drawing.Point(49, 44);
            this.numOrderHanzi.Name = "numOrderHanzi";
            this.numOrderHanzi.Size = new System.Drawing.Size(60, 21);
            this.numOrderHanzi.TabIndex = 7;
            this.numOrderHanzi.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numOrderHanzi.ValueChanged += new System.EventHandler(this.numOrderPinyin_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "汉字:";
            // 
            // cbbxSplitString
            // 
            this.cbbxSplitString.FormattingEnabled = true;
            this.cbbxSplitString.Items.AddRange(new object[] {
            "空格",
            "\'",
            "|",
            ",",
            "Tab"});
            this.cbbxSplitString.Location = new System.Drawing.Point(196, 66);
            this.cbbxSplitString.Name = "cbbxSplitString";
            this.cbbxSplitString.Size = new System.Drawing.Size(86, 20);
            this.cbbxSplitString.TabIndex = 8;
            this.cbbxSplitString.Text = "空格";
            this.cbbxSplitString.SelectedIndexChanged += new System.EventHandler(this.cbbxSplitString_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(155, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "编码汉字词频之间的分隔符:";
            // 
            // txbSample
            // 
            this.txbSample.Location = new System.Drawing.Point(16, 234);
            this.txbSample.Multiline = true;
            this.txbSample.Name = "txbSample";
            this.txbSample.ReadOnly = true;
            this.txbSample.Size = new System.Drawing.Size(286, 74);
            this.txbSample.TabIndex = 9;
            this.txbSample.Text = "这是例子";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(114, 314);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(205, 314);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cbxPinyinSplitBefore
            // 
            this.cbxPinyinSplitBefore.AutoSize = true;
            this.cbxPinyinSplitBefore.Location = new System.Drawing.Point(190, 112);
            this.cbxPinyinSplitBefore.Name = "cbxPinyinSplitBefore";
            this.cbxPinyinSplitBefore.Size = new System.Drawing.Size(108, 16);
            this.cbxPinyinSplitBefore.TabIndex = 11;
            this.cbxPinyinSplitBefore.Text = "编码前带分隔符";
            this.cbxPinyinSplitBefore.UseVisualStyleBackColor = true;
            this.cbxPinyinSplitBefore.CheckedChanged += new System.EventHandler(this.cbxPinyinSplitBefore_CheckedChanged);
            // 
            // cbxPinyinSplitBehind
            // 
            this.cbxPinyinSplitBehind.AutoSize = true;
            this.cbxPinyinSplitBehind.Location = new System.Drawing.Point(190, 140);
            this.cbxPinyinSplitBehind.Name = "cbxPinyinSplitBehind";
            this.cbxPinyinSplitBehind.Size = new System.Drawing.Size(108, 16);
            this.cbxPinyinSplitBehind.TabIndex = 11;
            this.cbxPinyinSplitBehind.Text = "编码后带分隔符";
            this.cbxPinyinSplitBehind.UseVisualStyleBackColor = true;
            this.cbxPinyinSplitBehind.CheckedChanged += new System.EventHandler(this.cbxPinyinSplitBefore_CheckedChanged);
            // 
            // rtbCodeFormat
            // 
            this.rtbCodeFormat.Location = new System.Drawing.Point(149, 168);
            this.rtbCodeFormat.Name = "rtbCodeFormat";
            this.rtbCodeFormat.Size = new System.Drawing.Size(153, 60);
            this.rtbCodeFormat.TabIndex = 12;
            this.rtbCodeFormat.Text = "code_e2=p11+p12+p21+p22\ncode_e3=p11+p21+p31+p32\ncode_a4=p11+p21+p31+n11";
            // 
            // cbxCodeFormat
            // 
            this.cbxCodeFormat.FormattingEnabled = true;
            this.cbxCodeFormat.Items.AddRange(new object[] {
            "拼音格式",
            "五笔格式",
            "自定义格式"});
            this.cbxCodeFormat.Location = new System.Drawing.Point(14, 201);
            this.cbxCodeFormat.Name = "cbxCodeFormat";
            this.cbxCodeFormat.Size = new System.Drawing.Size(118, 20);
            this.cbxCodeFormat.TabIndex = 13;
            this.cbxCodeFormat.Text = "拼音格式";
            this.cbxCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cbxCodeFormat_SelectedIndexChanged);
            // 
            // HelpBuildParsePatternForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 345);
            this.Controls.Add(this.cbxCodeFormat);
            this.Controls.Add(this.rtbCodeFormat);
            this.Controls.Add(this.cbxPinyinSplitBehind);
            this.Controls.Add(this.cbxPinyinSplitBefore);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txbSample);
            this.Controls.Add(this.cbbxSplitString);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbxIncludeCipin);
            this.Controls.Add(this.cbbxPinyinSplitString);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxIncludePinyin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HelpBuildParsePatternForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自定义格式配置";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.HelpBuildParsePatternForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numOrderPinyin)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderCipin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderHanzi)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbxIncludePinyin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbbxPinyinSplitString;
        private System.Windows.Forms.CheckBox cbxIncludeCipin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numOrderPinyin;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numOrderCipin;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numOrderHanzi;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbbxSplitString;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txbSample;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbxPinyinSplitBefore;
        private System.Windows.Forms.CheckBox cbxPinyinSplitBehind;
        private System.Windows.Forms.RichTextBox rtbCodeFormat;
        private System.Windows.Forms.ComboBox cbxCodeFormat;
    }
}