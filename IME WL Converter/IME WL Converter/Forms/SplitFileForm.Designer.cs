namespace Studyzy.IMEWLConverter
{
    partial class SplitFileForm
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
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.txbFilePath = new System.Windows.Forms.TextBox();
            this.btnSplit = new System.Windows.Forms.Button();
            this.rbtnSplitByLine = new System.Windows.Forms.RadioButton();
            this.rbtnSplitBySize = new System.Windows.Forms.RadioButton();
            this.rbtnSplitByLength = new System.Windows.Forms.RadioButton();
            this.numdMaxLine = new System.Windows.Forms.NumericUpDown();
            this.numdMaxSize = new System.Windows.Forms.NumericUpDown();
            this.numdMaxLength = new System.Windows.Forms.NumericUpDown();
            this.rtbLogs = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numdMaxLine)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numdMaxSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numdMaxLength)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectFile.Location = new System.Drawing.Point(320, 10);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(34, 23);
            this.btnSelectFile.TabIndex = 0;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // txbFilePath
            // 
            this.txbFilePath.Location = new System.Drawing.Point(12, 12);
            this.txbFilePath.Name = "txbFilePath";
            this.txbFilePath.Size = new System.Drawing.Size(297, 21);
            this.txbFilePath.TabIndex = 1;
            // 
            // btnSplit
            // 
            this.btnSplit.Location = new System.Drawing.Point(362, 7);
            this.btnSplit.Name = "btnSplit";
            this.btnSplit.Size = new System.Drawing.Size(66, 31);
            this.btnSplit.TabIndex = 2;
            this.btnSplit.Text = "分 割";
            this.btnSplit.UseVisualStyleBackColor = true;
            this.btnSplit.Click += new System.EventHandler(this.btnSplit_Click);
            // 
            // rbtnSplitByLine
            // 
            this.rbtnSplitByLine.AutoSize = true;
            this.rbtnSplitByLine.Checked = true;
            this.rbtnSplitByLine.Location = new System.Drawing.Point(12, 59);
            this.rbtnSplitByLine.Name = "rbtnSplitByLine";
            this.rbtnSplitByLine.Size = new System.Drawing.Size(83, 16);
            this.rbtnSplitByLine.TabIndex = 3;
            this.rbtnSplitByLine.TabStop = true;
            this.rbtnSplitByLine.Text = "按行数分割";
            this.rbtnSplitByLine.UseVisualStyleBackColor = true;
            // 
            // rbtnSplitBySize
            // 
            this.rbtnSplitBySize.AutoSize = true;
            this.rbtnSplitBySize.Location = new System.Drawing.Point(12, 97);
            this.rbtnSplitBySize.Name = "rbtnSplitBySize";
            this.rbtnSplitBySize.Size = new System.Drawing.Size(107, 16);
            this.rbtnSplitBySize.TabIndex = 4;
            this.rbtnSplitBySize.Text = "按文件大小分割";
            this.rbtnSplitBySize.UseVisualStyleBackColor = true;
            // 
            // rbtnSplitByLength
            // 
            this.rbtnSplitByLength.AutoSize = true;
            this.rbtnSplitByLength.Location = new System.Drawing.Point(13, 134);
            this.rbtnSplitByLength.Name = "rbtnSplitByLength";
            this.rbtnSplitByLength.Size = new System.Drawing.Size(83, 16);
            this.rbtnSplitByLength.TabIndex = 5;
            this.rbtnSplitByLength.Text = "按字数分割";
            this.rbtnSplitByLength.UseVisualStyleBackColor = true;
            // 
            // numdMaxLine
            // 
            this.numdMaxLine.Location = new System.Drawing.Point(148, 59);
            this.numdMaxLine.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numdMaxLine.Name = "numdMaxLine";
            this.numdMaxLine.Size = new System.Drawing.Size(91, 21);
            this.numdMaxLine.TabIndex = 6;
            this.numdMaxLine.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // numdMaxSize
            // 
            this.numdMaxSize.Location = new System.Drawing.Point(148, 97);
            this.numdMaxSize.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numdMaxSize.Name = "numdMaxSize";
            this.numdMaxSize.Size = new System.Drawing.Size(91, 21);
            this.numdMaxSize.TabIndex = 7;
            this.numdMaxSize.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // numdMaxLength
            // 
            this.numdMaxLength.Location = new System.Drawing.Point(148, 134);
            this.numdMaxLength.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numdMaxLength.Name = "numdMaxLength";
            this.numdMaxLength.Size = new System.Drawing.Size(91, 21);
            this.numdMaxLength.TabIndex = 8;
            this.numdMaxLength.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // rtbLogs
            // 
            this.rtbLogs.Location = new System.Drawing.Point(12, 161);
            this.rtbLogs.Name = "rtbLogs";
            this.rtbLogs.Size = new System.Drawing.Size(414, 207);
            this.rtbLogs.TabIndex = 9;
            this.rtbLogs.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(245, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "行";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(245, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "KB";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(245, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "字";
            // 
            // SplitFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 380);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rtbLogs);
            this.Controls.Add(this.numdMaxLength);
            this.Controls.Add(this.numdMaxSize);
            this.Controls.Add(this.numdMaxLine);
            this.Controls.Add(this.rbtnSplitByLength);
            this.Controls.Add(this.rbtnSplitBySize);
            this.Controls.Add(this.rbtnSplitByLine);
            this.Controls.Add(this.btnSplit);
            this.Controls.Add(this.txbFilePath);
            this.Controls.Add(this.btnSelectFile);
            this.MaximizeBox = false;
            this.Name = "SplitFileForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "文件分割";
            ((System.ComponentModel.ISupportInitialize)(this.numdMaxLine)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numdMaxSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numdMaxLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.TextBox txbFilePath;
        private System.Windows.Forms.Button btnSplit;
        private System.Windows.Forms.RadioButton rbtnSplitByLine;
        private System.Windows.Forms.RadioButton rbtnSplitBySize;
        private System.Windows.Forms.RadioButton rbtnSplitByLength;
        private System.Windows.Forms.NumericUpDown numdMaxLine;
        private System.Windows.Forms.NumericUpDown numdMaxSize;
        private System.Windows.Forms.NumericUpDown numdMaxLength;
        private System.Windows.Forms.RichTextBox rtbLogs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}