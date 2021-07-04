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
            this.cbxFilterNoAlphabetCode = new System.Windows.Forms.CheckBox();
            this.cbxKeepNumber = new System.Windows.Forms.CheckBox();
            this.cbxKeepEnglish = new System.Windows.Forms.CheckBox();
            this.cbxFilterFirstCJK = new System.Windows.Forms.CheckBox();
            this.cbxKeepNumber_ = new System.Windows.Forms.CheckBox();
            this.cbxKeepEnglish_ = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbxChsNumber = new System.Windows.Forms.CheckBox();
            this.cbxPrefixEnglish = new System.Windows.Forms.CheckBox();
            this.cbxKeepPunctuation_ = new System.Windows.Forms.CheckBox();
            this.cbxKeepPunctuation = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbxKeepSpace_ = new System.Windows.Forms.CheckBox();
            this.cbxKeepSpace = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numWordLengthFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordLengthTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWordRankPercentage)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(752, 443);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 29);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // numWordLengthFrom
            // 
            this.numWordLengthFrom.Location = new System.Drawing.Point(129, 26);
            this.numWordLengthFrom.Margin = new System.Windows.Forms.Padding(4);
            this.numWordLengthFrom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWordLengthFrom.Name = "numWordLengthFrom";
            this.numWordLengthFrom.Size = new System.Drawing.Size(87, 25);
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
            this.label1.Location = new System.Drawing.Point(16, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "保留字数： 从";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 74);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "保留词频： 从";
            // 
            // numWordLengthTo
            // 
            this.numWordLengthTo.Location = new System.Drawing.Point(265, 26);
            this.numWordLengthTo.Margin = new System.Windows.Forms.Padding(4);
            this.numWordLengthTo.Name = "numWordLengthTo";
            this.numWordLengthTo.Size = new System.Drawing.Size(87, 25);
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
            this.label3.Location = new System.Drawing.Point(224, 29);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "到";
            // 
            // numWordRankFrom
            // 
            this.numWordRankFrom.Location = new System.Drawing.Point(129, 71);
            this.numWordRankFrom.Margin = new System.Windows.Forms.Padding(4);
            this.numWordRankFrom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWordRankFrom.Name = "numWordRankFrom";
            this.numWordRankFrom.Size = new System.Drawing.Size(87, 25);
            this.numWordRankFrom.TabIndex = 1;
            this.numWordRankFrom.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // numWordRankTo
            // 
            this.numWordRankTo.Location = new System.Drawing.Point(265, 71);
            this.numWordRankTo.Margin = new System.Windows.Forms.Padding(4);
            this.numWordRankTo.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numWordRankTo.Name = "numWordRankTo";
            this.numWordRankTo.Size = new System.Drawing.Size(87, 25);
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
            this.label4.Location = new System.Drawing.Point(224, 74);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "到";
            // 
            // cbxFilterEnglish
            // 
            this.cbxFilterEnglish.AutoSize = true;
            this.cbxFilterEnglish.Location = new System.Drawing.Point(22, 201);
            this.cbxFilterEnglish.Margin = new System.Windows.Forms.Padding(4);
            this.cbxFilterEnglish.Name = "cbxFilterEnglish";
            this.cbxFilterEnglish.Size = new System.Drawing.Size(149, 19);
            this.cbxFilterEnglish.TabIndex = 5;
            this.cbxFilterEnglish.Text = "过滤包含英文的词";
            this.cbxFilterEnglish.UseVisualStyleBackColor = true;
            // 
            // cbxFilterSpace
            // 
            this.cbxFilterSpace.AutoSize = true;
            this.cbxFilterSpace.Location = new System.Drawing.Point(22, 290);
            this.cbxFilterSpace.Margin = new System.Windows.Forms.Padding(4);
            this.cbxFilterSpace.Name = "cbxFilterSpace";
            this.cbxFilterSpace.Size = new System.Drawing.Size(149, 19);
            this.cbxFilterSpace.TabIndex = 6;
            this.cbxFilterSpace.Text = "过滤包含空格的词";
            this.cbxFilterSpace.UseVisualStyleBackColor = true;
            // 
            // cbxFilterPunctuation
            // 
            this.cbxFilterPunctuation.AutoSize = true;
            this.cbxFilterPunctuation.Location = new System.Drawing.Point(22, 336);
            this.cbxFilterPunctuation.Margin = new System.Windows.Forms.Padding(4);
            this.cbxFilterPunctuation.Name = "cbxFilterPunctuation";
            this.cbxFilterPunctuation.Size = new System.Drawing.Size(149, 19);
            this.cbxFilterPunctuation.TabIndex = 7;
            this.cbxFilterPunctuation.Text = "过滤包含标点的词";
            this.cbxFilterPunctuation.UseVisualStyleBackColor = true;
            // 
            // cbxNoFilter
            // 
            this.cbxNoFilter.AutoSize = true;
            this.cbxNoFilter.Location = new System.Drawing.Point(22, 155);
            this.cbxNoFilter.Margin = new System.Windows.Forms.Padding(4);
            this.cbxNoFilter.Name = "cbxNoFilter";
            this.cbxNoFilter.Size = new System.Drawing.Size(74, 19);
            this.cbxNoFilter.TabIndex = 8;
            this.cbxNoFilter.Text = "不过滤";
            this.cbxNoFilter.UseVisualStyleBackColor = true;
            // 
            // cbxReplacePunctuation
            // 
            this.cbxReplacePunctuation.AutoSize = true;
            this.cbxReplacePunctuation.Location = new System.Drawing.Point(227, 336);
            this.cbxReplacePunctuation.Margin = new System.Windows.Forms.Padding(4);
            this.cbxReplacePunctuation.Name = "cbxReplacePunctuation";
            this.cbxReplacePunctuation.Size = new System.Drawing.Size(119, 19);
            this.cbxReplacePunctuation.TabIndex = 11;
            this.cbxReplacePunctuation.Text = "替换标点部分";
            this.cbxReplacePunctuation.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceSpace
            // 
            this.cbxReplaceSpace.AutoSize = true;
            this.cbxReplaceSpace.Location = new System.Drawing.Point(227, 290);
            this.cbxReplaceSpace.Margin = new System.Windows.Forms.Padding(4);
            this.cbxReplaceSpace.Name = "cbxReplaceSpace";
            this.cbxReplaceSpace.Size = new System.Drawing.Size(119, 19);
            this.cbxReplaceSpace.TabIndex = 10;
            this.cbxReplaceSpace.Text = "替换空格部分";
            this.cbxReplaceSpace.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceEnglish
            // 
            this.cbxReplaceEnglish.AutoSize = true;
            this.cbxReplaceEnglish.Location = new System.Drawing.Point(227, 201);
            this.cbxReplaceEnglish.Margin = new System.Windows.Forms.Padding(4);
            this.cbxReplaceEnglish.Name = "cbxReplaceEnglish";
            this.cbxReplaceEnglish.Size = new System.Drawing.Size(119, 19);
            this.cbxReplaceEnglish.TabIndex = 9;
            this.cbxReplaceEnglish.Text = "替换英文部分";
            this.cbxReplaceEnglish.UseVisualStyleBackColor = true;
            // 
            // cbxReplaceNumber
            // 
            this.cbxReplaceNumber.AutoSize = true;
            this.cbxReplaceNumber.Location = new System.Drawing.Point(227, 246);
            this.cbxReplaceNumber.Margin = new System.Windows.Forms.Padding(4);
            this.cbxReplaceNumber.Name = "cbxReplaceNumber";
            this.cbxReplaceNumber.Size = new System.Drawing.Size(119, 19);
            this.cbxReplaceNumber.TabIndex = 13;
            this.cbxReplaceNumber.Text = "替换数字部分";
            this.cbxReplaceNumber.UseVisualStyleBackColor = true;
            // 
            // cbxFilterNumber
            // 
            this.cbxFilterNumber.AutoSize = true;
            this.cbxFilterNumber.Location = new System.Drawing.Point(22, 246);
            this.cbxFilterNumber.Margin = new System.Windows.Forms.Padding(4);
            this.cbxFilterNumber.Name = "cbxFilterNumber";
            this.cbxFilterNumber.Size = new System.Drawing.Size(149, 19);
            this.cbxFilterNumber.TabIndex = 12;
            this.cbxFilterNumber.Text = "过滤包含数字的词";
            this.cbxFilterNumber.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 114);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 15);
            this.label5.TabIndex = 14;
            this.label5.Text = "保留高词频：";
            // 
            // numWordRankPercentage
            // 
            this.numWordRankPercentage.Location = new System.Drawing.Point(128, 109);
            this.numWordRankPercentage.Margin = new System.Windows.Forms.Padding(4);
            this.numWordRankPercentage.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWordRankPercentage.Name = "numWordRankPercentage";
            this.numWordRankPercentage.Size = new System.Drawing.Size(87, 25);
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
            this.label6.Location = new System.Drawing.Point(223, 111);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "%";
            // 
            // cbxFilterNoAlphabetCode
            // 
            this.cbxFilterNoAlphabetCode.AutoSize = true;
            this.cbxFilterNoAlphabetCode.Location = new System.Drawing.Point(22, 384);
            this.cbxFilterNoAlphabetCode.Margin = new System.Windows.Forms.Padding(4);
            this.cbxFilterNoAlphabetCode.Name = "cbxFilterNoAlphabetCode";
            this.cbxFilterNoAlphabetCode.Size = new System.Drawing.Size(164, 19);
            this.cbxFilterNoAlphabetCode.TabIndex = 17;
            this.cbxFilterNoAlphabetCode.Text = "过滤非字母编码的词";
            this.cbxFilterNoAlphabetCode.UseVisualStyleBackColor = true;
            // 
            // cbxKeepNumber
            // 
            this.cbxKeepNumber.AutoSize = true;
            this.cbxKeepNumber.Location = new System.Drawing.Point(235, 91);
            this.cbxKeepNumber.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepNumber.Name = "cbxKeepNumber";
            this.cbxKeepNumber.Size = new System.Drawing.Size(194, 19);
            this.cbxKeepNumber.TabIndex = 19;
            this.cbxKeepNumber.Text = "词条中的数字直接当编码";
            this.cbxKeepNumber.UseVisualStyleBackColor = true;
            this.cbxKeepNumber.CheckedChanged += new System.EventHandler(this.cbxKeepNumber_CheckedChanged);
            // 
            // cbxKeepEnglish
            // 
            this.cbxKeepEnglish.AutoSize = true;
            this.cbxKeepEnglish.Checked = true;
            this.cbxKeepEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxKeepEnglish.Location = new System.Drawing.Point(235, 45);
            this.cbxKeepEnglish.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepEnglish.Name = "cbxKeepEnglish";
            this.cbxKeepEnglish.Size = new System.Drawing.Size(194, 19);
            this.cbxKeepEnglish.TabIndex = 18;
            this.cbxKeepEnglish.Text = "词条中的字母直接当编码";
            this.cbxKeepEnglish.UseVisualStyleBackColor = true;
            this.cbxKeepEnglish.CheckedChanged += new System.EventHandler(this.cbxKeepEnglish_CheckedChanged);
            // 
            // cbxFilterFirstCJK
            // 
            this.cbxFilterFirstCJK.AutoSize = true;
            this.cbxFilterFirstCJK.Checked = true;
            this.cbxFilterFirstCJK.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxFilterFirstCJK.Location = new System.Drawing.Point(22, 431);
            this.cbxFilterFirstCJK.Margin = new System.Windows.Forms.Padding(4);
            this.cbxFilterFirstCJK.Name = "cbxFilterFirstCJK";
            this.cbxFilterFirstCJK.Size = new System.Drawing.Size(269, 19);
            this.cbxFilterFirstCJK.TabIndex = 20;
            this.cbxFilterFirstCJK.Text = "过滤首字非中日韩统一表义字符的词";
            this.cbxFilterFirstCJK.UseVisualStyleBackColor = true;
            // 
            // cbxKeepNumber_
            // 
            this.cbxKeepNumber_.AutoSize = true;
            this.cbxKeepNumber_.Checked = true;
            this.cbxKeepNumber_.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxKeepNumber_.Location = new System.Drawing.Point(14, 91);
            this.cbxKeepNumber_.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepNumber_.Name = "cbxKeepNumber_";
            this.cbxKeepNumber_.Size = new System.Drawing.Size(164, 19);
            this.cbxKeepNumber_.TabIndex = 22;
            this.cbxKeepNumber_.Text = "词条中的数字不编码";
            this.cbxKeepNumber_.UseVisualStyleBackColor = true;
            this.cbxKeepNumber_.CheckedChanged += new System.EventHandler(this.cbxKeepNumber__CheckedChanged);
            // 
            // cbxKeepEnglish_
            // 
            this.cbxKeepEnglish_.AutoSize = true;
            this.cbxKeepEnglish_.Location = new System.Drawing.Point(14, 45);
            this.cbxKeepEnglish_.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepEnglish_.Name = "cbxKeepEnglish_";
            this.cbxKeepEnglish_.Size = new System.Drawing.Size(164, 19);
            this.cbxKeepEnglish_.TabIndex = 21;
            this.cbxKeepEnglish_.Text = "词条中的字母不编码";
            this.cbxKeepEnglish_.UseVisualStyleBackColor = true;
            this.cbxKeepEnglish_.CheckedChanged += new System.EventHandler(this.cbxKeepEnglish__CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbxKeepSpace);
            this.groupBox1.Controls.Add(this.cbxKeepSpace_);
            this.groupBox1.Controls.Add(this.cbxChsNumber);
            this.groupBox1.Controls.Add(this.cbxPrefixEnglish);
            this.groupBox1.Controls.Add(this.cbxKeepPunctuation_);
            this.groupBox1.Controls.Add(this.cbxKeepPunctuation);
            this.groupBox1.Controls.Add(this.cbxKeepNumber_);
            this.groupBox1.Controls.Add(this.cbxKeepEnglish_);
            this.groupBox1.Controls.Add(this.cbxKeepNumber);
            this.groupBox1.Controls.Add(this.cbxKeepEnglish);
            this.groupBox1.Location = new System.Drawing.Point(423, 155);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(471, 269);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "词条分段处理";
            // 
            // cbxChsNumber
            // 
            this.cbxChsNumber.AutoSize = true;
            this.cbxChsNumber.Location = new System.Drawing.Point(235, 229);
            this.cbxChsNumber.Margin = new System.Windows.Forms.Padding(4);
            this.cbxChsNumber.Name = "cbxChsNumber";
            this.cbxChsNumber.Size = new System.Drawing.Size(194, 19);
            this.cbxChsNumber.TabIndex = 26;
            this.cbxChsNumber.Text = "数字先转汉字再进行编码";
            this.cbxChsNumber.UseVisualStyleBackColor = true;
            this.cbxChsNumber.CheckedChanged += new System.EventHandler(this.cbxChsNumber_CheckedChanged);
            // 
            // cbxPrefixEnglish
            // 
            this.cbxPrefixEnglish.AutoSize = true;
            this.cbxPrefixEnglish.Checked = true;
            this.cbxPrefixEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxPrefixEnglish.Location = new System.Drawing.Point(14, 229);
            this.cbxPrefixEnglish.Margin = new System.Windows.Forms.Padding(4);
            this.cbxPrefixEnglish.Name = "cbxPrefixEnglish";
            this.cbxPrefixEnglish.Size = new System.Drawing.Size(218, 19);
            this.cbxPrefixEnglish.TabIndex = 25;
            this.cbxPrefixEnglish.Text = "中英文的编码中间用\"_\"分隔";
            this.cbxPrefixEnglish.UseVisualStyleBackColor = true;
            this.cbxPrefixEnglish.CheckedChanged += new System.EventHandler(this.cbxPrefixEnglish_CheckedChanged);
            // 
            // cbxKeepPunctuation_
            // 
            this.cbxKeepPunctuation_.AutoSize = true;
            this.cbxKeepPunctuation_.Checked = true;
            this.cbxKeepPunctuation_.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxKeepPunctuation_.Location = new System.Drawing.Point(14, 181);
            this.cbxKeepPunctuation_.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepPunctuation_.Name = "cbxKeepPunctuation_";
            this.cbxKeepPunctuation_.Size = new System.Drawing.Size(164, 19);
            this.cbxKeepPunctuation_.TabIndex = 24;
            this.cbxKeepPunctuation_.Text = "词条中的标点不编码";
            this.cbxKeepPunctuation_.UseVisualStyleBackColor = true;
            this.cbxKeepPunctuation_.CheckedChanged += new System.EventHandler(this.cbxKeepPunctuation__CheckedChanged);
            // 
            // cbxKeepPunctuation
            // 
            this.cbxKeepPunctuation.AutoSize = true;
            this.cbxKeepPunctuation.Location = new System.Drawing.Point(235, 181);
            this.cbxKeepPunctuation.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepPunctuation.Name = "cbxKeepPunctuation";
            this.cbxKeepPunctuation.Size = new System.Drawing.Size(194, 19);
            this.cbxKeepPunctuation.TabIndex = 23;
            this.cbxKeepPunctuation.Text = "词条中的标点直接当编码";
            this.cbxKeepPunctuation.UseVisualStyleBackColor = true;
            this.cbxKeepPunctuation.CheckedChanged += new System.EventHandler(this.cbxKeepPunctuator_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(210, 155);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 269);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "词条字符替换";
            // 
            // cbxKeepSpace_
            // 
            this.cbxKeepSpace_.AutoSize = true;
            this.cbxKeepSpace_.Location = new System.Drawing.Point(14, 135);
            this.cbxKeepSpace_.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepSpace_.Name = "cbxKeepSpace_";
            this.cbxKeepSpace_.Size = new System.Drawing.Size(164, 19);
            this.cbxKeepSpace_.TabIndex = 27;
            this.cbxKeepSpace_.Text = "词条中的空格不编码";
            this.cbxKeepSpace_.UseVisualStyleBackColor = true;
            // 
            // cbxKeepSpace
            // 
            this.cbxKeepSpace.AutoSize = true;
            this.cbxKeepSpace.Location = new System.Drawing.Point(235, 135);
            this.cbxKeepSpace.Margin = new System.Windows.Forms.Padding(4);
            this.cbxKeepSpace.Name = "cbxKeepSpace";
            this.cbxKeepSpace.Size = new System.Drawing.Size(194, 19);
            this.cbxKeepSpace.TabIndex = 28;
            this.cbxKeepSpace.Text = "编码时保留字母后的空格";
            this.cbxKeepSpace.UseVisualStyleBackColor = true;
            // 
            // FilterConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 485);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbxFilterFirstCJK);
            this.Controls.Add(this.cbxFilterNoAlphabetCode);
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
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
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
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
    }
}