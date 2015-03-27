namespace Studyzy.IMEWLConverter
{
    partial class FilterConfigForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.numWordLengthFrom = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numWordLengthTo = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numWordRankFrom = new System.Windows.Forms.NumericUpDown();
            this.numWordRankTo = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.cbxFilterEnglish = new System.Windows.Forms.CheckBox();
            this.cbxFilterSpace = new System.Windows.Forms.CheckBox();
            this.cbxFilterPunctuation = new System.Windows.Forms.CheckBox();
            this.cbxNoFilter = new System.Windows.Forms.CheckBox();
            this.cbxReplacePunctuation = new System.Windows.Forms.CheckBox();
            this.cbxReplaceSpace = new System.Windows.Forms.CheckBox();
            this.cbxReplaceEnglish = new System.Windows.Forms.CheckBox();
            this.cbxReplaceNumber = new System.Windows.Forms.CheckBox();
            this.cbxFilterNumber = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numWordRankPercentage = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numWordLengthFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordLengthTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankPercentage)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(189, 287);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // numWordLengthFrom
            // 
            this.numWordLengthFrom.Location = new System.Drawing.Point(97, 23);
            this.numWordLengthFrom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWordLengthFrom.Name = "numWordLengthFrom";
            this.numWordLengthFrom.Size = new System.Drawing.Size(65, 20);
            this.numWordLengthFrom.TabIndex = 1;
            this.numWordLengthFrom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "保留字数： 从";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "保留词频： 从";
            // 
            // numWordLengthTo
            // 
            this.numWordLengthTo.Location = new System.Drawing.Point(199, 23);
            this.numWordLengthTo.Name = "numWordLengthTo";
            this.numWordLengthTo.Size = new System.Drawing.Size(65, 20);
            this.numWordLengthTo.TabIndex = 1;
            this.numWordLengthTo.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(168, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "到";
            // 
            // numWordRankFrom
            // 
            this.numWordRankFrom.Location = new System.Drawing.Point(97, 62);
            this.numWordRankFrom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWordRankFrom.Name = "numWordRankFrom";
            this.numWordRankFrom.Size = new System.Drawing.Size(65, 20);
            this.numWordRankFrom.TabIndex = 1;
            this.numWordRankFrom.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // numWordRankTo
            // 
            this.numWordRankTo.Location = new System.Drawing.Point(199, 62);
            this.numWordRankTo.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numWordRankTo.Name = "numWordRankTo";
            this.numWordRankTo.Size = new System.Drawing.Size(65, 20);
            this.numWordRankTo.TabIndex = 1;
            this.numWordRankTo.Value = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(168, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "到";
            // 
            // cbxFilterEnglish
            // 
            this.cbxFilterEnglish.AutoSize = true;
            this.cbxFilterEnglish.Checked = true;
            this.cbxFilterEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxFilterEnglish.Location = new System.Drawing.Point(14, 134);
            this.cbxFilterEnglish.Name = "cbxFilterEnglish";
            this.cbxFilterEnglish.Size = new System.Drawing.Size(122, 17);
            this.cbxFilterEnglish.TabIndex = 5;
            this.cbxFilterEnglish.Text = "过滤包含英文的词";
            this.cbxFilterEnglish.UseVisualStyleBackColor = true;
            // 
            // cbxFilterSpace
            // 
            this.cbxFilterSpace.AutoSize = true;
            this.cbxFilterSpace.Checked = true;
            this.cbxFilterSpace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxFilterSpace.Location = new System.Drawing.Point(14, 211);
            this.cbxFilterSpace.Name = "cbxFilterSpace";
            this.cbxFilterSpace.Size = new System.Drawing.Size(122, 17);
            this.cbxFilterSpace.TabIndex = 6;
            this.cbxFilterSpace.Text = "过滤包含空格的词";
            this.cbxFilterSpace.UseVisualStyleBackColor = true;
            // 
            // cbxFilterPunctuation
            // 
            this.cbxFilterPunctuation.AutoSize = true;
            this.cbxFilterPunctuation.Checked = true;
            this.cbxFilterPunctuation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxFilterPunctuation.Location = new System.Drawing.Point(14, 251);
            this.cbxFilterPunctuation.Name = "cbxFilterPunctuation";
            this.cbxFilterPunctuation.Size = new System.Drawing.Size(122, 17);
            this.cbxFilterPunctuation.TabIndex = 7;
            this.cbxFilterPunctuation.Text = "过滤包含标点的词";
            this.cbxFilterPunctuation.UseVisualStyleBackColor = true;
            // 
            // cbxNoFilter
            // 
            this.cbxNoFilter.AutoSize = true;
            this.cbxNoFilter.Location = new System.Drawing.Point(14, 291);
            this.cbxNoFilter.Name = "cbxNoFilter";
            this.cbxNoFilter.Size = new System.Drawing.Size(62, 17);
            this.cbxNoFilter.TabIndex = 8;
            this.cbxNoFilter.Text = "不过滤";
            this.cbxNoFilter.UseVisualStyleBackColor = true;
            // 
            // cbxReplacePunctuation
            // 
            this.cbxReplacePunctuation.AutoSize = true;
            this.cbxReplacePunctuation.Location = new System.Drawing.Point(170, 251);
            this.cbxReplacePunctuation.Name = "cbxReplacePunctuation";
            this.cbxReplacePunctuation.Size = new System.Drawing.Size(98, 17);
            this.cbxReplacePunctuation.TabIndex = 11;
            this.cbxReplacePunctuation.Text = "替换标点部分";
            this.cbxReplacePunctuation.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceSpace
            // 
            this.cbxReplaceSpace.AutoSize = true;
            this.cbxReplaceSpace.Location = new System.Drawing.Point(170, 211);
            this.cbxReplaceSpace.Name = "cbxReplaceSpace";
            this.cbxReplaceSpace.Size = new System.Drawing.Size(98, 17);
            this.cbxReplaceSpace.TabIndex = 10;
            this.cbxReplaceSpace.Text = "替换空格部分";
            this.cbxReplaceSpace.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceEnglish
            // 
            this.cbxReplaceEnglish.AutoSize = true;
            this.cbxReplaceEnglish.Location = new System.Drawing.Point(170, 134);
            this.cbxReplaceEnglish.Name = "cbxReplaceEnglish";
            this.cbxReplaceEnglish.Size = new System.Drawing.Size(98, 17);
            this.cbxReplaceEnglish.TabIndex = 9;
            this.cbxReplaceEnglish.Text = "替换英文部分";
            this.cbxReplaceEnglish.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceNumber
            // 
            this.cbxReplaceNumber.AutoSize = true;
            this.cbxReplaceNumber.Location = new System.Drawing.Point(170, 173);
            this.cbxReplaceNumber.Name = "cbxReplaceNumber";
            this.cbxReplaceNumber.Size = new System.Drawing.Size(98, 17);
            this.cbxReplaceNumber.TabIndex = 13;
            this.cbxReplaceNumber.Text = "替换数字部分";
            this.cbxReplaceNumber.UseVisualStyleBackColor = true;
            // 
            // cbxFilterNumber
            // 
            this.cbxFilterNumber.AutoSize = true;
            this.cbxFilterNumber.Checked = true;
            this.cbxFilterNumber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxFilterNumber.Location = new System.Drawing.Point(14, 173);
            this.cbxFilterNumber.Name = "cbxFilterNumber";
            this.cbxFilterNumber.Size = new System.Drawing.Size(122, 17);
            this.cbxFilterNumber.TabIndex = 12;
            this.cbxFilterNumber.Text = "过滤包含数字的词";
            this.cbxFilterNumber.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "保留高词频：";
            // 
            // numWordRankPercentage
            // 
            this.numWordRankPercentage.Location = new System.Drawing.Point(96, 94);
            this.numWordRankPercentage.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWordRankPercentage.Name = "numWordRankPercentage";
            this.numWordRankPercentage.Size = new System.Drawing.Size(65, 20);
            this.numWordRankPercentage.TabIndex = 15;
            this.numWordRankPercentage.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(167, 96);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "%";
            // 
            // FilterConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 323);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numWordRankPercentage);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbxReplaceNumber);
            this.Controls.Add(this.cbxFilterNumber);
            this.Controls.Add(this.cbxReplacePunctuation);
            this.Controls.Add(this.cbxReplaceSpace);
            this.Controls.Add(this.cbxReplaceEnglish);
            this.Controls.Add(this.cbxNoFilter);
            this.Controls.Add(this.cbxFilterPunctuation);
            this.Controls.Add(this.cbxFilterSpace);
            this.Controls.Add(this.cbxFilterEnglish);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numWordRankTo);
            this.Controls.Add(this.numWordRankFrom);
            this.Controls.Add(this.numWordLengthTo);
            this.Controls.Add(this.numWordLengthFrom);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "词条过滤设置";
            this.Load += new System.EventHandler(this.FilterConfigForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numWordLengthFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordLengthTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankPercentage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.NumericUpDown numWordLengthFrom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numWordLengthTo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numWordRankFrom;
        private System.Windows.Forms.NumericUpDown numWordRankTo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbxFilterEnglish;
        private System.Windows.Forms.CheckBox cbxFilterSpace;
        private System.Windows.Forms.CheckBox cbxFilterPunctuation;
        private System.Windows.Forms.CheckBox cbxNoFilter;
        private System.Windows.Forms.CheckBox cbxReplacePunctuation;
        private System.Windows.Forms.CheckBox cbxReplaceSpace;
        private System.Windows.Forms.CheckBox cbxReplaceEnglish;
        private System.Windows.Forms.CheckBox cbxReplaceNumber;
        private System.Windows.Forms.CheckBox cbxFilterNumber;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numWordRankPercentage;
        private System.Windows.Forms.Label label6;
    }
}