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
    [ComboBoxShow(ConstantString.QQ_SHOUJI, ConstantString.QQ_SHOUJI_C, 1030)]
    public class QQShouji : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        private int number = 1;

        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            string py = wl.GetPinYinString("'", BuildType.None);
            sb.Append(py);
            sb.Append(" ");
            sb.Append(wl.Word);
            sb.Append(" ");
            sb.Append(number);
            sb.Append(" Z, ");
            sb.Append(py);
            sb.Append(" ");
            sb.Append(number);
            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                number = (int)Math.Ceiling((wlList.Count - i) * 100.0 / wlList.Count);
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }
            return new List<string>() { sb.ToString() };
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion

        #region IWordLibraryImport 成员

        public override WordLibraryList ImportLine(string line)
        {
            var wll = new WordLibraryList();
            if (line.IndexOf("Z,") > 0)
            {
                string py = line.Split(' ')[0];
                string word = line.Split(' ')[1];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = 1;
                wl.PinYin = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);

                wll.Add(wl);
            }
            return wll;
        }

        #endregion
    }
}
