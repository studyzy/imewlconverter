using System;
using System.Collections.Generic;
using System.Text;

using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Entities
{
    public class ParsePattern
    {
        private readonly WordLibrary sample;

        public ParsePattern()
        {
            Sort = new List<int> { 1, 2, 3 };
            sample = new WordLibrary();
            sample.Count = 1234;
            sample.Word = "深蓝词库转换";
            sample.PinYin = new[] { "shen", "lan", "ci", "ku", "zhuan", "huan" };
            IsPinyinFormat = true;
        }
        //private IWordCodeGenerater pyFactory = new PinyinGenerater();
        private SelfDefiningCodeGenerater selfFactory = new SelfDefiningCodeGenerater();
        private bool isPinyinFormat = true;

        /// <summary>
        /// 是否是拼音模式的一个字一个音
        /// </summary>
        public bool IsPinyinFormat
        {
            get { return isPinyinFormat; }
            set
            {
                isPinyinFormat = value;
            }
        }

        private bool isPinyin = false;
        /// <summary>
        /// 是否是拼音编码
        /// </summary>
        public bool IsPinyin
        {
            get { return isPinyin; }
            set { isPinyin = value; }
        }

        /// <summary>
        /// 对于多个字的编码的设定
        /// </summary>
        public string MutiWordCodeFormat { get; set; }
        public bool ContainCode { get; set; }
        public bool ContainRank { get; set; }
        public string CodeSplitString { get; set; }
        public string SplitString { get; set; }
        public BuildType CodeSplitType { get; set; }
        public List<int> Sort { get; set; }
        /// <summary>
        /// 每个字对应的编码的文件路径
        /// </summary>
        public string MappingTablePath { get; set; }

        public string BuildWLStringSample()
        {
       
           var dic = new Dictionary<char, string>()
                {
                    {'深', "shen"},
                    {'蓝', "lan"},
                    {'词', "ci"},
                    {'库', "ku"},
                    {'转', "zhuan"},
                    {'换', "huan"}
                };
        
            string word = "";
            string result = "";

            foreach (var c in sample.Word)
            {
                word += c;
             
                result += BuildWLString(dic,1234,word) + "\r\n";
            }

            return result;
        }
        /// <summary>
        /// 传入一个字与码的集合，以及词频，根据用户设定的格式，生成一条词条字符串
        /// </summary>
        /// <param name="charCodes"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public string BuildWLString(IDictionary<char, string> charCodes, int rank,string word="")
        {
            string code = "";
           if(word=="")
           {
               foreach (var c in charCodes.Keys)
            {
                word += c;
            }
           }
            if (ContainCode)
            {
                if (IsPinyinFormat)
                {
                    code = CollectionHelper.GetString(charCodes.Values, CodeSplitString, CodeSplitType);
                }
                else//多字一码，根据用户设置的编码规则，生成编码
                {
                    selfFactory.MutiWordCodeFormat = MutiWordCodeFormat;
                    selfFactory.MappingDictionary = charCodes;
                    
                    code = selfFactory.GetCodeOfString(word)[0];
                }
            }
            return BuildWLString(word, code, rank);

        }

        public string BuildWLString(string word,string code,int rank)
        {
            string  cp = "";
            if (!ContainCode)
            {
                code = "";
            }
            var sb = new StringBuilder();
            if (ContainRank)
            {
                cp = rank.ToString();
            }
            var dic = new Dictionary<int, string>();
            dic.Add(Sort[0], code);
            dic.Add(Sort[1], word);
            dic.Add(Sort[2], cp);
            var newSort = new List<int>(Sort);
            newSort.Sort();
            foreach (int x in newSort)
            {
                if (dic[x] != "")
                {
                    sb.Append(dic[x] + SplitString);
                }
            }
            string str = sb.ToString();
            return str.Substring(0, str.LastIndexOf(SplitString));
        }
        ////没有什么思路，接下来的代码写得乱七八糟的，但是好像还是对的。zengyi20101114
        ////如果wl中提供了拼音数组，而且自定义格式也是拼音格式，那么就只转换格式即可。
        //public string BuildWLString(WordLibrary wl)
        //{
        //    string py = "", cp = "";
        //    var sb = new StringBuilder();
        //    if (ContainCode)
        //    {
        //        if (IsPinyinFormat)
        //        {
        //            py = wl.GetPinYinString(CodeSplitString, CodeSplitType);
        //        }
        //        else
        //        {
        //            selfFactory.MutiWordCodeFormat = MutiWordCodeFormat;
        //            py = selfFactory.GetCodeOfString(wl.Word)[0];
        //        }
        //    }
        //    if (ContainRank)
        //    {
        //        cp = wl.Count.ToString();
        //    }
        //    var dic = new Dictionary<int, string>();
        //    dic.Add(Sort[0], py);
        //    dic.Add(Sort[1], wl.Word);
        //    dic.Add(Sort[2], cp);
        //    var newSort = new List<int>(Sort);
        //    newSort.Sort();
        //    foreach (int x in newSort)
        //    {
        //        if (dic[x] != "")
        //        {
        //            sb.Append(dic[x] + SplitString);
        //        }
        //    }
        //    string str = sb.ToString();
        //    return str.Substring(0, str.LastIndexOf(SplitString));
        //}

        public WordLibrary BuildWordLibrary(string line)
        {
            var wl = new WordLibrary();
            string[] strlist = line.Split(new[] { SplitString }, StringSplitOptions.RemoveEmptyEntries);
            var newSort = new List<int>(Sort);
            newSort.Sort();
            if (isPinyin)
            {
                int index1 = Sort.FindIndex(i => i == newSort[0]); //最小的一个
                if (index1 == 0 && ContainCode) //第一个是拼音
                {
                    wl.PinYinString = strlist[0];
                }
                if (index1 == 1)
                {
                    wl.Word = strlist[0];
                }
                if (index1 == 2 && ContainRank)
                {
                    wl.Count = Convert.ToInt32(strlist[0]);
                }
                if (strlist.Length > 1)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[1]); //中间的一个
                    if (index2 == 0 && ContainCode) //第一个是拼音
                    {
                        wl.PinYinString = strlist[1];
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[1];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[1]);
                    }
                }
                if (strlist.Length > 2)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[2]); //最大的一个
                    if (index2 == 0 && ContainCode) //第一个是拼音
                    {
                        wl.PinYinString = strlist[2];
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[2];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[2]);
                    }
                }

                wl.PinYin = wl.PinYinString.Split(new[] { CodeSplitString }, StringSplitOptions.RemoveEmptyEntries);
            
            }
            else//不是拼音，那么就抛弃直接加入Unknown Code。
            {
                int index1 = Sort.FindIndex(i => i == newSort[0]); //最小的一个
                if (index1 == 0 && ContainCode) //第一个是Code
                {
                    wl.SetCode(CodeType.Unknown, strlist[0]);
                }
                if (index1 == 1)
                {
                    wl.Word = strlist[0];
                }
                if (index1 == 2 && ContainRank)
                {
                    wl.Count = Convert.ToInt32(strlist[0]);
                }
                if (strlist.Length > 1)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[1]); //中间的一个
                    if (index2 == 0 && ContainCode) //第一个是Code
                    {
                        wl.SetCode(CodeType.Unknown, strlist[1]);
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[1];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[1]);
                    }
                }
                if (strlist.Length > 2)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[2]); //最大的一个
                    if (index2 == 0 && ContainCode) //第一个是拼音
                    {
                        wl.SetCode(CodeType.Unknown, strlist[2]);
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[2];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[2]);
                    }
                }

            }
            return wl;
        }

        //public void CodingString(WordLibrary wl, IWordCodeGenerater factory)
        //{
        //    var codes = new List<string>();
        //    foreach (char c in wl.Word)
        //    {
        //        string code = factory.GetDefaultCodeOfChar(c);
        //        codes.Add(code);
        //    }
        //    wl.PinYin = codes.ToArray();
        //}
    }
}