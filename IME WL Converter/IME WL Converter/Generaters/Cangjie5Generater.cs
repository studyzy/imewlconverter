using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class Cangjie5Generater : IWordCodeGenerater
    {
        #region IWordCodeGenerater Members
        public bool Is1Char1Code { get { return false; } }
        public string GetDefaultCodeOfChar(char str)
        {
            return Dictionary[str][0].Code;
        }

        private static Dictionary<char, string> OneCodeChar = new Dictionary<char, string>()
            {
                {'日', "a"},
                {'月', "b"},
                {'金', "c"},
                {'木', "d"},
                {'水', "e"},
                {'火', "f"},
                {'土', "g"},
                {'竹', "h"},
                {'戈', "i"},
                {'十', "j"},
                {'大', "k"},
                {'中', "l"},
                {'一', "m"},
                {'弓', "n"},
                {'人', "o"},
                {'心', "p"},
                {'手', "q"},
                {'口', "r"},
                {'尸', "s"},
                {'廿', "t"},
                {'山', "u"},
                {'女', "v"},
                {'田', "w"},
                {'卜', "y"},
                {'曰',"a"},
                {'八',"c"},
                {'儿',"c"},
                {'又',"e"},
                {'小',"f"},
                {'士',"g"},
                {'广',"i"},
                {'厂',"m"},
                {'工',"m"},
                {'乙',"n"},
                {'入', "o"},
                {'匕',"p"},
                {'七',"p"},
           
            };
        /// <summary>
        /// 一个字有多种编码，所以一个词也会多种编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
         public IList<string> GetCodeOfString(string str, string charCodeSplit = "")

        {
            foreach (var c in str)
            {
                if (!Dictionary.ContainsKey(c))
                {
                    return null;
                }
            }
            if (str.Length == 1)
            {
                var c = new List<string>();
                foreach (var cangjy in Dictionary[str[0]])
                {
                    c.Add(cangjy.Code);
                }

                return c;
            }
            IList<IList<string>> codes = new List<IList<string>>();
            StringBuilder sb = new StringBuilder();
            if (str.Length == 2) //第一个字2码（首尾码），第二字3码（取首次尾码）
            {
                codes.Add(GetFirstAndLastCode(str[0]));
                codes.Add(GetFirstSecondLastCode(str[1]));
            }
            if (str.Length == 3) //221取码
            {
                codes.Add(GetFirstAndLastCode(str[0]));
                var code2 = GetFirstAndLastCode(str[1]);
                codes.Add(code2);
                if (code2[0].Length == 1)//212取码
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
                var code3 = GetFirstAndLastCode(str[2]);
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

            var result = CollectionHelper.Descartes(codes);

            return result;
        }

        public IList<string> GetCodeOfChar(char str)
        {
            var result = new List<string>();
            foreach (var cangjie in Dictionary[str])
            {
                result.Add(cangjie.Code);
            }
            return result;
        }


        public bool Is1CharMutiCode { get { return true; } }

        private char GetSplitedCode(string code)
        {
            var arr = code.Split('\'');
            return arr[0][arr[0].Length - 1];
        }

       

        private IList<string> GetLastCode(char c)
        {
            if (OneCodeChar.ContainsKey(c))
            {
                return new List<string>() { OneCodeChar[c] };
            }
            var x = Dictionary[c];
            var result = new List<string>();
            foreach (var cangjy in x)
            {
                if (cangjy.SplitCode != null && !IgnoreContainRule)
                {
                    var code = GetSplitedCode(cangjy.SplitCode).ToString();
                    if (!result.Contains(code))
                    {
                        result.Add(code);
                    }
                }
                else
                {
                    var code = cangjy.Code;
                    var lcode = code[code.Length - 1].ToString();
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
                return new List<string>() { OneCodeChar[c] };
            }
            var x = Dictionary[c];
            var result = new List<string>();
            foreach (var cangjy in x)
            {
                var code = cangjy.Code[0].ToString();
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
                return new List<string>() { OneCodeChar[c] };
            }
            var x = Dictionary[c];
            var result = new List<string>();
            foreach (var cangjy in x)
            {
                var firstCode = cangjy.Code[0];
                var lastCode = cangjy.Code[cangjy.Code.Length - 1];
                if (cangjy.SplitCode != null && !IgnoreContainRule)
                {
                    var arr = cangjy.SplitCode.Split('\'');
                    if (arr[0].Length > 1)
                    {
                        lastCode = GetSplitedCode(cangjy.SplitCode);
                    }
                }
                var code = firstCode.ToString() + lastCode;
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
                return new List<string>() { OneCodeChar[c] };
            }
            var x = Dictionary[c];
            var result = new List<string>();
            foreach (var cangjy in x)
            {
                string code = cangjy.Code[0].ToString() + cangjy.Code[1];
                if (cangjy.Code.Length > 2)
                {
                    var lastCode = cangjy.Code[cangjy.Code.Length - 1];
                    if (cangjy.SplitCode != null && !IgnoreContainRule)
                    {
                        var arr = cangjy.SplitCode.Split('\'');

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
            return result;
        }

        private Dictionary<char, IList<Cangjie>> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    var txt = Dictionaries.Cangjie5;
                    dictionary = new Dictionary<char, IList<Cangjie>>();

                    foreach (var line in txt.Split(new char[]{'\r', '\n'},StringSplitOptions.RemoveEmptyEntries))
                    {
                        var arr = line.Split('\t');
                        try
                        {
                            char word = arr[0][0];
                            Cangjie cj = new Cangjie();
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
                                dictionary.Add(word, new List<Cangjie>() { cj });
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


        private Dictionary<char, IList<Cangjie>> dictionary;

        #endregion
        public bool IgnoreContainRule { get; set; }
        public struct Cangjie
        {
            public string Code { get; set; }
            public string SplitCode { get; set; }
        }
    }
}