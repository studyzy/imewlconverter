namespace Studyzy.IMEWLConverter
{
    partial class SelfDefiningConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelfDefiningConfigForm));
            this.rtbFrom = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rtbTo = new System.Windows.Forms.RichTextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbFileSelect = new System.Windows.Forms.Label();
            this.txbFilePath = new System.Windows.Forms.TextBox();
            this.btnFileSelect = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.lbRemark = new System.Windows.Forms.Label();
            this.cbxIsPinyin = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxCodeFormat = new System.Windows.Forms.ComboBox();
            this.rtbCodeFormat = new System.Windows.Forms.RichTextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbxPinyinSplitBehind = new System.Windows.Forms.CheckBox();
            this.cbxPinyinSplitBefore = new System.Windows.Forms.CheckBox();
            this.cbbxSplitString = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.numOrderCipin = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numOrderHanzi = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numOrderPinyin = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxIncludeCipin = new System.Windows.Forms.CheckBox();
            this.cbbxPinyinSplitString = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbxIncludePinyin = new System.Windows.Forms.CheckBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.cbxTextEncoding = new Studyzy.IMEWLConverter.EncodingComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderCipin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderHanzi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderPinyin)).BeginInit();
            this.SuspendLayout();
            // 
            // rtbFrom
            // 
            this.rtbFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbFrom.Location = new System.Drawing.Point(3, 17);
            this.rtbFrom.Name = "rtbFrom";
            this.rtbFrom.Size = new System.Drawing.Size(210, 107);
            this.rtbFrom.TabIndex = 0;
            this.rtbFrom.Text = "";
            this.rtbFrom.WordWrap = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rtbFrom);
            this.groupBox1.Location = new System.Drawing.Point(10, 311);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(216, 127);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "源内容";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rtbTo);
            this.groupBox2.Location = new System.Drawing.Point(275, 311);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 127);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "结果";
            // 
            // rtbTo
            // 
            this.rtbTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbTo.Location = new System.Drawing.Point(3, 17);
            this.rtbTo.Name = "rtbTo";
            this.rtbTo.ReadOnly = true;
            this.rtbTo.Size = new System.Drawing.Size(260, 107);
            this.rtbTo.TabIndex = 0;
            this.rtbTo.Text = "";
            this.rtbTo.WordWrap = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(377, 456);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(463, 456);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbFileSelect
            // 
            this.lbFileSelect.AutoSize = true;
            this.lbFileSelect.Location = new System.Drawing.Point(88, 25);
            this.lbFileSelect.Name = "lbFileSelect";
            this.lbFileSelect.Size = new System.Drawing.Size(59, 12);
            this.lbFileSelect.TabIndex = 10;
            this.lbFileSelect.Text = "编码文件:";
            this.lbFileSelect.Visible = false;
            // 
            // txbFilePath
            // 
            this.txbFilePath.Location = new System.Drawing.Point(153, 20);
            this.txbFilePath.Name = "txbFilePath";
            this.txbFilePath.Size = new System.Drawing.Size(330, 21);
            this.txbFilePath.TabIndex = 9;
            this.txbFilePath.Visible = false;
            // 
            // btnFileSelect
            // 
            this.btnFileSelect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFileSelect.Location = new System.Drawing.Point(486, 20);
            this.btnFileSelect.Name = "btnFileSelect";
            this.btnFileSelect.Size = new System.Drawing.Size(36, 23);
            this.btnFileSelect.TabIndex = 8;
            this.btnFileSelect.Text = "..";
            this.btnFileSelect.UseVisualStyleBackColor = true;
            this.btnFileSelect.Visible = false;
            this.btnFileSelect.Click += new System.EventHandler(this.btnFileSelect_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // lbRemark
            // 
            this.lbRemark.AutoSize = true;
            this.lbRemark.Location = new System.Drawing.Point(4, 55);
            this.lbRemark.Name = "lbRemark";
            this.lbRemark.Size = new System.Drawing.Size(503, 12);
            this.lbRemark.TabIndex = 12;
            this.lbRemark.Text = "若不使用拼音，编码文件中每行一个汉字和编码，汉字不可重复，格式形如：“深<Tab>shen”";
            // 
            // cbxIsPinyin
            // 
            this.cbxIsPinyin.AutoSize = true;
            this.cbxIsPinyin.Checked = true;
            this.cbxIsPinyin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIsPinyin.Location = new System.Drawing.Point(6, 25);
            this.cbxIsPinyin.Name = "cbxIsPinyin";
            this.cbxIsPinyin.Size = new System.Drawing.Size(72, 16);
            this.cbxIsPinyin.TabIndex = 15;
            this.cbxIsPinyin.Text = "拼音编码";
            this.toolTip1.SetToolTip(this.cbxIsPinyin, "选中则使用拼音编码规则对词库进行编码，不选中则使用外部指定的编码文件进行编码");
            this.cbxIsPinyin.UseVisualStyleBackColor = true;
            this.cbxIsPinyin.CheckedChanged += new System.EventHandler(this.cbxIsPinyin_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.cbxCodeFormat);
            this.groupBox3.Controls.Add(this.rtbCodeFormat);
            this.groupBox3.Controls.Add(this.lbFileSelect);
            this.groupBox3.Controls.Add(this.btnFileSelect);
            this.groupBox3.Controls.Add(this.cbxTextEncoding);
            this.groupBox3.Controls.Add(this.cbxIsPinyin);
            this.groupBox3.Controls.Add(this.lbRemark);
            this.groupBox3.Controls.Add(this.txbFilePath);
            this.groupBox3.Location = new System.Drawing.Point(10, 155);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(528, 150);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "编码设置";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(358, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 18;
            this.label3.Text = "词库编码：";
            // 
            // cbxCodeFormat
            // 
            this.cbxCodeFormat.FormattingEnabled = true;
            this.cbxCodeFormat.Items.AddRange(new object[] {
            "拼音规则",
            "五笔规则",
            "自定义规则"});
            this.cbxCodeFormat.Location = new System.Drawing.Point(8, 100);
            this.cbxCodeFormat.Name = "cbxCodeFormat";
            this.cbxCodeFormat.Size = new System.Drawing.Size(118, 20);
            this.cbxCodeFormat.TabIndex = 17;
            this.cbxCodeFormat.Text = "拼音规则";
            this.cbxCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cbxCodeFormat_SelectedIndexChanged);
            // 
            // rtbCodeFormat
            // 
            this.rtbCodeFormat.Location = new System.Drawing.Point(153, 79);
            this.rtbCodeFormat.Name = "rtbCodeFormat";
            this.rtbCodeFormat.Size = new System.Drawing.Size(153, 60);
            this.rtbCodeFormat.TabIndex = 16;
            this.rtbCodeFormat.Text = "code_e2=p11+p12+p21+p22\ncode_e3=p11+p21+p31+p32\ncode_a4=p11+p21+p31+n11";
            this.rtbCodeFormat.Visible = false;
            this.rtbCodeFormat.TextChanged += new System.EventHandler(this.rtbCodeFormat_TextChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbxPinyinSplitBehind);
            this.groupBox4.Controls.Add(this.cbxPinyinSplitBefore);
            this.groupBox4.Controls.Add(this.cbbxSplitString);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.groupBox5);
            this.groupBox4.Controls.Add(this.cbxIncludeCipin);
            this.groupBox4.Controls.Add(this.cbbxPinyinSplitString);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.cbxIncludePinyin);
            this.groupBox4.Location = new System.Drawing.Point(10, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(528, 146);
            this.groupBox4.TabIndex = 17;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "格式设置";
            // 
            // cbxPinyinSplitBehind
            // 
            this.cbxPinyinSplitBehind.AutoSize = true;
            this.cbxPinyinSplitBehind.Location = new System.Drawing.Point(257, 93);
            this.cbxPinyinSplitBehind.Name = "cbxPinyinSplitBehind";
            this.cbxPinyinSplitBehind.Size = new System.Drawing.Size(108, 16);
            this.cbxPinyinSplitBehind.TabIndex = 19;
            this.cbxPinyinSplitBehind.Text = "编码后带分隔符";
            this.cbxPinyinSplitBehind.UseVisualStyleBackColor = true;
            this.cbxPinyinSplitBehind.CheckedChanged += new System.EventHandler(this.cbxPinyinSplit_CheckedChanged);
            // 
            // cbxPinyinSplitBefore
            // 
            this.cbxPinyinSplitBefore.AutoSize = true;
            this.cbxPinyinSplitBefore.Location = new System.Drawing.Point(257, 59);
            this.cbxPinyinSplitBefore.Name = "cbxPinyinSplitBefore";
            this.cbxPinyinSplitBefore.Size = new System.Drawing.Size(108, 16);
            this.cbxPinyinSplitBefore.TabIndex = 20;
            this.cbxPinyinSplitBefore.Text = "编码前带分隔符";
            this.cbxPinyinSplitBefore.UseVisualStyleBackColor = true;
            this.cbxPinyinSplitBefore.CheckedChanged += new System.EventHandler(this.cbxPinyinSplit_CheckedChanged);
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
            this.cbbxSplitString.Location = new System.Drawing.Point(416, 19);
            this.cbbxSplitString.Name = "cbbxSplitString";
            this.cbbxSplitString.Size = new System.Drawing.Size(53, 20);
            this.cbbxSplitString.TabIndex = 18;
            this.cbbxSplitString.Text = "空格";
            this.cbbxSplitString.SelectedIndexChanged += new System.EventHandler(this.cbbxSplitString_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(255, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(155, 12);
            this.label5.TabIndex = 17;
            this.label5.Text = "编码汉字词频之间的分隔符:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.numOrderCipin);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.numOrderHanzi);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.numOrderPinyin);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Location = new System.Drawing.Point(84, 42);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(118, 100);
            this.groupBox5.TabIndex = 16;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "词条排序";
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "汉字:";
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "编码:";
            // 
            // cbxIncludeCipin
            // 
            this.cbxIncludeCipin.AutoSize = true;
            this.cbxIncludeCipin.Checked = true;
            this.cbxIncludeCipin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIncludeCipin.Location = new System.Drawing.Point(6, 117);
            this.cbxIncludeCipin.Name = "cbxIncludeCipin";
            this.cbxIncludeCipin.Size = new System.Drawing.Size(72, 16);
            this.cbxIncludeCipin.TabIndex = 15;
            this.cbxIncludeCipin.Text = "包含词频";
            this.cbxIncludeCipin.UseVisualStyleBackColor = true;
            this.cbxIncludeCipin.CheckedChanged += new System.EventHandler(this.cbxIncludeCipin_CheckedChanged);
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
            this.cbbxPinyinSplitString.Location = new System.Drawing.Point(158, 16);
            this.cbbxPinyinSplitString.Name = "cbbxPinyinSplitString";
            this.cbbxPinyinSplitString.Size = new System.Drawing.Size(44, 20);
            this.cbbxPinyinSplitString.TabIndex = 14;
            this.cbbxPinyinSplitString.Text = ",";
            this.cbbxPinyinSplitString.SelectedIndexChanged += new System.EventHandler(this.cbbxPinyinSplitString_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(131, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "每个编码之间的分隔符:";
            // 
            // cbxIncludePinyin
            // 
            this.cbxIncludePinyin.AutoSize = true;
            this.cbxIncludePinyin.Checked = true;
            this.cbxIncludePinyin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIncludePinyin.Location = new System.Drawing.Point(6, 64);
            this.cbxIncludePinyin.Name = "cbxIncludePinyin";
            this.cbxIncludePinyin.Size = new System.Drawing.Size(72, 16);
            this.cbxIncludePinyin.TabIndex = 12;
            this.cbxIncludePinyin.Text = "包含编码";
            this.cbxIncludePinyin.UseVisualStyleBackColor = true;
            this.cbxIncludePinyin.CheckedChanged += new System.EventHandler(this.cbxIncludePinyin_CheckedChanged);
            // 
            // btnTest
            // 
            this.btnTest.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTest.Location = new System.Drawing.Point(229, 368);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(40, 23);
            this.btnTest.TabIndex = 1;
            this.btnTest.Text = "->";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // cbxTextEncoding
            // 
            this.cbxTextEncoding.FormattingEnabled = true;
            this.cbxTextEncoding.Items.AddRange(new object[] {
            "Unicode",
            "UTF-8",
            "GB18030",
            "GBK",
            "Big5",
            "UnicodeFFFE",
            "ASCII",
            "Unicode",
            "UTF-8",
            "GB18030",
            "GBK",
            "Big5",
            "UnicodeFFFE",
            "ASCII",
            "Unicode",
            "UTF-8",
            "GB18030",
            "GBK",
            "Big5",
            "UnicodeFFFE",
            "ASCII"});
            this.cbxTextEncoding.Location = new System.Drawing.Point(429, 100);
            this.cbxTextEncoding.Name = "cbxTextEncoding";
            this.cbxTextEncoding.SelectedEncoding = ((System.Text.Encoding)(resources.GetObject("cbxTextEncoding.SelectedEncoding")));
            this.cbxTextEncoding.Size = new System.Drawing.Size(93, 20);
            this.cbxTextEncoding.TabIndex = 14;
            this.cbxTextEncoding.Text = "UTF-8";
            // 
            // SelfDefiningConfigForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 491);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelfDefiningConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自定义词库编码";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelfDefiningConverterForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderCipin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderHanzi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOrderPinyin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.RichTextBox rtbFrom;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.RichTextBox rtbTo;
        protected System.Windows.Forms.Button btnOK;
        protected System.Windows.Forms.Button btnCancel;
        protected System.Windows.Forms.Label lbFileSelect;
        protected System.Windows.Forms.TextBox txbFilePath;
        protected System.Windows.Forms.Button btnFileSelect;
        protected System.Windows.Forms.OpenFileDialog openFileDialog1;
        protected System.Windows.Forms.Label lbRemark;
        protected EncodingComboBox cbxTextEncoding;
        protected System.Windows.Forms.CheckBox cbxIsPinyin;
        protected System.Windows.Forms.ToolTip toolTip1;
        protected System.Windows.Forms.GroupBox groupBox3;
        protected System.Windows.Forms.GroupBox groupBox4;
        protected System.Windows.Forms.ComboBox cbxCodeFormat;
        protected System.Windows.Forms.RichTextBox rtbCodeFormat;
        protected System.Windows.Forms.CheckBox cbxPinyinSplitBehind;
        protected System.Windows.Forms.CheckBox cbxPinyinSplitBefore;
        protected System.Windows.Forms.ComboBox cbbxSplitString;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.GroupBox groupBox5;
        protected System.Windows.Forms.NumericUpDown numOrderCipin;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.NumericUpDown numOrderHanzi;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.NumericUpDown numOrderPinyin;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.CheckBox cbxIncludeCipin;
        protected System.Windows.Forms.ComboBox cbbxPinyinSplitString;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.CheckBox cbxIncludePinyin;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.Button btnTest;
    }
}