
namespace Studyzy.IMEWLConverter.Forms
{
    partial class ResultSortForm
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
            this.cbxSortOptions = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.cbxDesc = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbxSortOptions
            // 
            this.cbxSortOptions.FormattingEnabled = true;
            this.cbxSortOptions.Items.AddRange(new object[] {
            "默认",
            "按编码顺序",
            "按汉字顺序",
            "按词频顺序"});
            this.cbxSortOptions.Location = new System.Drawing.Point(12, 34);
            this.cbxSortOptions.Name = "cbxSortOptions";
            this.cbxSortOptions.Size = new System.Drawing.Size(137, 20);
            this.cbxSortOptions.TabIndex = 0;
            this.cbxSortOptions.Text = "默认";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(256, 34);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确 定";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // cbxDesc
            // 
            this.cbxDesc.AutoSize = true;
            this.cbxDesc.Location = new System.Drawing.Point(172, 36);
            this.cbxDesc.Name = "cbxDesc";
            this.cbxDesc.Size = new System.Drawing.Size(48, 16);
            this.cbxDesc.TabIndex = 2;
            this.cbxDesc.Text = "反序";
            this.cbxDesc.UseVisualStyleBackColor = true;
            // 
            // ResultSortForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 98);
            this.Controls.Add(this.cbxDesc);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbxSortOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ResultSortForm";
            this.ShowInTaskbar = false;
            this.Text = "结果词库排序";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxSortOptions;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox cbxDesc;
    }
}