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
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /*
     * 二字词：取每个字的前两位编码。例如“计算”取“JP”+“SQ”，即：“JPSQ”。
三字词：取第一字的前二位编码和最后两个字的第一码。例如“计算机”取“JPSJ”。
四字词：取每个字的第一码。例如“兴高采烈”取“XGCL”。
多字词（四字以上词）：取前三字和最后一字的第一码（前三末一）。
     */

    public abstract class ErbiGenerater : BaseCodeGenerater, IWordCodeGenerater
    {
        /// <summary>
        ///     二笔的编码可能是一字多码的
        /// </summary>
        private Dictionary<char, IList<string>> erbiDic;

        /// <summary>
        ///     1是现代二笔，2是音形，3是超强二笔，4是青松二笔
        /// </summary>
        protected abstract int DicColumnIndex { get; }

        protected Dictionary<char, IList<string>> ErbiDic
        {
            get
            {
                if (erbiDic == null)
                {
                    //该字典包含4种编码，1是现代二笔，2是音形，3是超强二笔，4是青松二笔
                    string txt = Helpers.DictionaryHelper.GetResourceContent("Erbi.txt");

                    erbiDic = new Dictionary<char, IList<string>>();
                    foreach (
                        string line in txt.Split(
                            new[] { "\r\n" },
                            StringSplitOptions.RemoveEmptyEntries
                        )
                    )
                    {
                        string[] arr = line.Split('\t');
                        if (arr[0].Length == 0)
                        {
                            continue;
                        }
                        char word = arr[0][0];
                        string code = arr[DicColumnIndex];
                        if (code == "")
                        {
                            code = arr[1];
                        }
                        string[] codes = code.Split(' '); //code之间空格分割
                        erbiDic[word] = new List<string>(codes);
                    }
                    //OverrideDictionary(erbiDic);
                }
                return erbiDic;
            }
        }

        #region IWordCodeGenerater Members

        protected PinyinGenerater pinyinGenerater = new PinyinGenerater();

        public bool Is1Char1Code
        {
            get { return false; }
        }

        public override Code GetCodeOfString(string str)
        {
            var code = pinyinGenerater.GetCodeOfString(str);
            IList<IList<string>> codes = GetErbiCode(str, code.GetDefaultCode());
            IList<string> result = CollectionHelper.Descartes(codes);
            return new Code(result, false);
        }

        public IList<string> GetAllCodesOfChar(char str)
        {
            return ErbiDic[str];
        }

        public bool Is1CharMutiCode
        {
            get { return true; }
        }

        #endregion

        ///// <summary>
        /////     读取外部的字典文件，覆盖系统默认字典
        ///// </summary>
        ///// <param name="dictionary"></param>
        //protected virtual void OverrideDictionary(IDictionary<char, IList<string>> dictionary)
        //{
        //    string fileContent = FileOperationHelper.ReadFile("mb.txt");
        //    if (fileContent != "")
        //    {
        //        foreach (string line in fileContent.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            string[] arr = line.Split('\t');
        //            if (arr[0].Length == 0)
        //            {
        //                continue;
        //            }
        //            char word = arr[0][0];
        //            string code = arr[1];
        //            string[] codes = code.Split(' ');
        //            dictionary[word] = new List<string>(codes); //强行覆盖现有字典
        //        }
        //    }
        //}


        protected virtual IList<IList<string>> GetErbiCode(string str, IList<string> py)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var codes = new List<IList<string>>();

            try
            {
                if (str.Length == 1)
                {
                    codes.Add(Get1CharCode(str[0], py[0]));
                }
                else if (str.Length == 2) //各取2码
                {
                    codes.Add(Get1CharCode(str[0], py[0]));
                    codes.Add(Get1CharCode(str[1], py[1]));
                }
                else if (str.Length == 3)
                {
                    codes.Add(Get1CharCode(str[0], py[0]));
                    codes.Add(new List<string> { py[1][0].ToString() });
                    codes.Add(new List<string> { py[2][0].ToString() });
                }
                else
                {
                    codes.Add(
                        new List<string>
                        {
                            py[0][0].ToString() + py[1][0] + py[2][0] + py[str.Length - 1][0]
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            return codes;
        }

        /// <summary>
        ///     获得一个字的二笔码
        /// </summary>
        /// <param name="c"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        protected IList<string> Get1CharCode(char c, string py)
        {
            var result = new List<string>();
            IList<string> codes = ErbiDic[c];
            foreach (string code in codes)
            {
                result.Add(py[0].ToString() + code[0]);
            }
            return result;
        }
    }
}
