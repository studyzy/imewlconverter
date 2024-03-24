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
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    ///     地球拼音输入法，就是带音调的拼音输入法
    /// </summary>
    public class TerraPinyinGenerater : PinyinGenerater
    {
        private static readonly Regex regex = new Regex(@"^[a-zA-Z]+\d$");

        public override IList<string> GetAllCodesOfChar(char str)
        {
            return PinyinHelper.GetPinYinWithToneOfChar(str);
        }

        public override Code GetCodeOfString(string str)
        {
            var py = base.GetCodeOfString(str);
            var result = new List<string>();
            for (int i = 0; i < str.Length; i++)
            {
                var prow = py[i];
                foreach (var p in prow)
                {
                    result.Add(PinyinHelper.AddToneToPinyin(str[i], p));
                }
            }
            return new Code(result, true);
        }

        public override void GetCodeOfWordLibrary(WordLibrary wl)
        {
            if (wl.CodeType == CodeType.TerraPinyin)
            {
                return;
            }
            if (wl.CodeType == CodeType.Pinyin) //如果本来就是拼音输入法导入的，那么就用其拼音，不过得加上音调
            {
                for (int i = 0; i < wl.Codes.Count; i++)
                {
                    var row = wl.Codes[i];
                    for (int j = 0; j < row.Count; j++)
                    {
                        string s = row[j];
                        string py = PinyinHelper.AddToneToPinyin(wl.Word[i], s); //add tone
                        wl.Codes[i][j] = py;
                    }
                }

                return;
            }
            base.GetCodeOfWordLibrary(wl);
        }
    }
}
