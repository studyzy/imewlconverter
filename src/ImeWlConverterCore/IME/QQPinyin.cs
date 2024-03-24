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
    [ComboBoxShow(ConstantString.QQ_PINYIN, ConstantString.QQ_PINYIN_C, 50)]
    public class QQPinyin : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
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
            sb.Append(" ");
            sb.Append(wl.Word);
            sb.Append(" ");
            sb.Append(wl.Rank);
            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            if (wlList.Count == 0)
            {
                return new List<string>();
            }
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count - 1; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                    sb.Append("\r\n");
                }
            }
            WordLibrary last = wlList[wlList.Count - 1];
            sb.Append(ExportLine(last));
            sb.Append(", ");
            sb.Append(last.GetPinYinString("'", BuildType.None));
            sb.Append(" ");
            sb.Append(last.Rank);
            sb.Append("\r\n");
            return new List<string>() { sb.ToString() };
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion

        public override WordLibraryList ImportLine(string line)
        {
            line = line.Split(',')[0]; //如果有逗号，就只取第一个
            string[] sp = line.Split(' ');
            string py = sp[0];
            string word = sp[1];
            int count = Convert.ToInt32(sp[2]);
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = count;
            wl.PinYin = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
    }
}
