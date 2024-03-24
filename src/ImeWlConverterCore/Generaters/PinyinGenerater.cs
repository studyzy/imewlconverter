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
    public class PinyinGenerater : BaseCodeGenerater, IWordCodeGenerater
    {
        private static Dictionary<string, List<string>> mutiPinYinWord;

        #region IWordCodeGenerater Members



        public override void GetCodeOfWordLibrary(WordLibrary wl)
        {
            if (wl.CodeType == CodeType.Pinyin)
            {
                return;
            }
            if (wl.CodeType == CodeType.TerraPinyin) //要去掉音调
            {
                for (int i = 0; i < wl.Codes.Count; i++)
                {
                    var row = wl.Codes[i];
                    for (int j = 0; j < row.Count; j++)
                    {
                        string s = row[j];
                        string py = s.Remove(s.Length - 1); //remove tone
                        wl.Codes[i][j] = py;
                    }
                }
                return;
            }
            //不是拼音，就调用GetCode生成拼音
            var code = GetCodeOfString(wl.Word);
            wl.Codes = code;
            wl.CodeType = CodeType.Pinyin;
        }

        /// <summary>
        ///     获得一个词的拼音
        ///     如果这个词不包含多音字，那么直接使用其拼音
        ///     如果包含多音字，则找对应的注音词，根据注音词进行注音
        ///     没有找到注音词的，使用默认拼音
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public override Code GetCodeOfString(string str)
        {
            if (IsInWordPinYin(str))
            {
                List<string> pyList = GenerateMutiWordPinYin(str);
                for (int i = 0; i < str.Length; i++)
                {
                    if (pyList[i] == null)
                    {
                        pyList[i] = PinyinHelper.GetDefaultPinyin(str[i]);
                    }
                }
                return new Code(pyList, true);
            }
            try
            {
                return new Code(PinyinHelper.GetDefaultPinyin(str), true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public virtual IList<string> GetAllCodesOfChar(char str)
        {
            return PinyinHelper.GetPinYinOfChar(str);
        }

        /// <summary>
        ///     因为使用了注音的方式，所以避免了多音字，一个词也只有一个音
        /// </summary>
        public virtual bool Is1CharMutiCode
        {
            get { return false; }
        }

        public virtual bool Is1Char1Code
        {
            get { return true; }
        }

        #endregion

        private void InitMutiPinYinWord()
        {
            if (mutiPinYinWord == null)
            {
                var wlList = new Dictionary<string, List<string>>();
                string[] lines = GetMutiPinyin()
                    .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    string py = line.Split(' ')[0];
                    string word = line.Split(' ')[1];

                    var pinyin = new List<string>(
                        py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries)
                    );
                    wlList.Add(word, pinyin);
                }
                mutiPinYinWord = wlList;
            }
        }

        private string GetMutiPinyin()
        {
            return DictionaryHelper.GetResourceContent("WordPinyin.txt");
        }

        /// <summary>
        ///     一个词中是否有多音字注音
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool IsInWordPinYin(string word)
        {
            InitMutiPinYinWord();
            foreach (string key in mutiPinYinWord.Keys)
            {
                if (word.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     产生一个词中多音字的拼音,没有的就空着
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private List<string> GenerateMutiWordPinYin(string word)
        {
            InitMutiPinYinWord();
            var pinyin = new string[word.Length];
            foreach (string key in mutiPinYinWord.Keys)
            {
                if (word.Contains(key))
                {
                    int index = word.IndexOf(key);
                    for (int i = 0; i < mutiPinYinWord[key].Count; i++)
                    {
                        pinyin[index + i] = mutiPinYinWord[key][i];
                    }
                }
            }
            return new List<string>(pinyin);
        }
    }
}
