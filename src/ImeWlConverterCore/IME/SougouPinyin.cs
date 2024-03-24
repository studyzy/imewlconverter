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
    [ComboBoxShow(ConstantString.SOUGOU_PINYIN, ConstantString.SOUGOU_PINYIN_C, 10)]
    public class SougouPinyin : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            //StringBuilder sb = new StringBuilder();

            string str = wl.GetPinYinString("'", BuildType.LeftContain) + " " + wl.Word;

            return str;
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                //sb.Append(" ");
                //sb.Append(wlList[i].Word);

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
            if (line.IndexOf("'") == 0)
            {
                string py = line.Split(' ')[0];
                string word = line.Split(' ')[1];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = 1;
                wl.PinYin = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
                var wll = new WordLibraryList();
                wll.Add(wl);
                return wll;
            }
            return null;
        }

        #endregion
    }
}
