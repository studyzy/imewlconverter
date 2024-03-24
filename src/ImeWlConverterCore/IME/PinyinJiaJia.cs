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
    ///
    /// </summary>
    [ComboBoxShow(ConstantString.PINYIN_JIAJIA, ConstantString.PINYIN_JIAJIA_C, 120)]
    public class PinyinJiaJia : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
    {
        #region IWordLibraryExport 成员

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                    sb.Append("\r\n");
                }
            }
            return new List<string>() { sb.ToString() };
        }

        public string ExportLine(WordLibrary wl)
        {
            try
            {
                var sb = new StringBuilder();

                string str = wl.Word;
                for (int j = 0; j < str.Length; j++)
                {
                    sb.Append(str[j] + wl.PinYin[j]);
                }

                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region IWordLibraryImport 成员

        private readonly IWordCodeGenerater single = new PinyinGenerater();

        /// <summary>
        ///     形如：冷血xue动物
        ///     只有多音字才注音，一般的字不注音，就使用默认读音即可
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>

        public override WordLibraryList ImportLine(string word)
        {
            string hz = "";
            var py = new List<string>();
            int j;
            for (j = 0; j < word.Length - 1; j++)
            {
                hz += word[j];
                if (word[j + 1] > 'z') //而且后面跟的不是拼音
                {
                    py.Add(single.GetAllCodesOfChar(word[j])[0]);
                }
                else //后面跟拼音
                {
                    int k = 1;
                    string py1 = "";
                    while (j + k != word.Length && word[j + k] <= 'z')
                    {
                        py1 += word[j + k];
                        k++;
                    }
                    py.Add(py1);
                    j += k - 1; //减1是因为接下来会运行j++
                }
            }
            if (j == word.Length - 1) //最后一个字是汉字
            {
                hz += word[j];
                py.Add(single.GetAllCodesOfChar(word[j])[0]);
            }
            var wl = new WordLibrary();
            wl.PinYin = py.ToArray();
            wl.Word = hz;
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}
