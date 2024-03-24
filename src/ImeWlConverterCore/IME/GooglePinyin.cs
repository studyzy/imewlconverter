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
    ///     Google拼音输入法
    /// </summary>
    [ComboBoxShow(ConstantString.GOOGLE_PINYIN, ConstantString.GOOGLE_PINYIN_C, 110)]
    public class GooglePinyin : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("\t");
            sb.Append(wl.Rank);
            sb.Append("\t");
            sb.Append(wl.GetPinYinString(" ", BuildType.None));

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

        public override Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                try
                {
                    return Encoding.GetEncoding("GBK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        ex.Message + " Your system doesn't support GBK, try to use GB2312."
                    );
                    return Encoding.GetEncoding("GB2312");
                }
            }
        }

        #endregion

        #region IWordLibraryImport 成员



        public override WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split('\t');
            var wl = new WordLibrary();
            wl.Word = c[0];
            wl.Rank = Convert.ToInt32(c[1]);
            wl.PinYin = c[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}
