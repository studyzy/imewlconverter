namespace Studyzy.IMEWLConverter
{
    partial class ChineseConverterSelectForm
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
            this.rbtnKernel = new System.Windows.Forms.RadioButton();
            this.rbtnOffice = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbtnTransToChs = new System.Windows.Forms.RadioButton();
            this.rbtnTransToCht = new System.Windows.Forms.RadioButton();
            this.rbtnNotTrans = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // rbtnKernel
            // 
            this.rbtnKernel.AutoSize = true;
            this.rbtnKernel.Checked = true;
            this.rbtnKernel.Location = new System.Drawing.Point(18, 26);
            this.rbtnKernel.Name = "rbtnKernel";
            this.rbtnKernel.Size = new System.Drawing.Size(71, 16);
            this.rbtnKernel.TabIndex = 0;
            this.rbtnKernel.TabStop = true;
            this.rbtnKernel.Text = "系统默认";
            this.rbtnKernel.UseVisualStyleBackColor = true;
            // 
            // rbtnOffice
            // 
            this.rbtnOffice.AutoSize = true;
            this.rbtnOffice.Location = new System.Drawing.Point(18, 115);
            this.rbtnOffice.Name = "rbtnOffice";
            this.rbtnOffice.Size = new System.Drawing.Size(83, 16);
            this.rbtnOffice.TabIndex = 1;
            this.rbtnOffice.Text = "Office组件";
            this.rbtnOffice.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(38, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 60);
            this.label1.TabIndex = 2;
            this.label1.Text = "调用操作系统的简繁体转换接口，不需要依赖第三方组件，转换速度快，但转换效果较差，对“发财”和“理发”中的\"发\"字都转化成了“發”";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(203, 269);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "确 定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(38, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(238, 57);
            this.label2.TabIndex = 4;
            this.label2.Text = "调用Office的简繁体转换接口，需要安装了Office才能使用，转换速度慢，但转换效果好，对“发财”和“理发”中的\"发\"字转化成了“發”和“髮”";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.rbtnKernel);
            this.groupBox1.Controls.Add(this.rbtnOffice);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(7, 59);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(281, 194);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "转换组件选择";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbtnTransToChs);
            this.groupBox2.Controls.Add(this.rbtnTransToCht);
            this.groupBox2.Controls.Add(this.rbtnNotTrans);
            this.groupBox2.Location = new System.Drawing.Point(7, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(281, 45);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "简繁体转换";
            // 
            // rbtnTransToChs
            // 
            this.rbtnTransToChs.AutoSize = true;
            this.rbtnTransToChs.Location = new System.Drawing.Point(196, 20);
            this.rbtnTransToChs.Name = "rbtnTransToChs";
            this.rbtnTransToChs.Size = new System.Drawing.Size(71, 16);
            this.rbtnTransToChs.TabIndex = 0;
            this.rbtnTransToChs.TabStop = true;
            this.rbtnTransToChs.Text = "转为简体";
            this.rbtnTransToChs.UseVisualStyleBackColor = true;
            // 
            // rbtnTransToCht
            // 
            this.rbtnTransToCht.AutoSize = true;
            this.rbtnTransToCht.Location = new System.Drawing.Point(104, 20);
            this.rbtnTransToCht.Name = "rbtnTransToCht";
            this.rbtnTransToCht.Size = new System.Drawing.Size(71, 16);
            this.rbtnTransToCht.TabIndex = 0;
            this.rbtnTransToCht.TabStop = true;
            this.rbtnTransToCht.Text = "转为繁体";
            this.rbtnTransToCht.UseVisualStyleBackColor = true;
            // 
            // rbtnNotTrans
            // 
            this.rbtnNotTrans.AutoSize = true;
            this.rbtnNotTrans.Checked = true;
            this.rbtnNotTrans.Location = new System.Drawing.Point(22, 20);
            this.rbtnNotTrans.Name = "rbtnNotTrans";
            this.rbtnNotTrans.Size = new System.Drawing.Size(59, 16);
            this.rbtnNotTrans.TabIndex = 0;
            this.rbtnNotTrans.TabStop = true;
            this.rbtnNotTrans.Text = "不转换";
            this.rbtnNotTrans.UseVisualStyleBackColor = true;
            // 
            // ChineseConverterSelectForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 301);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChineseConverterSelectForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "简繁体转换设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rbtnKernel;
        private System.Windows.Forms.RadioButton rbtnOffice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbtnTransToChs;
        private System.Windows.Forms.RadioButton rbtnTransToCht;
        private System.Windows.Forms.RadioButton rbtnNotTrans;
    }
}