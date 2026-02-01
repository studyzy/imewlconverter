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
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters;

public class CalcWordRankGenerater : IWordRankGenerater
{
    public bool ForceUse { get; set; }

    public int GetRank(string word)
    {
        double x = 1;
        foreach (var c in word)
        {
            var freq = DictionaryHelper.GetCode(c).Freq;
            x += freq;
        }

        return (int)x;
    }

    public void GenerateRank(WordLibraryList wordLibraryList, Action<int, int> progressCallback = null)
    {
        for (int i = 0; i < wordLibraryList.Count; i++)
        {
            var wordLibrary = wordLibraryList[i];
            if (wordLibrary.Rank == 0 || ForceUse)
            {
                wordLibrary.Rank = GetRank(wordLibrary.Word);
            }
            progressCallback?.Invoke(i + 1, wordLibraryList.Count);
        }
    }
}
