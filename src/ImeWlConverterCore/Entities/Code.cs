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
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Entities
{
    public class Code : List<IList<string>>
    {
        public Code(IEnumerable<IList<string>> code)
        {
            foreach (var c in code)
            {
                this.Add(c);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="is1Char1Code">是否是单拼音这样的一字一码，如果不是则表示为一词多码</param>
        public Code(IList<string> code, bool is1Char1Code)
        {
            if (is1Char1Code)
            {
                foreach (var py in code)
                {
                    this.Add(new List<string> { py });
                }
            }
            else
            {
                this.Add(code);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code">五笔这种一词一码类型</param>
        public Code(string code)
        {
            this.Add(new List<string>() { code });
        }

        public Code() { }

        /// <summary>
        /// 取得每个字的编码的第一个编码
        /// </summary>
        /// <returns></returns>
        public IList<string> GetDefaultCode()
        {
            var result = new List<string>();
            foreach (var row in this)
            {
                result.Add(row[0]);
            }
            return result;
        }

        public IList<string> ToCodeString(
            string codeSplit = "",
            BuildType buildType = BuildType.None
        )
        {
            return CollectionHelper.CartesianProduct(this, codeSplit, buildType);
        }

        /// <summary>
        /// 取得第一行第一列编码
        /// </summary>
        /// <returns></returns>
        public string GetTop1Code()
        {
            return this[0][0];
        }
    }
}
