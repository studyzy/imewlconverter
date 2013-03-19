namespace Studyzy.IMEWLConverter
{
    partial class MergeWLForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txbMainWLFile = new System.Windows.Forms.TextBox();
            this.btnSelectMainWLFile = new System.Windows.Forms.Button();
            this.btnSelectUserWLFile = new System.Windows.Forms.Button();
            this.txbUserWLFiles = new System.Windows.Forms.TextBox();
            this.btnMergeWL = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cbxSortByCode = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "主词库：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "附加词库：";
            // 
            // txbMainWLFile
            // 
            this.txbMainWLFile.Location = new System.Drawing.Point(84, 12);
            this.txbMainWLFile.Name = "txbMainWLFile";
            this.txbMainWLFile.Size = new System.Drawing.Size(388, 21);
            this.txbMainWLFile.TabIndex = 2;
            // 
            // btnSelectMainWLFile
            // 
            this.btnSelectMainWLFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectMainWLFile.Location = new System.Drawing.Point(478, 10);
            this.btnSelectMainWLFile.Name = "btnSelectMainWLFile";
            this.btnSelectMainWLFile.Size = new System.Drawing.Size(40, 23);
            this.btnSelectMainWLFile.TabIndex = 3;
            this.btnSelectMainWLFile.Text = "...";
            this.btnSelectMainWLFile.UseVisualStyleBackColor = true;
            this.btnSelectMainWLFile.Click += new System.EventHandler(this.btnSelectMainWLFile_Click);
            // 
            // btnSelectUserWLFile
            // 
            this.btnSelectUserWLFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectUserWLFile.Location = new System.Drawing.Point(478, 47);
            this.btnSelectUserWLFile.Name = "btnSelectUserWLFile";
            this.btnSelectUserWLFile.Size = new System.Drawing.Size(40, 23);
            this.btnSelectUserWLFile.TabIndex = 5;
            this.btnSelectUserWLFile.Text = "...";
            this.btnSelectUserWLFile.UseVisualStyleBackColor = true;
            this.btnSelectUserWLFile.Click += new System.EventHandler(this.btnSelectUserWLFile_Click);
            // 
            // txbUserWLFiles
            // 
            this.txbUserWLFiles.Location = new System.Drawing.Point(83, 49);
            this.txbUserWLFiles.Name = "txbUserWLFiles";
            this.txbUserWLFiles.Size = new System.Drawing.Size(389, 21);
            this.txbUserWLFiles.TabIndex = 4;
            // 
            // btnMergeWL
            // 
            this.btnMergeWL.Location = new System.Drawing.Point(524, 12);
            this.btnMergeWL.Name = "btnMergeWL";
            this.btnMergeWL.Size = new System.Drawing.Size(56, 58);
            this.btnMergeWL.TabIndex = 6;
            this.btnMergeWL.Text = "合 并";
            this.btnMergeWL.UseVisualStyleBackColor = true;
            this.btnMergeWL.Click += new System.EventHandler(this.btnMergeWL_Click);
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.Multiselect = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 116);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(568, 403);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // cbxSortByCode
            // 
            this.cbxSortByCode.AutoSize = true;
            this.cbxSortByCode.Location = new System.Drawing.Point(14, 85);
            this.cbxSortByCode.Name = "cbxSortByCode";
            this.cbxSortByCode.Size = new System.Drawing.Size(144, 16);
            this.cbxSortByCode.TabIndex = 8;
            this.cbxSortByCode.Text = "合并后按编码重新排序";
            this.cbxSortByCode.UseVisualStyleBackColor = true;
            // 
            // MergeWLForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 531);
            this.Controls.Add(this.cbxSortByCode);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnMergeWL);
            this.Controls.Add(this.btnSelectUserWLFile);
            this.Controls.Add(this.txbUserWLFiles);
            this.Controls.Add(this.btnSelectMainWLFile);
            this.Controls.Add(this.txbMainWLFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "MergeWLForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "词库合并";
            this.Load += new System.EventHandler(this.MergeWLForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txbMainWLFile;
        private System.Windows.Forms.Button btnSelectMainWLFile;
        private System.Windows.Forms.Button btnSelectUserWLFile;
        private System.Windows.Forms.TextBox txbUserWLFiles;
        private System.Windows.Forms.Button btnMergeWL;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox cbxSortByCode;
    }
}