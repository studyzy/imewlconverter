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
    /// Chinese-pyim
    /// http://tumashu.github.io/chinese-pyim
    /// </summary>
    [ComboBoxShow(ConstantString.CHINESE_PYIM, ConstantString.CHINESE_PYIM_C, 177)]
    public class ChinesePyim : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryImport 成员



        public override WordLibraryList ImportLine(string line)
        {
            var array = line.Split(' ');
            var py = array[0].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            for (var i = 1; i < array.Length; i++)
            {
                string word = line.Split(' ')[i];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = 1;
                wl.PinYin = py;

                wll.Add(wl);
            }
            return wll;
        }

        #endregion

        #region IWordLibraryExport 成员

        #region IWordLibraryExport Members

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append(wl.GetPinYinString("-", BuildType.None));
            sb.Append(" ");
            sb.Append(wl.Word);

            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            sb.Append(";; -*- coding: utf-8 -*--\n");

            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\n");
            }
            return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryTextImport Members


        #endregion

        #endregion
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
