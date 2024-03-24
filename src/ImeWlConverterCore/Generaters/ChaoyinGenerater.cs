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
    public class ChaoyinGenerater : IWordCodeGenerater
    {
        protected PinyinGenerater pinyinGenerater = new PinyinGenerater();

        public bool Is1Char1Code
        {
            get { return false; }
        }

        public bool Is1CharMutiCode
        {
            get { return true; }
        }

        public Code GetCodeOfString(string str)
        {
            var pyCode = pinyinGenerater.GetCodeOfString(str);
            var pyList = pyCode.GetDefaultCode();
            return new Code(ChaoyinHelper.GetChaoyin(pyList));
        }

        public void GetCodeOfWordLibrary(WordLibrary wl)
        {
            if (wl.CodeType == CodeType.Pinyin)
            {
                var code = ChaoyinHelper.GetChaoyin(wl.PinYin);
                wl.SetCode(CodeType.Chaoyin, code);
            }
            else
            {
                wl.SetCode(CodeType.Chaoyin, GetCodeOfString(wl.Word));
            }
        }

        public IList<string> GetAllCodesOfChar(char str)
        {
            var pyCode = pinyinGenerater.GetAllCodesOfChar(str);
            var result = new List<string>();
            foreach (var py in pyCode)
            {
                result.Add(ChaoyinHelper.GetChaoyin(py));
            }
            return result;
        }
    }
}
