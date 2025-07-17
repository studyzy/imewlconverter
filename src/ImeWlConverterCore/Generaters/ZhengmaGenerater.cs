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
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters;

public class ZhengmaGenerater : BaseCodeGenerater, IWordCodeGenerater
{
    private Dictionary<char, Zhengma> zhengmaDic;

    private Dictionary<char, Zhengma> ZhengmaDic
    {
        get
        {
            if (zhengmaDic == null)
            {
                var txt = DictionaryHelper.GetResourceContent("Zhengma.txt");

                zhengmaDic = new Dictionary<char, Zhengma>();
                foreach (
                    var line in txt.Split(
                        new[] { "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                {
                    var arr = line.Split('\t');
                    if (arr[0].Length == 0) continue;
                    var word = arr[0][0];
                    var shortCode = arr[1].Trim();
                    var codes = new List<string>();
                    for (var i = 1; i < arr.Length; i++)
                    {
                        var code = arr[i].Trim();
                        if (code != "") codes.Add(code);
                    }

                    zhengmaDic.Add(word, new Zhengma { ShortCode = shortCode, Code = codes });
                }
            }

            return zhengmaDic;
        }
    }

    private string Get2Code(char c)
    {
        var codes = ZhengmaDic[c];
        return codes.ShortCode;
    }

    private string Get1Code(char c)
    {
        return ZhengmaDic[c].ShortCode[0].ToString();
    }


    #region Nested type: Zhengma

    private struct Zhengma
    {
        /// <summary>
        ///     构词码
        /// </summary>
        public string ShortCode { get; set; }

        /// <summary>
        ///     单字郑码
        /// </summary>
        public IList<string> Code { get; set; }
    }

    #endregion

    #region IWordCodeGenerater Members

    public override Code GetCodeOfString(string str)
    {
        foreach (var c in str)
            if (!ZhengmaDic.ContainsKey(c))
                return null;

        if (str.Length == 1) return new Code(ZhengmaDic[str[0]].Code, false);
        var codes = new StringBuilder();
        if (str.Length == 2) //二字词组 2+2
        {
            codes.Append(Get2Code(str[0]));
            codes.Append(Get2Code(str[1]));
        }
        else if (str.Length == 3) //三字词组 1+2+1
        {
            codes.Append(Get1Code(str[0]));
            codes.Append(Get2Code(str[1]));
            codes.Append(Get1Code(str[2]));
        }
        else
        {
            codes.Append(Get1Code(str[0]));
            codes.Append(Get1Code(str[1]));
            codes.Append(Get1Code(str[2]));
            codes.Append(Get1Code(str[3]));
        }

        return new Code(codes.ToString());
    }

    public IList<string> GetAllCodesOfChar(char str)
    {
        return ZhengmaDic[str].Code;
    }

    public bool Is1CharMutiCode => false;

    public bool Is1Char1Code => false;

    #endregion
}
