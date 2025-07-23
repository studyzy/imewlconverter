/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
            btnOK = new System.Windows.Forms.Button();
            numWordLengthFrom = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            numWordLengthTo = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            numWordRankFrom = new System.Windows.Forms.NumericUpDown();
            numWordRankTo = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            cbxFilterEnglish = new System.Windows.Forms.CheckBox();
            cbxFilterSpace = new System.Windows.Forms.CheckBox();
            cbxFilterPunctuation = new System.Windows.Forms.CheckBox();
            cbxNoFilter = new System.Windows.Forms.CheckBox();
            cbxReplacePunctuation = new System.Windows.Forms.CheckBox();
            cbxReplaceSpace = new System.Windows.Forms.CheckBox();
            cbxReplaceEnglish = new System.Windows.Forms.CheckBox();
            cbxReplaceNumber = new System.Windows.Forms.CheckBox();
            cbxFilterNumber = new System.Windows.Forms.CheckBox();
            label5 = new System.Windows.Forms.Label();
            numWordRankPercentage = new System.Windows.Forms.NumericUpDown();
            label6 = new System.Windows.Forms.Label();
            cbxFilterNoAlphabetCode = new System.Windows.Forms.CheckBox();
            cbxKeepNumber = new System.Windows.Forms.CheckBox();
            cbxKeepEnglish = new System.Windows.Forms.CheckBox();
            cbxFilterFirstCJK = new System.Windows.Forms.CheckBox();
            cbxKeepNumber_ = new System.Windows.Forms.CheckBox();
            cbxKeepEnglish_ = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            cbxFullWidth = new System.Windows.Forms.CheckBox();
            cbxKeepSpace = new System.Windows.Forms.CheckBox();
            cbxKeepSpace_ = new System.Windows.Forms.CheckBox();
            cbxChsNumber = new System.Windows.Forms.CheckBox();
            cbxPrefixEnglish = new System.Windows.Forms.CheckBox();
            cbxKeepPunctuation_ = new System.Windows.Forms.CheckBox();
            cbxKeepPunctuation = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)numWordLengthFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWordLengthTo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWordRankFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWordRankTo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWordRankPercentage).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(695, 33);
            btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(88, 33);
            btnOK.TabIndex = 0;
            btnOK.Text = "确定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // numWordLengthFrom
            // 
            numWordLengthFrom.Location = new System.Drawing.Point(113, 29);
            numWordLengthFrom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            numWordLengthFrom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numWordLengthFrom.Name = "numWordLengthFrom";
            numWordLengthFrom.Size = new System.Drawing.Size(76, 23);
            numWordLengthFrom.TabIndex = 1;
            numWordLengthFrom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 33);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(84, 17);
            label1.TabIndex = 2;
            label1.Text = "保留字数： 从";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(14, 84);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(84, 17);
            label2.TabIndex = 3;
            label2.Text = "保留词频： 从";
            // 
            // numWordLengthTo
            // 
            numWordLengthTo.Location = new System.Drawing.Point(232, 29);
            numWordLengthTo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            numWordLengthTo.Name = "numWordLengthTo";
            numWordLengthTo.Size = new System.Drawing.Size(76, 23);
            numWordLengthTo.TabIndex = 1;
            numWordLengthTo.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(196, 33);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(20, 17);
            label3.TabIndex = 4;
            label3.Text = "到";
            // 
            // numWordRankFrom
            // 
            numWordRankFrom.Location = new System.Drawing.Point(113, 80);
            numWordRankFrom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            numWordRankFrom.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numWordRankFrom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numWordRankFrom.Name = "numWordRankFrom";
            numWordRankFrom.Size = new System.Drawing.Size(76, 23);
            numWordRankFrom.TabIndex = 1;
            numWordRankFrom.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // numWordRankTo
            // 
            numWordRankTo.Location = new System.Drawing.Point(232, 80);
            numWordRankTo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            numWordRankTo.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numWordRankTo.Minimum = new decimal(new int[] { 9, 0, 0, 0 });
            numWordRankTo.Name = "numWordRankTo";
            numWordRankTo.Size = new System.Drawing.Size(76, 23);
            numWordRankTo.TabIndex = 1;
            numWordRankTo.Value = new decimal(new int[] { 999999, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(196, 84);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(20, 17);
            label4.TabIndex = 4;
            label4.Text = "到";
            // 
            // cbxFilterEnglish
            // 
            cbxFilterEnglish.AutoSize = true;
            cbxFilterEnglish.Location = new System.Drawing.Point(19, 228);
            cbxFilterEnglish.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFilterEnglish.Name = "cbxFilterEnglish";
            cbxFilterEnglish.Size = new System.Drawing.Size(123, 21);
            cbxFilterEnglish.TabIndex = 5;
            cbxFilterEnglish.Text = "过滤包含英文的词";
            cbxFilterEnglish.UseVisualStyleBackColor = true;
            // 
            // cbxFilterSpace
            // 
            cbxFilterSpace.AutoSize = true;
            cbxFilterSpace.Location = new System.Drawing.Point(19, 329);
            cbxFilterSpace.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFilterSpace.Name = "cbxFilterSpace";
            cbxFilterSpace.Size = new System.Drawing.Size(123, 21);
            cbxFilterSpace.TabIndex = 6;
            cbxFilterSpace.Text = "过滤包含空格的词";
            cbxFilterSpace.UseVisualStyleBackColor = true;
            // 
            // cbxFilterPunctuation
            // 
            cbxFilterPunctuation.AutoSize = true;
            cbxFilterPunctuation.Location = new System.Drawing.Point(19, 381);
            cbxFilterPunctuation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFilterPunctuation.Name = "cbxFilterPunctuation";
            cbxFilterPunctuation.Size = new System.Drawing.Size(123, 21);
            cbxFilterPunctuation.TabIndex = 7;
            cbxFilterPunctuation.Text = "过滤包含标点的词";
            cbxFilterPunctuation.UseVisualStyleBackColor = true;
            // 
            // cbxNoFilter
            // 
            cbxNoFilter.AutoSize = true;
            cbxNoFilter.Location = new System.Drawing.Point(19, 176);
            cbxNoFilter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxNoFilter.Name = "cbxNoFilter";
            cbxNoFilter.Size = new System.Drawing.Size(63, 21);
            cbxNoFilter.TabIndex = 8;
            cbxNoFilter.Text = "不过滤";
            cbxNoFilter.UseVisualStyleBackColor = true;
            // 
            // cbxReplacePunctuation
            // 
            cbxReplacePunctuation.AutoSize = true;
            cbxReplacePunctuation.Location = new System.Drawing.Point(199, 381);
            cbxReplacePunctuation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxReplacePunctuation.Name = "cbxReplacePunctuation";
            cbxReplacePunctuation.Size = new System.Drawing.Size(99, 21);
            cbxReplacePunctuation.TabIndex = 11;
            cbxReplacePunctuation.Text = "替换标点部分";
            cbxReplacePunctuation.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceSpace
            // 
            cbxReplaceSpace.AutoSize = true;
            cbxReplaceSpace.Location = new System.Drawing.Point(199, 329);
            cbxReplaceSpace.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxReplaceSpace.Name = "cbxReplaceSpace";
            cbxReplaceSpace.Size = new System.Drawing.Size(99, 21);
            cbxReplaceSpace.TabIndex = 10;
            cbxReplaceSpace.Text = "替换空格部分";
            cbxReplaceSpace.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceEnglish
            // 
            cbxReplaceEnglish.AutoSize = true;
            cbxReplaceEnglish.Location = new System.Drawing.Point(199, 228);
            cbxReplaceEnglish.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxReplaceEnglish.Name = "cbxReplaceEnglish";
            cbxReplaceEnglish.Size = new System.Drawing.Size(99, 21);
            cbxReplaceEnglish.TabIndex = 9;
            cbxReplaceEnglish.Text = "替换英文部分";
            cbxReplaceEnglish.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceNumber
            // 
            cbxReplaceNumber.AutoSize = true;
            cbxReplaceNumber.Location = new System.Drawing.Point(199, 279);
            cbxReplaceNumber.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxReplaceNumber.Name = "cbxReplaceNumber";
            cbxReplaceNumber.Size = new System.Drawing.Size(99, 21);
            cbxReplaceNumber.TabIndex = 13;
            cbxReplaceNumber.Text = "替换数字部分";
            cbxReplaceNumber.UseVisualStyleBackColor = true;
            // 
            // cbxFilterNumber
            // 
            cbxFilterNumber.AutoSize = true;
            cbxFilterNumber.Location = new System.Drawing.Point(19, 279);
            cbxFilterNumber.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFilterNumber.Name = "cbxFilterNumber";
            cbxFilterNumber.Size = new System.Drawing.Size(123, 21);
            cbxFilterNumber.TabIndex = 12;
            cbxFilterNumber.Text = "过滤包含数字的词";
            cbxFilterNumber.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(14, 129);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(80, 17);
            label5.TabIndex = 14;
            label5.Text = "保留高词频：";
            // 
            // numWordRankPercentage
            // 
            numWordRankPercentage.Location = new System.Drawing.Point(112, 124);
            numWordRankPercentage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            numWordRankPercentage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numWordRankPercentage.Name = "numWordRankPercentage";
            numWordRankPercentage.Size = new System.Drawing.Size(76, 23);
            numWordRankPercentage.TabIndex = 15;
            numWordRankPercentage.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(195, 126);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(19, 17);
            label6.TabIndex = 16;
            label6.Text = "%";
            // 
            // cbxFilterNoAlphabetCode
            // 
            cbxFilterNoAlphabetCode.AutoSize = true;
            cbxFilterNoAlphabetCode.Location = new System.Drawing.Point(19, 435);
            cbxFilterNoAlphabetCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFilterNoAlphabetCode.Name = "cbxFilterNoAlphabetCode";
            cbxFilterNoAlphabetCode.Size = new System.Drawing.Size(135, 21);
            cbxFilterNoAlphabetCode.TabIndex = 17;
            cbxFilterNoAlphabetCode.Text = "过滤非字母编码的词";
            cbxFilterNoAlphabetCode.UseVisualStyleBackColor = true;
            // 
            // cbxKeepNumber
            // 
            cbxKeepNumber.AutoSize = true;
            cbxKeepNumber.Location = new System.Drawing.Point(206, 103);
            cbxKeepNumber.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepNumber.Name = "cbxKeepNumber";
            cbxKeepNumber.Size = new System.Drawing.Size(159, 21);
            cbxKeepNumber.TabIndex = 19;
            cbxKeepNumber.Text = "词条中的数字直接当编码";
            cbxKeepNumber.UseVisualStyleBackColor = true;
            cbxKeepNumber.CheckedChanged += cbxKeepNumber_CheckedChanged;
            // 
            // cbxKeepEnglish
            // 
            cbxKeepEnglish.AutoSize = true;
            cbxKeepEnglish.Checked = true;
            cbxKeepEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxKeepEnglish.Location = new System.Drawing.Point(206, 51);
            cbxKeepEnglish.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepEnglish.Name = "cbxKeepEnglish";
            cbxKeepEnglish.Size = new System.Drawing.Size(159, 21);
            cbxKeepEnglish.TabIndex = 18;
            cbxKeepEnglish.Text = "词条中的字母直接当编码";
            cbxKeepEnglish.UseVisualStyleBackColor = true;
            cbxKeepEnglish.CheckedChanged += cbxKeepEnglish_CheckedChanged;
            // 
            // cbxFilterFirstCJK
            // 
            cbxFilterFirstCJK.AutoSize = true;
            cbxFilterFirstCJK.Checked = true;
            cbxFilterFirstCJK.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxFilterFirstCJK.Location = new System.Drawing.Point(19, 488);
            cbxFilterFirstCJK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFilterFirstCJK.Name = "cbxFilterFirstCJK";
            cbxFilterFirstCJK.Size = new System.Drawing.Size(219, 21);
            cbxFilterFirstCJK.TabIndex = 20;
            cbxFilterFirstCJK.Text = "过滤首字非中日韩统一表义字符的词";
            cbxFilterFirstCJK.UseVisualStyleBackColor = true;
            // 
            // cbxKeepNumber_
            // 
            cbxKeepNumber_.AutoSize = true;
            cbxKeepNumber_.Checked = true;
            cbxKeepNumber_.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxKeepNumber_.Location = new System.Drawing.Point(12, 103);
            cbxKeepNumber_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepNumber_.Name = "cbxKeepNumber_";
            cbxKeepNumber_.Size = new System.Drawing.Size(135, 21);
            cbxKeepNumber_.TabIndex = 22;
            cbxKeepNumber_.Text = "词条中的数字不编码";
            cbxKeepNumber_.UseVisualStyleBackColor = true;
            cbxKeepNumber_.CheckedChanged += cbxKeepNumber__CheckedChanged;
            // 
            // cbxKeepEnglish_
            // 
            cbxKeepEnglish_.AutoSize = true;
            cbxKeepEnglish_.Location = new System.Drawing.Point(12, 51);
            cbxKeepEnglish_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepEnglish_.Name = "cbxKeepEnglish_";
            cbxKeepEnglish_.Size = new System.Drawing.Size(135, 21);
            cbxKeepEnglish_.TabIndex = 21;
            cbxKeepEnglish_.Text = "词条中的字母不编码";
            cbxKeepEnglish_.UseVisualStyleBackColor = true;
            cbxKeepEnglish_.CheckedChanged += cbxKeepEnglish__CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cbxFullWidth);
            groupBox1.Controls.Add(cbxKeepSpace);
            groupBox1.Controls.Add(cbxKeepSpace_);
            groupBox1.Controls.Add(cbxChsNumber);
            groupBox1.Controls.Add(cbxPrefixEnglish);
            groupBox1.Controls.Add(cbxKeepPunctuation_);
            groupBox1.Controls.Add(cbxKeepPunctuation);
            groupBox1.Controls.Add(cbxKeepNumber_);
            groupBox1.Controls.Add(cbxKeepEnglish_);
            groupBox1.Controls.Add(cbxKeepNumber);
            groupBox1.Controls.Add(cbxKeepEnglish);
            groupBox1.Location = new System.Drawing.Point(370, 176);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(412, 360);
            groupBox1.TabIndex = 23;
            groupBox1.TabStop = false;
            groupBox1.Text = "词条分段处理";
            // 
            // cbxFullWidth
            // 
            cbxFullWidth.AutoSize = true;
            cbxFullWidth.Checked = true;
            cbxFullWidth.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxFullWidth.Location = new System.Drawing.Point(12, 313);
            cbxFullWidth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxFullWidth.Name = "cbxFullWidth";
            cbxFullWidth.Size = new System.Drawing.Size(267, 21);
            cbxFullWidth.TabIndex = 29;
            cbxFullWidth.Text = "全角英语数字和基本符号先转半角再进行编码";
            cbxFullWidth.UseVisualStyleBackColor = true;
            // 
            // cbxKeepSpace
            // 
            cbxKeepSpace.AutoSize = true;
            cbxKeepSpace.Location = new System.Drawing.Point(206, 153);
            cbxKeepSpace.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepSpace.Name = "cbxKeepSpace";
            cbxKeepSpace.Size = new System.Drawing.Size(159, 21);
            cbxKeepSpace.TabIndex = 28;
            cbxKeepSpace.Text = "编码时保留字母后的空格";
            cbxKeepSpace.UseVisualStyleBackColor = true;
            // 
            // cbxKeepSpace_
            // 
            cbxKeepSpace_.AutoSize = true;
            cbxKeepSpace_.Location = new System.Drawing.Point(12, 153);
            cbxKeepSpace_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepSpace_.Name = "cbxKeepSpace_";
            cbxKeepSpace_.Size = new System.Drawing.Size(135, 21);
            cbxKeepSpace_.TabIndex = 27;
            cbxKeepSpace_.Text = "词条中的空格不编码";
            cbxKeepSpace_.UseVisualStyleBackColor = true;
            // 
            // cbxChsNumber
            // 
            cbxChsNumber.AutoSize = true;
            cbxChsNumber.Location = new System.Drawing.Point(206, 260);
            cbxChsNumber.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxChsNumber.Name = "cbxChsNumber";
            cbxChsNumber.Size = new System.Drawing.Size(159, 21);
            cbxChsNumber.TabIndex = 26;
            cbxChsNumber.Text = "数字先转汉字再进行编码";
            cbxChsNumber.UseVisualStyleBackColor = true;
            cbxChsNumber.CheckedChanged += cbxChsNumber_CheckedChanged;
            // 
            // cbxPrefixEnglish
            // 
            cbxPrefixEnglish.AutoSize = true;
            cbxPrefixEnglish.Checked = true;
            cbxPrefixEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxPrefixEnglish.Location = new System.Drawing.Point(12, 260);
            cbxPrefixEnglish.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxPrefixEnglish.Name = "cbxPrefixEnglish";
            cbxPrefixEnglish.Size = new System.Drawing.Size(174, 21);
            cbxPrefixEnglish.TabIndex = 25;
            cbxPrefixEnglish.Text = "中英文的编码中间用\"_\"分隔";
            cbxPrefixEnglish.UseVisualStyleBackColor = true;
            cbxPrefixEnglish.CheckedChanged += cbxPrefixEnglish_CheckedChanged;
            // 
            // cbxKeepPunctuation_
            // 
            cbxKeepPunctuation_.AutoSize = true;
            cbxKeepPunctuation_.Checked = true;
            cbxKeepPunctuation_.CheckState = System.Windows.Forms.CheckState.Checked;
            cbxKeepPunctuation_.Location = new System.Drawing.Point(12, 205);
            cbxKeepPunctuation_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepPunctuation_.Name = "cbxKeepPunctuation_";
            cbxKeepPunctuation_.Size = new System.Drawing.Size(135, 21);
            cbxKeepPunctuation_.TabIndex = 24;
            cbxKeepPunctuation_.Text = "词条中的标点不编码";
            cbxKeepPunctuation_.UseVisualStyleBackColor = true;
            cbxKeepPunctuation_.CheckedChanged += cbxKeepPunctuation__CheckedChanged;
            // 
            // cbxKeepPunctuation
            // 
            cbxKeepPunctuation.AutoSize = true;
            cbxKeepPunctuation.Location = new System.Drawing.Point(206, 205);
            cbxKeepPunctuation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxKeepPunctuation.Name = "cbxKeepPunctuation";
            cbxKeepPunctuation.Size = new System.Drawing.Size(159, 21);
            cbxKeepPunctuation.TabIndex = 23;
            cbxKeepPunctuation.Text = "词条中的标点直接当编码";
            cbxKeepPunctuation.UseVisualStyleBackColor = true;
            cbxKeepPunctuation.CheckedChanged += cbxKeepPunctuator_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Location = new System.Drawing.Point(184, 176);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(175, 250);
            groupBox2.TabIndex = 24;
            groupBox2.TabStop = false;
            groupBox2.Text = "词条字符替换";
            // 
            // FilterConfigForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(795, 550);
            Controls.Add(groupBox1);
            Controls.Add(cbxFilterFirstCJK);
            Controls.Add(cbxFilterNoAlphabetCode);
            Controls.Add(label6);
            Controls.Add(numWordRankPercentage);
            Controls.Add(label5);
            Controls.Add(cbxReplaceNumber);
            Controls.Add(cbxFilterNumber);
            Controls.Add(cbxReplacePunctuation);
            Controls.Add(cbxReplaceSpace);
            Controls.Add(cbxReplaceEnglish);
            Controls.Add(cbxNoFilter);
            Controls.Add(cbxFilterPunctuation);
            Controls.Add(cbxFilterSpace);
            Controls.Add(cbxFilterEnglish);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(numWordRankTo);
            Controls.Add(numWordRankFrom);
            Controls.Add(numWordLengthTo);
            Controls.Add(numWordLengthFrom);
            Controls.Add(btnOK);
            Controls.Add(groupBox2);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FilterConfigForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "词条过滤设置";
            Load += FilterConfigForm_Load;
            ((System.ComponentModel.ISupportInitialize)numWordLengthFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWordLengthTo).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWordRankFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWordRankTo).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWordRankPercentage).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.CheckBox cbxFilterNoAlphabetCode;
        private System.Windows.Forms.CheckBox cbxKeepNumber;
        private System.Windows.Forms.CheckBox cbxKeepEnglish;
        private System.Windows.Forms.CheckBox cbxFilterFirstCJK;
        private System.Windows.Forms.CheckBox cbxKeepNumber_;
        private System.Windows.Forms.CheckBox cbxKeepEnglish_;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbxKeepPunctuation;
        private System.Windows.Forms.CheckBox cbxPrefixEnglish;
        private System.Windows.Forms.CheckBox cbxKeepPunctuation_;
        private System.Windows.Forms.CheckBox cbxChsNumber;
        private System.Windows.Forms.CheckBox cbxKeepSpace_;
        private System.Windows.Forms.CheckBox cbxKeepSpace;
        private System.Windows.Forms.CheckBox cbxFullWidth;
    }
}