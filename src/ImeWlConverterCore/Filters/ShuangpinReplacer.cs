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
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 将普通的拼音编码替换成小鹤双拼
    /// </summary>
    public class ShuangpinReplacer : IReplaceFilter
    {
        private Dictionary<string, string> mapping = new Dictionary<string, string>();

        public ShuangpinReplacer(PinyinType type)
        {
            string pinyinMapping = DictionaryHelper.GetResourceContent("Shuangpin.txt");
            foreach (
                var line in pinyinMapping.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                )
            )
            {
                var arr = line.Split('\t');
                var pinyin = arr[0];
                var shuangpin = arr[(int)type];
                mapping[pinyin] = shuangpin;
            }
        }

        public bool ReplaceAfterCode => true;

        public void Replace(WordLibrary wl)
        {
            if (wl.CodeType != CodeType.Pinyin) //必须是拼音才能被双拼替换
            {
                return;
            }
            foreach (var code in wl.Codes)
            {
                for (var i = 0; i < code.Count; i++)
                {
                    try
                    {
                        code[i] = mapping[code[i]];
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " " + code[i]);
                    }
                }
            }
        }
    }
}
