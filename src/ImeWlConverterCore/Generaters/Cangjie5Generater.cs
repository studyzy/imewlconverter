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
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class Cangjie5Generater : BaseCodeGenerater, IWordCodeGenerater
    {
        #region IWordCodeGenerater Members

        private static readonly Dictionary<char, string> OneCodeChar = new Dictionary<char, string>
        {
            { '日', "a" },
            { '月', "b" },
            { '金', "c" },
            { '木', "d" },
            { '水', "e" },
            { '火', "f" },
            { '土', "g" },
            { '竹', "h" },
            { '戈', "i" },
            { '十', "j" },
            { '大', "k" },
            { '中', "l" },
            { '一', "m" },
            { '弓', "n" },
            { '人', "o" },
            { '心', "p" },
            { '手', "q" },
            { '口', "r" },
            { '尸', "s" },
            { '廿', "t" },
            { '山', "u" },
            { '女', "v" },
            { '田', "w" },
            { '卜', "y" },
            { '曰', "a" },
            { '八', "c" },
            { '儿', "c" },
            { '又', "e" },
            { '小', "f" },
            { '士', "g" },
            { '广', "i" },
            { '厂', "m" },
            { '工', "m" },
            { '乙', "n" },
            { '入', "o" },
            { '匕', "p" },
            { '七', "p" },
        };

        private Dictionary<char, IList<Cangjie>> dictionary;

        private Dictionary<char, IList<Cangjie>> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    string txt = Helpers.DictionaryHelper.GetResourceContent("Cangjie5.txt");
                    dictionary = new Dictionary<char, IList<Cangjie>>();

                    foreach (
                        string line in txt.Split(
                            new[] { '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries
                        )
                    )
                    {
                        string[] arr = line.Split('\t');
                        try
                        {
                            char word = arr[0][0];
                            var cj = new Cangjie();
                            cj.Code = arr[1];
                            if (arr.Length == 3)
                            {
                                cj.SplitCode = arr[2];
                            }
                            if (dictionary.ContainsKey(word))
                            {
                                dictionary[word].Add(cj);
                            }
                            else
                            {
                                dictionary.Add(word, new List<Cangjie> { cj });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(line + ex.Message);
                        }
                    }
                }
                return dictionary;
            }
        }

        public bool Is1Char1Code
        {
            get { return false; }
        }

        public override Code GetCodeOfString(string str)
        {
            foreach (char c in str)
            {
                if (!Dictionary.ContainsKey(c))
                {
                    return null;
                }
            }
            if (str.Length == 1)
            {
                var c = new List<string>();
                foreach (Cangjie cangjy in Dictionary[str[0]])
                {
                    c.Add(cangjy.Code);
                }

                return new Code(c, false);
            }
            IList<IList<string>> codes = new List<IList<string>>();
            var sb = new StringBuilder();
            if (str.Length == 2) //第一个字2码（首尾码），第二字3码（取首次尾码）
            {
                codes.Add(GetFirstAndLastCode(str[0]));
                codes.Add(GetFirstSecondLastCode(str[1]));
            }
            if (str.Length == 3) //221取码
            {
                codes.Add(GetFirstAndLastCode(str[0]));
                IList<string> code2 = GetFirstAndLastCode(str[1]);
                codes.Add(code2);
                if (code2[0].Length == 1) //212取码
                {
                    codes.Add(GetFirstAndLastCode(str[2]));
                }
                else
                {
                    codes.Add(GetLastCode(str[2]));
                }
            }
            if (str.Length == 4) //首2字当字首取2码，剩下的2字当字身3码（第3字2码，第4字尾码）。
            {
                codes.Add(GetFirstCode(str[0]));
                codes.Add(GetLastCode(str[1]));
                IList<string> code3 = GetFirstAndLastCode(str[2]);
                codes.Add(code3);
                if (code3[0].Length == 1)
                {
                    codes.Add(GetFirstAndLastCode(str[3]));
                }
                else
                {
                    codes.Add(GetLastCode(str[3]));
                }
            }
            if (str.Length >= 5) //首2字当字首取2码，剩下的3字当字身3码（第3字1码，第4字尾码  ,5字尾码）。
            {
                codes.Add(GetFirstCode(str[0]));
                codes.Add(GetLastCode(str[1]));
                codes.Add(GetFirstCode(str[2]));
                codes.Add(GetLastCode(str[str.Length - 2]));
                codes.Add(GetLastCode(str[str.Length - 1]));
            }

            IList<string> result = CollectionHelper.Descartes(codes);

            return new Code(result, false);
        }

        public IList<string> GetAllCodesOfChar(char str)
        {
            var result = new List<string>();
            foreach (Cangjie cangjie in Dictionary[str])
            {
                result.Add(cangjie.Code);
            }
            return result;
        }

        public bool Is1CharMutiCode
        {
            get { return true; }
        }

        private char GetSplitedCode(string code)
        {
            string[] arr = code.Split('\'');
            return arr[0][arr[0].Length - 1];
        }

        private IList<string> GetLastCode(char c)
        {
            if (OneCodeChar.ContainsKey(c))
            {
                return new List<string> { OneCodeChar[c] };
            }
            IList<Cangjie> x = Dictionary[c];
            var result = new List<string>();
            foreach (Cangjie cangjy in x)
            {
                if (cangjy.SplitCode != null && !IgnoreContainRule)
                {
                    string code = GetSplitedCode(cangjy.SplitCode).ToString();
                    if (!result.Contains(code))
                    {
                        result.Add(code);
                    }
                }
                else
                {
                    string code = cangjy.Code;
                    string lcode = code[code.Length - 1].ToString();
                    if (!result.Contains(lcode))
                    {
                        result.Add(lcode);
                    }
                }
            }
            return result;
        }

        private IList<string> GetFirstCode(char c)
        {
            if (OneCodeChar.ContainsKey(c))
            {
                return new List<string> { OneCodeChar[c] };
            }
            IList<Cangjie> x = Dictionary[c];
            var result = new List<string>();
            foreach (Cangjie cangjy in x)
            {
                string code = cangjy.Code[0].ToString();
                if (!result.Contains(code))
                {
                    result.Add(code);
                }
            }
            return result;
        }

        private IList<string> GetFirstAndLastCode(char c)
        {
            if (OneCodeChar.ContainsKey(c))
            {
                return new List<string> { OneCodeChar[c] };
            }
            IList<Cangjie> x = Dictionary[c];
            var result = new List<string>();
            foreach (Cangjie cangjy in x)
            {
                char firstCode = cangjy.Code[0];
                char lastCode = cangjy.Code[cangjy.Code.Length - 1];
                if (cangjy.SplitCode != null && !IgnoreContainRule)
                {
                    string[] arr = cangjy.SplitCode.Split('\'');
                    if (arr[0].Length > 1)
                    {
                        lastCode = GetSplitedCode(cangjy.SplitCode);
                    }
                }
                string code = firstCode.ToString() + lastCode;
                if (!result.Contains(code))
                {
                    result.Add(code);
                }
            }
            return result;
        }

        private IList<string> GetFirstSecondLastCode(char c)
        {
            if (OneCodeChar.ContainsKey(c))
            {
                return new List<string> { OneCodeChar[c] };
            }
            IList<Cangjie> x = Dictionary[c];
            var result = new List<string>();
            foreach (Cangjie cangjy in x)
            {
                try
                {
                    if (cangjy.Code.Length == 1)
                    {
                        result.Add(cangjy.Code);
                        continue;
                    }
                    string code = cangjy.Code[0].ToString() + cangjy.Code[1];
                    if (cangjy.Code.Length > 2)
                    {
                        char lastCode = cangjy.Code[cangjy.Code.Length - 1];
                        if (cangjy.SplitCode != null && !IgnoreContainRule)
                        {
                            string[] arr = cangjy.SplitCode.Split('\'');

                            if (arr[0].Length > 2)
                            {
                                lastCode = arr[0][arr[0].Length - 1];
                            }
                        }
                        code += lastCode;
                    }
                    if (!result.Contains(code))
                        result.Add(code);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            return result;
        }

        #endregion

        public bool IgnoreContainRule { get; set; }

        public struct Cangjie
        {
            public string Code { get; set; }
            public string SplitCode { get; set; }
        }
    }
}
