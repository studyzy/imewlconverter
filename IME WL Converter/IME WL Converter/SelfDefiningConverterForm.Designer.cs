namespace Studyzy.IMEWLConverter
{
    partial class SelfDefiningConverterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelfDefiningConverterForm));
            this.rtbFrom = new System.Windows.Forms.RichTextBox();
            this.txbParsePattern = new System.Windows.Forms.TextBox();
            this.btnParse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rtbTo = new System.Windows.Forms.RichTextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnHelpBuild = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txbFilePath = new System.Windows.Forms.TextBox();
            this.btnFileSelect = new System.Windows.Forms.Button();
            this.btnConvertTest = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxTextEncoding = new Studyzy.IMEWLConverter.EncodingComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbFrom
            // 
            this.rtbFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbFrom.Location = new System.Drawing.Point(3, 17);
            this.rtbFrom.Name = "rtbFrom";
            this.rtbFrom.Size = new System.Drawing.Size(253, 314);
            this.rtbFrom.TabIndex = 0;
            this.rtbFrom.Text = "";
            this.rtbFrom.WordWrap = false;
            // 
            // txbParsePattern
            // 
            this.txbParsePattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbParsePattern.Location = new System.Drawing.Point(73, 13);
            this.txbParsePattern.Name = "txbParsePattern";
            this.txbParsePattern.ReadOnly = true;
            this.txbParsePattern.Size = new System.Drawing.Size(423, 21);
            this.txbParsePattern.TabIndex = 1;
            this.txbParsePattern.Text = "点击右侧按钮选择规则-->";
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(48, 456);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(75, 23);
            this.btnParse.TabIndex = 2;
            this.btnParse.Text = "测试识别";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "匹配规则:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rtbFrom);
            this.groupBox1.Location = new System.Drawing.Point(10, 104);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 334);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "源内容";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rtbTo);
            this.groupBox2.Location = new System.Drawing.Point(275, 104);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 334);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "测试结果";
            // 
            // rtbTo
            // 
            this.rtbTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbTo.Location = new System.Drawing.Point(3, 17);
            this.rtbTo.Name = "rtbTo";
            this.rtbTo.Size = new System.Drawing.Size(260, 314);
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
            // btnHelpBuild
            // 
            this.btnHelpBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelpBuild.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnHelpBuild.Location = new System.Drawing.Point(502, 11);
            this.btnHelpBuild.Name = "btnHelpBuild";
            this.btnHelpBuild.Size = new System.Drawing.Size(36, 23);
            this.btnHelpBuild.TabIndex = 7;
            this.btnHelpBuild.Text = "...";
            this.btnHelpBuild.UseVisualStyleBackColor = true;
            this.btnHelpBuild.Click += new System.EventHandler(this.btnHelpBuild_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "编码文件:";
            // 
            // txbFilePath
            // 
            this.txbFilePath.Location = new System.Drawing.Point(73, 40);
            this.txbFilePath.Name = "txbFilePath";
            this.txbFilePath.Size = new System.Drawing.Size(423, 21);
            this.txbFilePath.TabIndex = 9;
            // 
            // btnFileSelect
            // 
            this.btnFileSelect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFileSelect.Location = new System.Drawing.Point(502, 39);
            this.btnFileSelect.Name = "btnFileSelect";
            this.btnFileSelect.Size = new System.Drawing.Size(36, 23);
            this.btnFileSelect.TabIndex = 8;
            this.btnFileSelect.Text = "..";
            this.btnFileSelect.UseVisualStyleBackColor = true;
            this.btnFileSelect.Click += new System.EventHandler(this.btnFileSelect_Click);
            // 
            // btnConvertTest
            // 
            this.btnConvertTest.Location = new System.Drawing.Point(158, 456);
            this.btnConvertTest.Name = "btnConvertTest";
            this.btnConvertTest.Size = new System.Drawing.Size(75, 23);
            this.btnConvertTest.TabIndex = 11;
            this.btnConvertTest.Text = "测试编码";
            this.btnConvertTest.UseVisualStyleBackColor = true;
            this.btnConvertTest.Visible = false;
            this.btnConvertTest.Click += new System.EventHandler(this.btnConvertTest_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(419, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "编码文件中每行一个汉字和编码，汉字不可重复，格式形如：“深<Tab>shen”";
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
            "BigEndianUnicode",
            "ASCII"});
            this.cbxTextEncoding.Location = new System.Drawing.Point(433, 69);
            this.cbxTextEncoding.Name = "cbxTextEncoding";
            this.cbxTextEncoding.SelectedEncoding = ((System.Text.Encoding)(resources.GetObject("cbxTextEncoding.SelectedEncoding")));
            this.cbxTextEncoding.Size = new System.Drawing.Size(105, 20);
            this.cbxTextEncoding.TabIndex = 14;
            this.cbxTextEncoding.Visible = false;
            // 
            // SelfDefiningConverterForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 491);
            this.Controls.Add(this.cbxTextEncoding);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnConvertTest);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txbFilePath);
            this.Controls.Add(this.btnFileSelect);
            this.Controls.Add(this.btnHelpBuild);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.txbParsePattern);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelfDefiningConverterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自定义词库编码";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelfDefiningConverterForm_FormClosing);
            this.Load += new System.EventHandler(this.SelfDefiningConverterForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbFrom;
        private System.Windows.Forms.TextBox txbParsePattern;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox rtbTo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnHelpBuild;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txbFilePath;
        private System.Windows.Forms.Button btnFileSelect;
        private System.Windows.Forms.Button btnConvertTest;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label3;
        private EncodingComboBox cbxTextEncoding;
    }
}