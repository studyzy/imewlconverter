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

using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.WORD_ONLY, ConstantString.WORD_ONLY_C, 2010)]
    public class NoPinyinWordOnly : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        //private IWordCodeGenerater pinyinFactory;
        public override CodeType CodeType
        {
            get { return CodeType.NoCode; }
        }

        #region IWordLibraryImport 成员

        /// <summary>
        ///     将一行纯文本转换为对象
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public override WordLibraryList ImportLine(string line)
        {
            //IList<string> py = pinyinFactory.GetCodeOfString(line);
            var wl = new WordLibrary();
            wl.Word = line;
            wl.CodeType = CodeType;
            //wl.PinYin = CollectionHelper.ToArray(py);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion

        #region IWordLibraryExport 成员

        #region IWordLibraryExport Members

        public virtual string ExportLine(WordLibrary wl)
        {
            return wl.Word;
        }

        public virtual IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(wlList[i].Word);
                sb.Append("\r\n");
            }

            return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryTextImport Members

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion

        #endregion
    }
}
