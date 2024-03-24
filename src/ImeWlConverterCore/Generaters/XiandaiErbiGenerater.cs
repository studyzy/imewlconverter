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

namespace Studyzy.IMEWLConverter.Generaters
{
    public class XiandaiErbiGenerater : ErbiGenerater
    {
        protected override int DicColumnIndex
        {
            get { return 1; }
        }

        /// <summary>
        ///     现代二笔与普通二笔不同的是，现代二笔组词的时候，每个词取2码，并没有4码长度的限制。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        protected override IList<IList<string>> GetErbiCode(string str, IList<string> py)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var codes = new List<IList<string>>();

            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    codes.Add(Get1CharCode(c, py[i]));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            return codes;
        }
    }
}
