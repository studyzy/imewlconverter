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
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class Wubi86Generater : IWordCodeGenerater
    {
        #region IWordCodeGenerater Members

        public virtual string GetDefaultCodeOfChar(char str)
        {
            return DictionaryHelper.GetCode(str).Wubi86;
        }

        public Code GetCodeOfString(string str)
        {
            return new Code(GetStringWubiCode(str));
        }

        public virtual IList<string> GetAllCodesOfChar(char str)
        {
            return new List<string> { DictionaryHelper.GetCode(str).Wubi86 };
        }

        public bool Is1CharMutiCode
        {
            get { return false; }
        }

        public bool Is1Char1Code
        {
            get { return false; }
        }

        #endregion


        public virtual void GetCodeOfWordLibrary(WordLibrary wl)
        {
            wl.Codes = GetCodeOfString(wl.Word);
        }

        private string GetStringWubiCode(string str)
        {
            if (str.Length == 1)
            {
                return GetDefaultCodeOfChar(str[0]);
            }
            if (str.Length == 2)
            {
                string code1 = GetDefaultCodeOfChar(str[0]);
                string code2 = GetDefaultCodeOfChar(str[1]);
                return code1.Substring(0, 2) + code2.Substring(0, 2);
            }
            if (str.Length == 3)
            {
                string code1 = GetDefaultCodeOfChar(str[0]);
                string code2 = GetDefaultCodeOfChar(str[1]);
                string code3 = GetDefaultCodeOfChar(str[2]);
                return code1[0].ToString() + code2[0] + code3.Substring(0, 2);
            }
            else
            {
                string code1 = GetDefaultCodeOfChar(str[0]);
                string code2 = GetDefaultCodeOfChar(str[1]);
                string code3 = GetDefaultCodeOfChar(str[2]);
                string code4 = GetDefaultCodeOfChar(str[str.Length - 1]);
                return code1[0].ToString() + code2[0] + code3[0] + code4[0];
            }
        }
    }
}
