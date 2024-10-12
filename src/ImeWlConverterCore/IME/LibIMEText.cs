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
    ///     LibIME (https://github.com/fcitx/libime) 文本格式
    /// </summary>
    [ComboBoxShow(ConstantString.LIBIME_TEXT, ConstantString.LIBIME_TEXT_C, 500)]
    public class LibIMEText : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
    {
        public override Encoding Encoding => Encoding.UTF8;

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append(" ");
            sb.Append(wl.GetPinYinString("'", BuildType.None)
                .Replace("lue", "lve")
                .Replace("nue", "nve"));
            sb.Append(" ");
            sb.Append(wl.Rank);

            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\n");
            }
            return new List<string>() { sb.ToString() };
        }

        public override WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split(' ');
            var wl = new WordLibrary();
            wl.PinYin = c[0].Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
            wl.Word = c[1];
            wl.Rank = Convert.ToInt32(c[2]);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
    }
}
