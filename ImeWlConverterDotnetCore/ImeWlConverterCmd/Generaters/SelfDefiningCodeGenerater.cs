using System;
using System.Collections.Generic;
using System.Diagnostics;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    /// 根据提供的外部字典，格式，生成编码的类
    /// </summary>
    public class SelfDefiningCodeGenerater :BaseCodeGenerater, IWordCodeGenerater
    {
        #region IWordCodeGenerater Members

        /// <summary>
        ///     外部的编码表,如果为空，则表示使用拼音编码
        /// </summary>
        public IDictionary<char, IList<string>> MappingDictionary { get; set; }

        /// <summary>
        ///     对于多个字的编码的设定
        ///     形如：
        ///     code_e2=p11+p12+p21+p22
        ///     code_e3=p11+p21+p31+p32
        ///     code_a4=p11+p21+p31+n11
        /// </summary>
        public string MutiWordCodeFormat { get; set; }

        /// <summary>
        ///     自定义编码时允许一字多码
        /// </summary>
        public bool Is1CharMutiCode
        {
            get { return true; }
        }

        /// <summary>
        ///     如果是拼音格式，那么就是一字一码，如果不是，那么就是一词一码
        /// </summary>
        public bool Is1Char1Code { get; set; }

        public string GetDefaultCodeOfChar(char str)
        {
            if (MappingDictionary != null && MappingDictionary.Count > 0)

            {
                if (MappingDictionary.ContainsKey(str))
                {
                    return MappingDictionary[str][0];
                }
            }
            //没有指定Mapping表，那么就按拼音处理
            //return pinyinGenerater.GetDefaultCodeOfChar(str);
            return null;
        }



        /// <summary>
        ///     获得一个词的编码,如果MutiCode==True，那么返回的是一个词的多种编码方式
        /// </summary>
        /// <param name="str"></param>
        /// <param name="charCodeSplit"></param>
        /// <returns></returns>
        public override Code GetCodeOfString(string str)
        {
            IList<IList<string>> codes = GetAllCodesOfString(str);
            if (Is1Char1Code)
            {
                return new Code(codes);
            }
            //一词一码，按组词规则生成
            var list = new List<string>();
            string result = "";
            string[] arr = MutiWordCodeFormat.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            var format = new Dictionary<string, string>();

            foreach (string line in arr)
            {
                string[] kv = line.Split('=');
                string key = kv[0].Substring(5);
                string value = kv[1];

                format.Add(key, value);
            }
            string k = "e" + str.Length;
            if (format.ContainsKey(k)) //找到对应编码
            {
                string f = format[k];
                result = GetStringCode(str, f);
            }
            else //字符串很长

            {
                string key = "";
                for (int i = str.Length; i > 0; i--)
                {
                    key = "a" + i;

                    if (format.ContainsKey(key))
                    {
                        break;
                    }
                }
                string f = format[key];
                result = GetStringCode(str, f);
            }
            list.Add(result);
            return new Code(list,false);
        }


        public IList<string> GetAllCodesOfChar(char str)
        {
            return MappingDictionary[str];
        }

        private string GetStringCode(string str, string format)

        {
            string result = "";
            string[] flist = format.Split('+');
            foreach (string s in flist)
            {
                char pn = s[0]; //可能P也可能N，p表示左取，n表示右取
                int pindex = Convert.ToInt32(s[1].ToString());
                char c = ' ';
                if (pn == 'p')
                    c = str[pindex - 1];

                else if (pn == 'n')
                    c = str[str.Length - pindex];
                string pcode = GetDefaultCodeOfChar(c);
                int cindex = Convert.ToInt32(s[2].ToString());
                if (pcode.Length >= cindex)

                    result += pcode[cindex - 1];

                else
                {
                    Debug.WriteLine(str + " 编码生成错误");
                }
            }
            return result;
        }

        private IList<IList<string>> GetAllCodesOfString(string str)
        {
            var result = new List<IList<string>>();
            foreach (char c in str)
            {
                result.Add(GetAllCodesOfChar(c));
            }
            return result;
        }

        #endregion

    }
}