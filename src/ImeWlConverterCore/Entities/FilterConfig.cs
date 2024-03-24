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

namespace Studyzy.IMEWLConverter.Entities
{
    public class FilterConfig
    {
        public FilterConfig()
        {
            WordLengthFrom = 1;
            WordLengthTo = 100;
            WordRankFrom = 1;
            WordRankTo = 999999;
            WordRankPercentage = 100;
            IgnoreFirstCJK = true;
            //     ReplaceSpace = true;
            //     ReplacePunctuation = true;
            //     ReplaceNumber = true;
            KeepSpace_ = true;
            KeepSpace = true;
            NoFilter = false;
            KeepEnglish = true;
            KeepNumber_ = true;
            PrefixEnglish = true;
            KeepPunctuation_ = true;
            FullWidth = true;
        }

        public bool NoFilter { get; set; }
        public bool KeepEnglish { get; set; }
        public int WordLengthFrom { get; set; }
        public int WordLengthTo { get; set; }

        public int WordRankFrom { get; set; }
        public int WordRankTo { get; set; }

        public int WordRankPercentage { get; set; }
        public bool IgnoreEnglish { get; set; }
        public bool IgnoreNumber { get; set; }
        public bool IgnoreSpace { get; set; }
        public bool IgnorePunctuation { get; set; }
        public bool IgnoreNoAlphabetCode { get; set; }

        public bool ReplaceNumber { get; set; }
        public bool ReplaceEnglish { get; set; }
        public bool ReplaceSpace { get; set; }
        public bool ReplacePunctuation { get; set; }
        public bool KeepNumber { get; set; }
        public bool IgnoreFirstCJK { get; set; }
        public bool KeepNumber_ { get; set; }
        public bool KeepEnglish_ { get; set; }
        public bool KeepPunctuation { get; set; }
        public bool KeepPunctuation_ { get; set; }

        public bool FullWidth { get; set; }
        public bool ChsNumber { get; set; }
        public bool PrefixEnglish { get; set; }
        public bool KeepSpace_ { get; set; }
        public bool KeepSpace { get; set; }

        public bool needEnglishTag()
        {
            return PrefixEnglish;
        }
    }
}
