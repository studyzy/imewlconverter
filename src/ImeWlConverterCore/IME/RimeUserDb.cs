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
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// RIME是一个输入法框架，支持多种输入法编码，词库规则是：
    /// 词语+Tab+编码（拼音空格隔开）+Tab+词频
    ///
    /// </summary>
    [ComboBoxShow(ConstantString.RIME_USERDB, ConstantString.RIME_USERDB_C, 150)]
    public class RimeUserDb : BaseTextImport, IWordLibraryTextImport, IMultiCodeType
    {
        private string lineSplitString;

        public RimeUserDb()
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

        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }

        #region IWordLibraryImport 成员

        //private IWordCodeGenerater pyGenerater=new PinyinGenerater();
        public override WordLibraryList ImportLine(string line)
        {
            string[] lineArray = line.Split('\t');

            string code = lineArray[0];
            string word = lineArray[1];
            var wl = new WordLibrary();
            wl.Word = word;
            // userdb.txt 的词频不通用, 注释掉
            // wl.Rank = Convert.ToInt32(lineArray[2]);
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
