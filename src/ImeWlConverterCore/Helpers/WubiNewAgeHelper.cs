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
using System.Text.RegularExpressions;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class WubiNewAgeHelper
    {
        private static readonly Regex regex = new Regex(@"^[a-zA-Z]+\d$");
        private static IDictionary<string, string> wubiDic;
        private static IDictionary<string, string> wordWubiDic;

        /// <summary>
        /// 以五笔编码为Key，字或者词为Value的字典
        /// </summary>
        private static IDictionary<string, string> WubiWordDic
        {
            get
            {
                if (wubiDic == null)
                {
                    wubiDic = new Dictionary<string, string>();
                    foreach (
                        string line in Helpers
                            .DictionaryHelper.GetResourceContent("WubiNewAge.txt")
                            .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    )
                    {
                        string[] arr = line.Split('\t');

                        string word = arr[0];
                        string wubiCode = arr[1];

                        if (!wubiDic.ContainsKey(wubiCode))
                        {
                            wubiDic.Add(wubiCode, word);
                        }
                        else
                        {
                            Debug.WriteLine(wubiCode + " mapping more than 1 word");
                        }
                    }
                }
                return wubiDic;
            }
        }

        /// <summary>
        /// 以汉字或者词语为Key，对应的五笔编码为Value的字典
        /// </summary>
        private static IDictionary<string, string> WordWubiDic
        {
            get
            {
                if (wordWubiDic == null)
                {
                    wordWubiDic = new Dictionary<string, string>();
                    foreach (
                        string line in Helpers
                            .DictionaryHelper.GetResourceContent("WubiNewAge.txt")
                            .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    )
                    {
                        string[] arr = line.Split('\t');

                        string word = arr[0];
                        string wubiCode = arr[1];

                        if (!wordWubiDic.ContainsKey(word))
                        {
                            wordWubiDic.Add(word, wubiCode);
                        }
                        else
                        {
                            Debug.WriteLine(word + " mapping more than 1 code");
                        }
                    }
                }
                return wordWubiDic;
            }
        }

        /// <summary>
        /// 根据五笔编码获得对应的字或词语
        /// </summary>
        /// <param name="wubiCode"></param>
        /// <returns></returns>
        public static string GetWord(string wubiCode)
        {
            if (!WubiWordDic.ContainsKey(wubiCode))
            {
                Debug.WriteLine("Can not find word by wubi code=" + wubiCode);
                return null;
            }

            return WubiWordDic[wubiCode];
        }

        /// <summary>
        /// 根据字获得对应的五笔编码
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string GetWubiCode(string word)
        {
            if (WordWubiDic.ContainsKey(word))
            {
                return WordWubiDic[word];
            }
            Debug.WriteLine("can not fine the wubi of word:" + word);
            return null;
        }
    }
}
