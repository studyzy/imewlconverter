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
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 仓颉输入法，主要用于台湾
    /// </summary>
    [ComboBoxShow(ConstantString.CANGJIE_PLATFORM, ConstantString.CANGJIE_PLATFORM_C, 230)]
    public class CangjiePlatform : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
    {
        public override CodeType CodeType
        {
            get { return CodeType.Cangjie; }
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

        #region IWordLibraryExport 成员

        //private readonly IWordCodeGenerater codeGenerater = new Cangjie5Generater();



        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            IList<string> codes = wl.Codes[0]; // codeGenerater.GetCodeOfString(wl.Word);
            int i = 0;
            foreach (string code in codes)
            {
                sb.Append(code);
                sb.Append(" ");
                sb.Append(wl.Word);
                i++;
                if (i != codes.Count)
                    sb.Append("\r\n");
            }
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

        #region IWordLibraryImport 成员

        private readonly IWordCodeGenerater pyGenerater = new PinyinGenerater();

        public override WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split(' ');
            var wl = new WordLibrary();
            string code = c[0];
            wl.Word = c[1];
            wl.Rank = DefaultRank;
            wl.SetCode(CodeType.Cangjie, pyGenerater.GetCodeOfString(wl.Word));
            wl.SetCode(CodeType, code);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}
