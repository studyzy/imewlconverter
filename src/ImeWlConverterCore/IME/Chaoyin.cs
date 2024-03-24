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
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 超音速录输入法
    /// </summary>
    [ComboBoxShow(ConstantString.CHAO_YIN, ConstantString.CHAO_YIN_C, 190)]
    public class Chaoyin : BaseImport, IWordLibraryExport
    {
        public Chaoyin()
        {
            DefaultRank = 1;
            CodeType = CodeType.Chaoyin;
        }

        //#region IWordLibraryImport 成员

        //public WordLibraryList Import(string path)
        //{
        //    string str = FileOperationHelper.ReadFile(path, Encoding);
        //    return ImportText(str);
        //}

        //public WordLibraryList ImportText(string str)
        //{
        //    var wlList = new WordLibraryList();
        //    string[] lines = str.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
        //    CountWord = lines.Length;
        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        string line = lines[i];
        //        CurrentStatus = i;
        //        wlList.AddWordLibraryList(ImportLine(line));
        //    }
        //    return wlList;
        //}


        //public WordLibraryList ImportLine(string line)
        //{
        //    var wl = new WordLibrary();
        //    string[] array = line.Split('\t');

        //    wl.Word = array[0];
        //    if (array.Length == 2) //English
        //    {
        //        wl.IsEnglish = true;
        //        wl.Rank = Convert.ToInt32(array[1]);
        //    }
        //    else
        //    {
        //        string py = line.Split('\t')[1];
        //        wl.PinYin = py.Split(new[] {'\''}, StringSplitOptions.RemoveEmptyEntries);
        //        wl.Rank = Convert.ToInt32(array[2]);
        //    }

        //    var wll = new WordLibraryList();
        //    wll.Add(wl);
        //    return wll;
        //}

        //#endregion
        private readonly IWordCodeGenerater generater = new ChaoyinGenerater();

        #region IWordLibraryExport 成员
        /// <summary>
        /// Code+空格+=+空格+词频+逗号+词语
        /// </summary>
        /// <param name="wl"></param>
        /// <returns></returns>
        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.SingleCode);
            sb.Append(" = ");
            sb.Append(wl.Rank);
            sb.Append(",");
            sb.Append(wl.Word);

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

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion
    }
}
