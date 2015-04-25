using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class ZhengmaGenerater : IWordCodeGenerater
    {
        private Dictionary<char, Zhengma> zhengmaDic;

        private Dictionary<char, Zhengma> ZhengmaDic
        {
            get
            {
                if (zhengmaDic == null)
                {
                    string txt = Dictionaries.Zhengma;

                    zhengmaDic = new Dictionary<char, Zhengma>();
                    foreach (string line in txt.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] arr = line.Split('\t');
                        if (arr[0].Length == 0)
                        {
                            continue;
                        }
                        char word = arr[0][0];
                        string shortCode = arr[1].Trim();
                        var codes = new List<string>();
                        for (int i = 1; i < arr.Length; i++)
                        {
                            string code = arr[i].Trim();
                            if (code != "")
                            {
                                codes.Add(code);
                            }
                        }
                        zhengmaDic.Add(word, new Zhengma {ShortCode = shortCode, Code = codes});
                    }
                }
                return zhengmaDic;
            }
        }

        #region IWordCodeGenerater Members

        public string GetDefaultCodeOfChar(char str)
        {
            return ZhengmaDic[str].Code[0];
        }

        public IList<string> GetCodeOfString(string str, string charCodeSplit = "", BuildType buildType = BuildType.None)
        {
            foreach (char c in str)
            {
                if (!ZhengmaDic.ContainsKey(c))
                {
                    return new List<string>();
                }
            }

            if (str.Length == 1)
            {
                return ZhengmaDic[str[0]].Code;
            }
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
            var result = new List<string>();
            result.Add(codes.ToString());
            return result;
        }

        public IList<string> GetAllCodesOfChar(char str)
        {
            return ZhengmaDic[str].Code;
        }


        public bool Is1CharMutiCode
        {
            get { return false; }
        }

        public bool Is1Char1Code
        {
            get { return false; }
        }

        #endregion

        public bool IsBaseOnOldCode
        {
            get { return false; }
        }

        public IList<string> GetCodeOfWordLibrary(WordLibrary str, string charCodeSplit = "")
        {
            return GetCodeOfString(str.Word, charCodeSplit);
        }

        #region Nested type: Zhengma

        private struct Zhengma
        {
            /// <summary>
            ///     构词嘛
            /// </summary>
            public string ShortCode { get; set; }

            /// <summary>
            ///     单字郑码
            /// </summary>
            public IList<string> Code { get; set; }
        }

        #endregion

        private string Get2Code(char c)
        {
            Zhengma codes = ZhengmaDic[c];
            return codes.ShortCode;
        }

        private string Get1Code(char c)
        {
            return ZhengmaDic[c].ShortCode[0].ToString();
        }
    }
}