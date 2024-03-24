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

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.LIBPINYIN, ConstantString.LIBPINYIN_C, 175)]
    public class Libpinyin : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append(wl.Word);
            sb.Append(" ");
            try
            {
                string py = wl.GetPinYinString("'", BuildType.None);
                if (string.IsNullOrEmpty(py))
                {
                    return "";
                }
                sb.Append(py);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count - 1; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                    sb.Append("\n");
                }
            }

            return new List<string>() { sb.ToString() };
        }

        #endregion
        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }

        public override WordLibraryList ImportLine(string line)
        {
            string[] sp = line.Split(' ');
            string py = sp[1];
            string word = sp[0];

            var wl = new WordLibrary { CodeType = CodeType.Pinyin };
            wl.Word = word;
            wl.Rank = DefaultRank;
            wl.PinYin = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
    }
}
