using System;
using System.Collections.Generic;
using System.Diagnostics;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class SelfDefiningCodeGenerater : IWordCodeGenerater
    {
    public SelfDefiningCodeGenerater()
    {

    }
        private static PinyinGenerater pinyinGenerater=new PinyinGenerater();
        #region IWordCodeGenerater Members
        /// <summary>
        /// 外部的编码表,如果为空，则表示使用拼音编码
        /// </summary>
        public IDictionary<char, string> MappingDictionary { get; set; }
        /// <summary>
        /// 对于多个字的编码的设定
        /// 形如：
        /// code_e2=p11+p12+p21+p22
        /// code_e3=p11+p21+p31+p32
        /// code_a4=p11+p21+p31+n11
        /// </summary>
        public string MutiWordCodeFormat { get; set; }
        /// <summary>
        /// 自定义编码时不允许一字多码
        /// </summary>
        public bool Is1CharMutiCode
        {
            get { return false; }
        }
        /// <summary>
        /// 如果是拼音格式，那么就是一字一码，如果不是，那么就是一词一码
        /// </summary>
        public bool Is1Char1Code { get; set; }
        /// <summary>
        /// 有可能是拼音编码，所以是True
        /// </summary>
        public bool IsBaseOnOldCode { get { return true; } }
        public string GetDefaultCodeOfChar(char str)
        {
            if (MappingDictionary != null && MappingDictionary.Count > 0)

            {
                return MappingDictionary[str];
            }
            //没有指定Mapping表，那么就按拼音处理
            return pinyinGenerater.GetDefaultCodeOfChar(str);
            
        }
        private bool IsPinyinCode
        {
            get { return MappingDictionary == null || MappingDictionary.Count == 0; }
        }

        public IList<string> GetCodeOfWordLibrary(WordLibrary wl, string charCodeSplit = "")
        {
            if (wl.CodeType == CodeType.Pinyin && IsPinyinCode)
            {
                return CollectionHelper.DescarteIndex1(wl.Codes);
            }

            return GetCodeOfString(wl.Word, charCodeSplit);
        }
     


        public IList<string> GetCodeOfString(string str, string charCodeSplit = "")
        {

            if (IsPinyinCode)
            {
                return pinyinGenerater.GetCodeOfString(str, charCodeSplit);
            }
            var list = new List<string>();

            if (Is1Char1Code || str.Length == 1)

            {
                foreach (char c in str)
                {
                    list.Add(GetDefaultCodeOfChar(c));
                }
            }
            else //多个字一个编码
            {
                var result = "";
                var arr = MutiWordCodeFormat.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, string> format = new Dictionary<string, string>();

                foreach (var line in arr)
                {
                    var kv = line.Split('=');
                    var key = kv[0].Substring(5);
                    var value = kv[1];

                    format.Add(key, value);
                }
                var k = "e" + str.Length;
                if (format.ContainsKey(k)) //找到对应编码
                {

                    var f = format[k];
                    result = GetStringCode(str, f);
                }
                else //字符串很长

                {
                    var key = "";
                    for (var i = str.Length; i > 0; i--)
                    {

                        key = "a" + i;

                        if (format.ContainsKey(key))
                        {
                            break;
                        }
                    }
                    var f = format[key];
                    result = GetStringCode(str, f);
                }
                list.Add(result);

            }
            return list;
        }


        private string GetStringCode(string str, string format)

        {
            var result = "";
            var flist = format.Split('+');
            foreach (var s in flist)
            {

                var pn = s[0]; //可能P也可能N，p表示左取，n表示右取
                var pindex = Convert.ToInt32(s[1].ToString());
                char c = ' ';
                if (pn == 'p')
                    c = str[pindex - 1];

                else if (pn == 'n')
                    c = str[str.Length - pindex];
                var pcode = GetDefaultCodeOfChar(c);
                var cindex = Convert.ToInt32(s[2].ToString());
                if (pcode.Length >= cindex)

                    result += pcode[cindex - 1];

                else
                {
                    Debug.WriteLine(str + " 编码生成错误");
                }
            }
            return result;
        }

        public IList<string> GetCodeOfChar(char str)
        {
            throw new System.NotImplementedException("Don't allow one char muti codes");
        }



    

        #endregion
      

    }
}