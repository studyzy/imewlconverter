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
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Filters
{
    public class EmojiReplacer : IReplaceFilter
    {
        private Dictionary<string, string> mapping = new Dictionary<string, string>();

        public EmojiReplacer(string path)
        {
            string str = FileOperationHelper.ReadFile(path);
            foreach (
                var line in str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            )
            {
                var arr = line.Split('\t');
                var emoji = arr[0];
                var word = arr[1];
                mapping[word] = emoji;
            }
        }

        public bool ReplaceAfterCode => true;

        public void Replace(WordLibrary wl)
        {
            if (mapping.ContainsKey(wl.Word))
            {
                wl.Word = mapping[wl.Word];
            }
        }
    }
}
