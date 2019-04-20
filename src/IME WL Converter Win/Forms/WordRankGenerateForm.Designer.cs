namespace Studyzy.IMEWLConverter
{
    partial class WordRankGenerateForm
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
            this.rbtnGoogle = new System.Windows.Forms.RadioButton();
            this.rbtnBaidu = new System.Windows.Forms.RadioButton();
            this.rbtnCalc = new System.Windows.Forms.RadioButton();
            this.rbtnDefault = new System.Windows.Forms.RadioButton();
            this.numRank = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.cbxForceUseNewRank = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numRank)).BeginInit();
            this.SuspendLayout();
            // 
            // rbtnGoogle
            // 
            this.rbtnGoogle.AutoSize = true;
            this.rbtnGoogle.Location = new System.Drawing.Point(35, 29);
            this.rbtnGoogle.Name = "rbtnGoogle";
            this.rbtnGoogle.Size = new System.Drawing.Size(191, 16);
            this.rbtnGoogle.TabIndex = 0;
            this.rbtnGoogle.TabStop = true;
            this.rbtnGoogle.Text = "Google搜索(访问网络，速度慢)";
            this.rbtnGoogle.UseVisualStyleBackColor = true;
            // 
            // rbtnBaidu
            // 
            this.rbtnBaidu.AutoSize = true;
            this.rbtnBaidu.Location = new System.Drawing.Point(35, 76);
            this.rbtnBaidu.Name = "rbtnBaidu";
            this.rbtnBaidu.Size = new System.Drawing.Size(179, 16);
            this.rbtnBaidu.TabIndex = 1;
            this.rbtnBaidu.TabStop = true;
            this.rbtnBaidu.Text = "百度搜索(访问网络，速度慢)";
            this.rbtnBaidu.UseVisualStyleBackColor = true;
            // 
            // rbtnCalc
            // 
            this.rbtnCalc.AutoSize = true;
            this.rbtnCalc.Location = new System.Drawing.Point(35, 121);
            this.rbtnCalc.Name = "rbtnCalc";
            this.rbtnCalc.Size = new System.Drawing.Size(131, 16);
            this.rbtnCalc.TabIndex = 2;
            this.rbtnCalc.TabStop = true;
            this.rbtnCalc.Text = "按字频计算(速度快)";
            this.rbtnCalc.UseVisualStyleBackColor = true;
            // 
            // rbtnDefault
            // 
            this.rbtnDefault.AutoSize = true;
            this.rbtnDefault.Location = new System.Drawing.Point(35, 168);
            this.rbtnDefault.Name = "rbtnDefault";
            this.rbtnDefault.Size = new System.Drawing.Size(71, 16);
            this.rbtnDefault.TabIndex = 3;
            this.rbtnDefault.TabStop = true;
            this.rbtnDefault.Text = "默认值：";
            this.rbtnDefault.UseVisualStyleBackColor = true;
            // 
            // numRank
            // 
            this.numRank.Location = new System.Drawing.Point(112, 168);
            this.numRank.Name = "numRank";
            this.numRank.Size = new System.Drawing.Size(66, 21);
            this.numRank.TabIndex = 4;
            this.numRank.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(188, 213);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确 定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // cbxForceUseNewRank
            // 
            this.cbxForceUseNewRank.AutoSize = true;
            this.cbxForceUseNewRank.Location = new System.Drawing.Point(35, 217);
            this.cbxForceUseNewRank.Name = "cbxForceUseNewRank";
            this.cbxForceUseNewRank.Size = new System.Drawing.Size(108, 16);
            this.cbxForceUseNewRank.TabIndex = 6;
            this.cbxForceUseNewRank.Text = "强制使用新词频";
            this.cbxForceUseNewRank.UseVisualStyleBackColor = true;
            // 
            // WordRankGenerateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.cbxForceUseNewRank);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numRank);
            this.Controls.Add(this.rbtnDefault);
            this.Controls.Add(this.rbtnCalc);
            this.Controls.Add(this.rbtnBaidu);
            this.Controls.Add(this.rbtnGoogle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WordRankGenerateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WordRankGenerateForm";
            this.Load += new System.EventHandler(this.WordRankGenerateForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numRank)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbtnGoogle;
        private System.Windows.Forms.RadioButton rbtnBaidu;
        private System.Windows.Forms.RadioButton rbtnCalc;
        private System.Windows.Forms.RadioButton rbtnDefault;
        private System.Windows.Forms.NumericUpDown numRank;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox cbxForceUseNewRank;
    }
}