namespace Studyzy.IMEWLConverter
{
    partial class PinyinConfigForm
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
            this.cbxPinyinType = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "编码方案：";
            // 
            // cbxPinyinType
            // 
            this.cbxPinyinType.FormattingEnabled = true;
            this.cbxPinyinType.Items.AddRange(new object[] {
            "全拼",
            "微软双拼",
            "小鹤双拼",
            "智能ABC",
            "自然码",
            "拼音加加",
            "星空键道",
            "大牛双拼",
            "小浪双拼",
            "紫光拼音",
            "用户自定义短语"});
            this.cbxPinyinType.Location = new System.Drawing.Point(133, 26);
            this.cbxPinyinType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbxPinyinType.Name = "cbxPinyinType";
            this.cbxPinyinType.Size = new System.Drawing.Size(180, 23);
            this.cbxPinyinType.TabIndex = 1;
            this.cbxPinyinType.Text = "全拼";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(304, 125);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 29);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // PinyinConfigForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 169);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbxPinyinType);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PinyinConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "拼音配置设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxPinyinType;
        private System.Windows.Forms.Button btnOK;
    }
}