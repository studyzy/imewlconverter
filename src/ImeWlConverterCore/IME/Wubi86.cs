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
    /// <summary>
    /// 搜狗五笔的词库格式为“五笔编码 词语”\r\n
    /// </summary>
    [ComboBoxShow(ConstantString.WUBI86, ConstantString.WUBI86_C, 210)]
    public class Wubi86 : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        public override CodeType CodeType
        {
            get { return CodeType.Wubi; }
        }

        #region IWordLibraryExport 成员

        //private readonly IWordCodeGenerater wubiGenerater = new Wubi86Generater();

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.WubiCode);
            sb.Append(" ");
            sb.Append(wl.Word);

            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                try
                {
                    sb.Append(ExportLine(wlList[i]));
                    sb.Append("\r\n");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
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
            string code = line.Split(' ')[0];
            string word = line.Split(' ')[1];
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = DefaultRank;
            wl.SetCode(CodeType.Wubi, code);
            //wl.PinYin = CollectionHelper.ToArray(pinyinFactory.GetCodeOfString(word));
            var wll = new WordLibraryList();
            if (wl.PinYin.Length > 0)
            {
                wll.Add(wl);
            }
            return wll;
        }

        #endregion

        //private readonly IWordCodeGenerater pinyinFactory = new PinyinGenerater();
    }
}
