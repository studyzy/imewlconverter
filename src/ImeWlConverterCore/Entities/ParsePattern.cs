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
using System.Text;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Entities
{
    /// <summary>
    ///     根据设置的样式，解析和展示词条
    /// </summary>
    public class ParsePattern
    {
        public ParsePattern()
        {
            Sort = new List<int> { 1, 2, 3 };
            CodeType = CodeType.Pinyin;
            IsPinyinFormat = true;
            LineSplitString = "\r\n";
            TextEncoding = Encoding.Default;
            ContainCode = true;
            ContainRank = true;
            CodeSplitType = BuildType.None;
        }

        /// <summary>
        ///     打开或保存自定义编码的文件时，使用的编码格式
        /// </summary>
        public Encoding TextEncoding { get; set; }

        /// <summary>
        ///     对于多个字的编码的设定(比如：code_e2=p11+p12+p21+p22)
        /// </summary>
        public string MutiWordCodeFormat { get; set; }

        /// <summary>
        ///     是否包含编码
        /// </summary>
        public bool ContainCode { get; set; }

        /// <summary>
        ///     是否包含词频
        /// </summary>
        public bool ContainRank { get; set; }

        /// <summary>
        ///     编码之间的分隔符
        /// </summary>
        public string CodeSplitString { get; set; }

        /// <summary>
        ///     编码、词频、汉字之间的分隔符
        /// </summary>
        public string SplitString { get; set; }

        /// <summary>
        ///     换行符
        /// </summary>
        public string LineSplitString { get; set; }

        /// <summary>
        ///     编码的分隔符所在位置
        /// </summary>
        public BuildType CodeSplitType { get; set; }

        /// <summary>
        ///     编码\汉字\词频的排序方式
        /// </summary>
        public List<int> Sort { get; set; }

        /// <summary>
        ///     每个字对应的编码的文件路径
        /// </summary>
        public string MappingTablePath { get; set; }

        /// <summary>
        ///     编码类型：拼音，五笔...
        /// </summary>
        public CodeType CodeType { get; set; }
        public OperationSystem OS { get; set; } = OperationSystem.Windows;

        /// <summary>
        ///     是否是拼音这种一字一码的编码规则，不是则需要采用MutiWordCodeFormat计算词语的编码
        /// </summary>
        public bool IsPinyinFormat { get; set; }

        #region 生成指定格式的字符串

        //public string BuildWlString(string word)
        //{
        //    //WordLibrary wl=new WordLibrary(){Word = word};
        //    return BuildWlString(word,null,1);

        //}
        /// <summary>
        ///     只有单词的情况下，根据规则生成目标格式的编码
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        /// <summary>
        ///     根据Pattern设置，将WordLibrary转换成该格式的字符串
        /// </summary>
        /// <param name="wl"></param>
        /// <returns></returns>
        public string BuildWlString(WordLibrary wl)
        {
            string code = "";
            if (ContainCode)
            {
                if (IsPinyinFormat)
                {
                    code = CollectionHelper.GetString(
                        CollectionHelper.DescarteIndex1(wl.Codes),
                        CodeSplitString,
                        CodeSplitType
                    );
                }
                else //多字一码，根据用户设置的编码规则，生成编码
                {
                    code = wl.SingleCode;
                }
            }
            return BuildWlString(wl.Word, code, wl.Rank);
        }

        /// <summary>
        ///     根据Pattern设置，将汉字、编码、词频组合成设置的格式
        /// </summary>
        /// <param name="word"></param>
        /// <param name="code"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public string BuildWlString(string word, string code, int rank)
        {
            string cp = "";
            if (!ContainCode)
            {
                code = "";
            }
            //else if(string.IsNullOrEmpty(code))//没有Code就生成Code
            //{
            //    GenerateCode(word)
            //}
            var sb = new StringBuilder();
            if (ContainRank)
            {
                cp = rank.ToString();
            }
            var dic = new Dictionary<int, string>();
            dic.Add(Sort[0], code);
            dic.Add(Sort[1], word);
            dic.Add(Sort[2], cp);
            var newSort = new List<int>(Sort);
            newSort.Sort();
            foreach (int x in newSort)
            {
                if (dic[x] != "")
                {
                    sb.Append(dic[x] + SplitString);
                }
            }
            string str = sb.ToString();
            return str.Substring(0, str.LastIndexOf(SplitString));
        }

        #endregion

        //#region Example

        ///// <summary>
        ///// 按照指定规则，生成一个示例
        ///// </summary>
        ///// <returns></returns>
        //public string BuildWLStringSample()
        //{

        //    IDictionary<char, string> dic = new Dictionary<char, string>()
        //        {
        //            {'深', "shen"},
        //            {'蓝', "lan"},
        //            {'词', "ci"},
        //            {'库', "ku"},
        //            {'转', "zhuan"},
        //            {'换', "huan"}
        //        };
        //   if (!IsPinyin)
        //   {
        //       if (!string.IsNullOrEmpty(MappingTablePath))
        //       {
        //           dic = UserCodingHelper.GetCodingDict(MappingTablePath);
        //       }
        //   }
        //    string word = "";
        //    string result = "";

        //    foreach (var c in sample.Word)
        //    {
        //        word += c;

        //        result += BuildWlString(dic,1234,word) + "\r\n";
        //    }

        //    return result;
        //}
        ///// <summary>
        ///// 传入一个字与码的集合，以及词频，根据用户设定的格式，生成一条词条字符串
        ///// </summary>
        ///// <param name="charCodes"></param>
        ///// <param name="rank"></param>
        ///// <returns></returns>
        //private string BuildWlString(IDictionary<char, string> charCodes, int rank, string word = "")
        //{
        //    string code = "";
        //    if (word == "")
        //    {
        //        foreach (var c in charCodes.Keys)
        //        {
        //            word += c;
        //        }
        //    }
        //    if (ContainCode)
        //    {
        //        if (IsPinyinFormat)
        //        {
        //            code = CollectionHelper.GetString(GetSelectWordCodes(word, charCodes), CodeSplitString, CodeSplitType);
        //        }
        //        else//多字一码，根据用户设置的编码规则，生成编码
        //        {
        //            selfFactory.MutiWordCodeFormat = MutiWordCodeFormat;
        //            selfFactory.MappingDictionary = charCodes;

        //            code = selfFactory.GetCodeOfString(word)[0];
        //        }
        //    }
        //    return BuildWlString(word, code, rank);

        //}
        //#endregion

        //public void CodingString(WordLibrary wl, IWordCodeGenerater factory)
        //{
        //    var codes = new List<string>();
        //    foreach (char c in wl.Word)
        //    {
        //        string code = factory.GetDefaultCodeOfChar(c);
        //        codes.Add(code);
        //    }
        //    wl.PinYin = codes.ToArray();
        //}

        //private IEnumerable<string> GetSelectWordCodes(string word, IDictionary<char, string> charCodes)
        //{
        //    if (word == "")
        //    {
        //        return charCodes.Values;
        //    }
        //    var result = new List<string>();
        //    foreach (var c in word)
        //    {
        //        result.Add(charCodes[c]);
        //    }
        //    return result;
        //}
    }
}
