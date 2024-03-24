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

using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Emoji表情，格式为：第一个字符是表情，Tab键，后面字符是汉字
    /// 😀   汉字
    /// </summary>
    [ComboBoxShow(ConstantString.EMOJI, ConstantString.EMOJI_C, 999)]
    public class Emoji : BaseTextImport, IWordLibraryTextImport
    {
        public override CodeType CodeType
        {
            get { return CodeType.NoCode; }
        }
        public override Encoding Encoding => Encoding.UTF8;

        public override WordLibraryList ImportLine(string line)
        {
            var wl = new WordLibrary();
            wl.Word = line.Split('\t')[1];
            wl.CodeType = CodeType;
            wl.IsEnglish = IsEnglish(wl.Word);
            if (wl.IsEnglish)
            {
                wl.SetCode(CodeType.English, wl.Word);
            }
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        private static Regex regex = new Regex("^[a-zA-Z]+$");

        private bool IsEnglish(string word)
        {
            return regex.IsMatch(word);
        }
    }
}
