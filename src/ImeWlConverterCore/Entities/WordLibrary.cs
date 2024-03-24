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
using System.Diagnostics;
using System.Linq;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Entities
{
    /// <summary>
    ///     词条类
    /// </summary>
    public class WordLibrary
    {
        public WordLibrary()
        {
            CodeType = CodeType.Pinyin;
            Codes = new Code();
        }

        #region 基本属性

        private bool isEnglish;
        private string pinYinString = "";
        private int rank;
        private string word;

        /// <summary>
        ///     该词条是否是英文词条,是则不处理编码
        /// </summary>
        public bool IsEnglish
        {
            get { return isEnglish; }
            set { isEnglish = value; }
        }

        /// <summary>
        ///     词语
        /// </summary>
        public string Word
        {
            get { return word; }
            set { word = value; }
        }

        /// <summary>
        ///     词频，打字出现次数
        /// </summary>
        public int Rank
        {
            get { return rank; }
            set { rank = value; }
        }

        public CodeType CodeType { get; set; }

        /// <summary>
        ///     如果是一词一码，那么Codes[0][0]就是编码，比如五笔
        /// 如果是一词多码，那么Codes[0]就是所有编码，比如二笔
        ///     如果是一字一码，字无多码，那么Codes[n][0]就是每个字的编码，比如去多音字的拼音
        ///     如果是一字多码，那么Codes[0]就是第一个字的所有编码，Codes[1]是第二个字的所有编码，以此类推，比如含多音字的拼音
        /// </summary>
        public Code Codes { get; set; }

        /// <summary>
        ///     一词一码，取Codes[0][0]
        /// </summary>
        public string SingleCode
        {
            get
            {
                if (Codes == null) //Code生成失败
                {
                    return null;
                }
                if (Codes.Count > 0)
                {
                    if (Codes[0].Count > 0)
                    {
                        return Codes[0][0];
                    }
                }
                return "";
            }
        }

        #endregion

        #region 扩展属性和方法

        //private static IWordCodeGenerater pyGenerater=new PinyinGenerater();
        //private string[] tempPinyin;
        /// <summary>
        ///     词中每个字的拼音(已消除多音字)
        /// </summary>
        public string[] PinYin
        {
            get
            {
                if (
                    (
                        CodeType == CodeType.Pinyin
                        || CodeType == CodeType.Zhuyin
                        || CodeType == CodeType.TerraPinyin
                    )
                    && Codes.Count > 0
                )
                {
                    var result = new string[Codes.Count];
                    int i = 0;
                    foreach (var list in Codes)
                    {
                        var code = (List<string>)list;
                        result[i++] = code[0];
                    }
                    return result;
                }

                return Codes[0].ToArray();
            }
            set
            {
                //CodeType=CodeType.Pinyin;
                Codes = new Code(value, true);
                int i = 0;
                foreach (string s in value)
                {
                    Codes[i++] = new List<string> { s };
                }
            }
        }

        public int GetPinYinLength()
        {
            int len = 0;
            if (Codes.Count > 1 || Codes[0].Count > 1)
            {
                foreach (var s in PinYin)
                {
                    len += s.Length;
                }
            }
            else
                len = Codes[0][0].Length;
            return len;
        }

        /// <summary>
        ///     词的拼音字符串，可以单独设置的一个属性，如果没有设置该属性，而获取该属性，则返回PinYin属性和“'”组合的字符串
        /// </summary>
        public string PinYinString
        {
            get
            {
                if (pinYinString == "" && !isEnglish)
                {
                    pinYinString = string.Join("'", PinYin);
                }
                return pinYinString;
            }
            set { pinYinString = value; }
        }

        public string WubiCode
        {
            get
            {
                if (
                    CodeType == CodeType.Wubi
                    || CodeType == CodeType.Wubi98
                    || CodeType == CodeType.WubiNewAge
                )
                {
                    return Codes[0][0];
                }
                return null;
            }
        }

        /// <summary>
        ///     添加一种编码类型和词对应的编码，针对一词一码的情况
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        public void SetCode(CodeType type, string str)
        {
            CodeType = type;
            Codes = new Code(str);
        }

        /// <summary>
        /// 设置没有任何分隔符的拼音，由系统重新分割开
        /// </summary>
        /// <param name="pinyin"></param>
        public void SetPinyinString(string pinyin)
        {
            foreach (var c in word)
            {
                var pys = PinyinHelper.GetPinYinOfChar(c);
                var match = false;
                foreach (var cpy in pys)
                {
                    if (pinyin.StartsWith(cpy)) //拼音匹配正确
                    {
                        match = true;
                        pinyin = pinyin.Substring(cpy.Length);
                        Codes.Add(new List<string> { cpy });
                        break;
                    }
                }
                if (!match) //这个字的所有拼音都不和传入的字符串中的拼音匹配
                {
                    Codes.Add(pys);
                }
            }
        }

        /// <summary>
        ///     设置无多音字的词的编码
        /// </summary>
        /// <param name="type"></param>
        /// <param name="codes"></param>
        public void SetCode(CodeType type, IList<string> codes, bool is1Char1Code)
        {
            CodeType = type;
            if (codes == null)
            {
                Debug.WriteLine("没有生成新编码，无法设置");
                Codes.Clear();
                return;
            }
            Codes = new Code(codes, is1Char1Code);
        }

        public void SetCode(CodeType type, IList<IList<string>> str)
        {
            CodeType = type;
            Codes = new Code(str);
        }

        public void SetCode(CodeType type, Code code)
        {
            CodeType = type;
            Codes = code;
        }

        /// <summary>
        ///     获得拼音字符串
        /// </summary>
        /// <param name="split">每个拼音之间的分隔符</param>
        /// <param name="buildType">组装拼音字符串的方式</param>
        /// <returns></returns>
        public string GetPinYinString(string split, BuildType buildType)
        {
            return CollectionHelper.GetString(PinYin, split, buildType);
        }

        public IList<string> GetCodeString(string split, BuildType buildType)
        {
            return CollectionHelper.CartesianProduct(Codes, split, buildType);
        }

        //public string ToDisplayString()
        //{
        //    return "汉字：" + word + (string.IsNullOrEmpty(WubiCode) ? "；编码：" + CollectionHelper.ListToString(CollectionHelper.Descartes(Codes)) : "五笔：" + WubiCode) + "；词频：" +
        //           rank;
        //}

        public override string ToString()
        {
            var codesList = Codes.ToCodeString(",");
            return "WordLibrary 汉字："
                + word
                + " Codes:"
                + string.Join(";", codesList.ToArray())
                + " 词频："
                + rank;
        }

        #endregion
    }
}
