namespace Studyzy.IMEWLConverter
{
    partial class RimeConfigForm
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
            this.cbxCodeType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.rbWin = new System.Windows.Forms.RadioButton();
            this.rbMac = new System.Windows.Forms.RadioButton();
            this.rbLinux = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // cbxCodeType
            // 
            this.cbxCodeType.FormattingEnabled = true;
            this.cbxCodeType.Items.AddRange(new object[] {
            "拼音",
            "五笔",
            "注音",
            "仓颉",
            "其他"});
            this.cbxCodeType.Location = new System.Drawing.Point(190, 24);
            this.cbxCodeType.Name = "cbxCodeType";
            this.cbxCodeType.Size = new System.Drawing.Size(82, 21);
            this.cbxCodeType.TabIndex = 0;
            this.cbxCodeType.Text = "拼音";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "请选择Rime输入法的编码类型：";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(99, 80);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // rbWin
            // 
            this.rbWin.AutoSize = true;
            this.rbWin.Checked = true;
            this.rbWin.Location = new System.Drawing.Point(15, 57);
            this.rbWin.Name = "rbWin";
            this.rbWin.Size = new System.Drawing.Size(69, 17);
            this.rbWin.TabIndex = 3;
            this.rbWin.TabStop = true;
            this.rbWin.Text = "Windows";
            this.rbWin.UseVisualStyleBackColor = true;
            // 
            // rbMac
            // 
            this.rbMac.AutoSize = true;
            this.rbMac.Location = new System.Drawing.Point(110, 57);
            this.rbMac.Name = "rbMac";
            this.rbMac.Size = new System.Drawing.Size(61, 17);
            this.rbMac.TabIndex = 3;
            this.rbMac.Text = "MacOS";
            this.rbMac.UseVisualStyleBackColor = true;
            // 
            // rbLinux
            // 
            this.rbLinux.AutoSize = true;
            this.rbLinux.Location = new System.Drawing.Point(190, 57);
            this.rbLinux.Name = "rbLinux";
            this.rbLinux.Size = new System.Drawing.Size(50, 17);
            this.rbLinux.TabIndex = 3;
            this.rbLinux.Text = "Linux";
            this.rbLinux.UseVisualStyleBackColor = true;
            // 
            // RimeConfigForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 117);
            this.Controls.Add(this.rbLinux);
            this.Controls.Add(this.rbMac);
            this.Controls.Add(this.rbWin);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxCodeType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RimeConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rime输入法编码设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxCodeType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.RadioButton rbWin;
        private System.Windows.Forms.RadioButton rbMac;
        private System.Windows.Forms.RadioButton rbLinux;
    }
}