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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// RIME是一个输入法框架，支持多种输入法编码，词库规则是：
    /// 词语+Tab+编码（拼音空格隔开）+Tab+词频
    ///
    /// </summary>
    [ComboBoxShow(ConstantString.RIME, ConstantString.RIME_C, 150)]
    public class Rime : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport, IMultiCodeType
    {
        private string lineSplitString;

        public Rime()
        {
            CodeType = CodeType.Pinyin;
            OS = OperationSystem.Windows;
        }

        private OperationSystem os;

        public OperationSystem OS
        {
            get { return os; }
            set
            {
                os = value;
                lineSplitString = GetLineSplit(os);
            }
        }

        private string GetLineSplit(OperationSystem os)
        {
            switch (os)
            {
                case OperationSystem.Windows:
                    return "\r\n";

                case OperationSystem.MacOS:
                    return "\r";

                case OperationSystem.Linux:
                    return "\n";
            }
            return "\r\n";
        }

        #region IWordLibraryExport 成员

        private IWordCodeGenerater codeGenerater;

        //private RimeConfigForm form;

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            if (
                this.CodeType == wl.CodeType
                && this.CodeType != CodeType.Pinyin
                && CodeType != CodeType.TerraPinyin
            )
            {
                return wl.Word + "\t" + wl.Codes[0][0] + "\t" + wl.Rank;
            }

            if (codeGenerater == null)
            {
                codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
            }
            try
            {
                codeGenerater.GetCodeOfWordLibrary(wl);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                return null;
            }

            if (codeGenerater.Is1CharMutiCode)
            {
                IList<string> codes = codeGenerater.GetCodeOfString(wl.Word).ToCodeString(" ");
                int i = 0;
                foreach (string code in codes)
                {
                    sb.Append(wl.Word);
                    sb.Append("\t");
                    sb.Append(code);
                    sb.Append("\t");
                    sb.Append(wl.Rank);
                    i++;
                    if (i != codes.Count)
                        sb.Append(lineSplitString);
                }
            }
            else
            {
                sb.Append(wl.Word);
                sb.Append("\t");
                if (CodeType == CodeType.Pinyin || CodeType == CodeType.TerraPinyin)
                {
                    sb.Append(wl.GetPinYinString(" ", BuildType.None));
                }
                else if (CodeType == wl.CodeType)
                {
                    sb.Append(wl.Codes[0][0]);
                }
                else
                {
                    sb.Append(wl.Codes.ToCodeString(" ")[0]);
                }
                sb.Append("\t");
                sb.Append(wl.Rank);
            }
            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                var line = ExportLine(wlList[i]);
                if (!string.IsNullOrEmpty(line))
                {
                    sb.Append(line);
                    sb.Append(lineSplitString);
                }
            }
            return new List<string>() { sb.ToString() };
        }

        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }

        #endregion

        #region IWordLibraryImport 成员


        //private IWordCodeGenerater pyGenerater=new PinyinGenerater();
        public override WordLibraryList ImportLine(string line)
        {
            string[] lineArray = line.Split('\t');

            string word = lineArray[0];
            string code = lineArray[1];
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = Convert.ToInt32(lineArray[2]);
            if (CodeType == CodeType.Pinyin)
            {
                wl.PinYin = code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                //wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(wl.Word));
                wl.SetCode(CodeType, code);
            }

            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}
