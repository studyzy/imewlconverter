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

using Microsoft.VisualBasic;

namespace Studyzy.IMEWLConverter.Language
{
    internal class MsVbComponent : IChineseConverter
    {
        #region IChineseConverter Members

        public string ToChs(string cht)
        {
            return Strings.StrConv(cht, VbStrConv.SimplifiedChinese, 0);
        }

        public string ToCht(string chs)
        {
            return Strings.StrConv(chs, VbStrConv.TraditionalChinese, 0);
        }

        #endregion

        public void Init() { }
    }
}
