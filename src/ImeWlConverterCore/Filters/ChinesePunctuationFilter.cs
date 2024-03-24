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

using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    ///     过滤包含中文标点的词
    /// </summary>
    public class ChinesePunctuationFilter : ISingleFilter, IReplaceFilter
    {
        public bool ReplaceAfterCode => false;

        #region ISingleFilter Members

        private static readonly Regex regex = new Regex(
            @"[\u3002\uff1b\uff0c\uff1a\u201c\u201d\uff08\uff09\u3001\uff1f\u300a\u300b]"
        );

        public void Replace(WordLibrary wl)
        {
            wl.Word = regex.Replace(wl.Word, "");
        }

        public bool IsKeep(WordLibrary wl)
        {
            return !regex.IsMatch(wl.Word);
        }

        #endregion
    }
}
