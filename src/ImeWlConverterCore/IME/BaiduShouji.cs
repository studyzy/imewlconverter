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
    [ComboBoxShow(ConstantString.BAIDU_SHOUJI, ConstantString.BAIDU_SHOUJI_C, 1000)]
    public class BaiduShouji : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员
        /// <summary>
        /// 百度手机的格式为 中文(pin|yin) 词频
        /// </summary>
        /// <param name="wl"></param>
        /// <returns></returns>
        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("(");
            sb.Append(wl.GetPinYinString("|", BuildType.None));
            sb.Append(") 20000");

            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }
            return new List<string>() { sb.ToString() };
        }

        #endregion
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #region IWordLibraryImport 成员



        public override WordLibraryList ImportLine(string line)
        {
            var wll = new WordLibraryList();

            var array1 = line.Split('(');
            string word = array1[0];
            string py = array1[1].Split(')')[0];

            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = 1;
            wl.PinYin = py.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            wll.Add(wl);

            return wll;
        }

        #endregion
    }
}
