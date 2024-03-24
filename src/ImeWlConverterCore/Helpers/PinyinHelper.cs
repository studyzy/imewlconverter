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

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class PinyinHelper
    {
        #region Init

        private static readonly Dictionary<char, List<string>> dictionary =
            new Dictionary<char, List<string>>();
        private static readonly Dictionary<char, IList<string>> pyDictionary =
            new Dictionary<char, IList<string>>();

        /// <summary>
        ///     字的拼音(包括音调)
        /// </summary>
        private static Dictionary<char, List<string>> PinYinWithToneDict
        {
            get
            {
                if (dictionary.Count == 0)
                {
                    List<ChineseCode> pyList = DictionaryHelper.GetAll();

                    foreach (ChineseCode code in pyList)
                    {
                        char hz = code.Word;
                        string py = code.Pinyins;
                        if (!string.IsNullOrEmpty(py))
                        {
                            dictionary.Add(hz, new List<string>(py.Split(';')));
                        }
                    }
                }
                return dictionary;
            }
        }

        /// <summary>
        ///     字的拼音，不包括音调
        /// </summary>
        public static Dictionary<char, IList<string>> PinYinDict
        {
            get
            {
                if (pyDictionary.Count == 0)
                {
                    List<ChineseCode> pyList = DictionaryHelper.GetAll();

                    foreach (ChineseCode code in pyList)
                    {
                        char hz = code.Word;
                        string pys = code.Pinyins;
                        if (!string.IsNullOrEmpty(pys))
                        {
                            foreach (string s in pys.Split(','))
                            {
                                string py = s.Remove(s.Length - 1); //remove tone
                                if (pyDictionary.ContainsKey(hz))
                                {
                                    if (!pyDictionary[hz].Contains(py))
                                    {
                                        pyDictionary[hz].Add(py);
                                    }
                                }
                                else
                                {
                                    pyDictionary.Add(hz, new List<string> { py });
                                }
                            }
                        }
                    }
                }
                return pyDictionary;
            }
        }

        #endregion

        /// <summary>
        ///     获得一个字的默认拼音(不包含音调)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string GetDefaultPinyin(char c)
        {
            try
            {
                IList<string> pys = PinYinDict[c];
                if (pys != null && pys.Count > 0)
                {
                    return pys[0];
                }
                throw new Exception("找不到字：“" + c + "”的拼音");
            }
            catch
            {
                throw new Exception("找不到字：“" + c + "”的拼音");
            }
        }

        public static IList<string> GetDefaultPinyin(string word)
        {
            var result = new List<string>();
            foreach (char c in word)
            {
                result.Add(GetDefaultPinyin(c));
            }
            return result;
        }

        /// <summary>
        ///     获得单个字的拼音,不包括声调
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IList<string> GetPinYinOfChar(char str)
        {
            return PinYinDict[str];
        }

        /// <summary>
        ///     判断一个字是否多音字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsMultiPinyinWord(char c)
        {
            return GetPinYinOfChar(c).Count > 1;
        }

        /// <summary>
        ///     获得单个字的拼音,包括声调
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<string> GetPinYinWithToneOfChar(char str)
        {
            return PinYinWithToneDict[str];
        }

        /// <summary>
        ///     如果给出一个字和一个没有音调的拼音，返回正确的带音调的拼音
        /// </summary>
        /// <param name="str"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        public static string AddToneToPinyin(char str, string py)
        {
            if (!PinYinWithToneDict.ContainsKey(str))
            {
                Debug.WriteLine("找不到" + str + "的拼音,使用其默认拼音对应的音调1");
                return py + "1";
            }
            List<string> list = PinYinWithToneDict[str];
            foreach (string allpinyin in list)
            {
                foreach (string pinyin in allpinyin.Split(','))
                {
                    if (
                        pinyin == py + "0"
                        || pinyin == py + "1"
                        || pinyin == py + "2"
                        || pinyin == py + "3"
                        || pinyin == py + "4"
                        || pinyin == py + "5"
                    )
                    {
                        return pinyin;
                    }
                }
            }
            Debug.WriteLine("找不到" + str + "的拼音" + py + "对应的音调");
            return py + "1"; //找不到音调就用拼音的一声
        }

        /// <summary>
        ///     判断给出的词和拼音是否有效
        /// </summary>
        /// <param name="word"></param>
        /// <param name="pinyin"></param>
        /// <returns></returns>
        public static bool ValidatePinyin(string word, List<string> pinyin)
        {
            List<string> pinyinList = pinyin;
            if (word.Length != pinyinList.Count)
            {
                return false;
            }
            for (int i = 0; i < word.Length; i++)
            {
                IList<string> charPinyinList = GetPinYinOfChar(word[i]);
                if (!charPinyinList.Contains(pinyinList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        ///// <summary>
        ///// 获得一个词中的每个字的音
        ///// </summary>
        ///// <param name="str">一个词</param>
        ///// <returns></returns>
        //public static List<List<string>> GetPinYinOfStringEveryChar(string str)
        //{
        //    var pyList = new List<List<string>>();
        //    for (int i = 0; i < str.Length; i++)
        //    {
        //        pyList.Add(GetPinYinOfChar(str[i]));
        //    }
        //    return pyList;
        //}
    }
}
