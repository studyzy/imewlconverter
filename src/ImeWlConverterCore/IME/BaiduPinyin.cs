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
    ///     百度PC输入法，中文词库和英文词库放在同一个文件，中文词库比如“跨年	kua'nian'	1”，英文词库比如“Jira	1”
    /// </summary>
    [ComboBoxShow(ConstantString.BAIDU_PINYIN, ConstantString.BAIDU_PINYIN_C, 90)]
    public class BaiduPinyin : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #region IWordLibraryImport 成员

        public override WordLibraryList ImportLine(string line)
        {
            var wl = new WordLibrary();
            string[] array = line.Split('\t');

            wl.Word = array[0];
            if (array.Length == 2) //English
            {
                wl.IsEnglish = true;
                wl.Rank = Convert.ToInt32(array[1]);
            }
            else
            {
                string py = line.Split('\t')[1];
                wl.PinYin = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
                wl.Rank = Convert.ToInt32(array[2]);
            }

            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion

        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("\t");
            if (!wl.IsEnglish)
            {
                sb.Append(wl.GetPinYinString("'", BuildType.RightContain));
                sb.Append("\t");
            }
            sb.Append(wl.Rank);
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
    }
}
