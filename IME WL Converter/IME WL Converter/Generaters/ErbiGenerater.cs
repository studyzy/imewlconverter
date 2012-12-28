using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /*
     * 二字词：取每个字的前两位编码。例如“计算”取“JP”+“SQ”，即：“JPSQ”。
　　三字词：取第一字的前二位编码和最后两个字的第一码。例如“计算机”取“JPSJ”。
　　四字词：取每个字的第一码。例如“兴高采烈”取“XGCL”。
　　多字词（四字以上词）：取前三字和最后一字的第一码（前三末一）。
     */

    public class ErbiGenerater : IWordCodeGenerater
    {
        public bool Is1Char1Code { get { return false; } }
        #region IWordCodeGenerater Members

        public string GetDefaultCodeOfChar(char str)
        {
            return ErbiDic[str][0];
        }

        public IList<string> GetCodeOfString(string str, string charCodeSplit = "")
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            foreach (var c in str)
            {
                if (!ErbiDic.ContainsKey(c))
                {
                    return new List<string>();
                }
            }
            
            if (str.Length == 1)
            {
                return ErbiDic[str[0]];
            }
            var codes = new List<IList<string>>();
            if (str.Length == 2) //二字词组 2+2
            {
                codes.Add(Get2Code(str[0]));
                codes.Add(Get2Code(str[1]));
            }
            else if (str.Length == 3) //三字词组 1+2+1
            {
                codes.Add(Get2Code(str[0]));
                codes.Add(Get1Code(str[1]));
                codes.Add(Get1Code(str[2]));
            }
            else
            {
                codes.Add(Get1Code(str[0]));
                codes.Add(Get1Code(str[1]));
                codes.Add(Get1Code(str[2]));
                codes.Add(Get1Code(str[str.Length-1]));
            }
            List<string> result = new List<string>();
            Descartes(codes, 0, result, string.Empty);
            return result;
        }
        private static string Descartes(IList<IList<string>> list, int count, IList<string> result, string data)
        {
            string temp = data;
            //获取当前数组
            var astr = list[count];
            //循环当前数组
            foreach (var item in astr)
            {
                if (count + 1 < list.Count)
                {
                    temp += Descartes(list, count + 1, result, data + item);
                }
                else
                {
                    result.Add(data + item);
                }
            }
            return temp;
        }
        private IList<string> Get2Code(char c)
        {
            var result = new List<string>();
            var codes = ErbiDic[c];
            foreach (var code in codes)
            {
                var c2 = code.Substring(0, Math.Min(code.Length, 2));
                if(!result.Contains(c2))
                    result.Add(c2);
            }
            return result;
        }

        private IList<string> Get1Code(char c)
        {
            var result = new List<string>();
            var codes = ErbiDic[c];
            foreach (var code in codes)
            {
                var c1 = code.Substring(0, 1);
                if (!result.Contains(c1))
                    result.Add(c1);
            }
            return result;
        }

        private Dictionary<char, IList<string>> erbiDic;

        private Dictionary<char, IList<string>> ErbiDic
        {
            get
            {
                if (erbiDic == null)
                {
                 
                    var txt = Dictionaries.Erbi;

                    erbiDic = new Dictionary<char, IList<string>>();
                    foreach (var line in txt.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var arr = line.Split('\t');
                        if (arr[0].Length == 0)
                        {
                            continue;
                        }
                        var word = arr[0][0];
                        var code = arr[1];
                        if (erbiDic.ContainsKey(word))
                        {
                            erbiDic[word].Add(code);
                        }
                        else
                        {
                            erbiDic.Add(word, new List<string>(){code});                            
                        }
                    }
                }
                return erbiDic;
            }
        }

      
        public IList<string> GetCodeOfChar(char str)
        {
            return ErbiDic[str];
        }

    

        public bool Is1CharMutiCode { get { return true; } }

        #endregion
    }
}