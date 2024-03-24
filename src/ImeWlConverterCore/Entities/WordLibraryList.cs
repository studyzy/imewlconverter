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

namespace Studyzy.IMEWLConverter.Entities
{
    /// <summary>
    ///     词库类，含有多个词条
    /// </summary>
    public class WordLibraryList : List<WordLibrary>
    {
        /// <summary>
        ///     将词库中重复出现的单词合并成一个词，多词库合并时使用(词重复就算)
        /// </summary>
        public void MergeSameWord()
        {
            var dic = new Dictionary<string, WordLibrary>();
            foreach (WordLibrary wl in this)
            {
                if (!dic.ContainsKey(wl.Word))
                {
                    dic.Add(wl.Word, wl);
                }
            }
            Clear();
            foreach (WordLibrary wl in dic.Values)
            {
                Add(wl);
            }
        }

        public void AddWordLibraryList(WordLibraryList wll)
        {
            if (wll != null)
            {
                AddRange(wll);
            }
        }
    }
}
