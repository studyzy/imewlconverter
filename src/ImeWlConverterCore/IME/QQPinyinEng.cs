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
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     QQ拼音支持单独的英语词库，使用“英文单词,词频”的格式
    /// </summary>
    [ComboBoxShow(ConstantString.QQ_PINYIN_ENG, ConstantString.QQ_PINYIN_ENG_C, 80)]
    public class QQPinyinEng : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            return wl.Word + "," + wl.Rank;
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count - 1; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }

            return new List<string>() { sb.ToString() };
        }

        public override CodeType CodeType
        {
            get { return CodeType.English; }
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion

        public override WordLibraryList ImportLine(string line)
        {
            string[] sp = line.Split(',');

            string word = sp[0];
            int count = Convert.ToInt32(sp[1]);
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = count;
            wl.CodeType = this.CodeType;
            wl.PinYin = new string[] { };
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
    }
}
