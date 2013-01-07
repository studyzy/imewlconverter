using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter
{
    public class ParsePattern
    {
        private readonly WordLibrary sample;

        public ParsePattern()
        {
            Sort = new List<int> {1, 2, 3};
            sample = new WordLibrary();
            sample.Count = 1234;
            sample.Word = "深蓝词库转换";
            sample.PinYin = new[] {"shen", "lan", "ci", "ku", "zhuan", "huan"};
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
        /// 每个字对应的编码
        /// </summary>
        public IDictionary<char, string> MappingTable { get; set; } 

        public string BuildWLStringSample()
        {
            var samFactory = new SelfDefiningCodeGenerater();
           
            samFactory.MappingDictionary = new Dictionary<char, string>()
                {
                    {'深', "shen"},
                    {'蓝', "lan"},
                    {'词', "ci"},
                    {'库', "ku"},
                    {'转', "zhuan"},
                    {'换', "huan"}
                };
            var temp = selfFactory;
            selfFactory = samFactory;
            string word = "";
            string result = "";
            List<string> codes = new List<string>();
            foreach (var c in sample.Word)
            {
                word += c;
                codes.Add(sample.PinYin[word.Length - 1]);
                var s = new WordLibrary();
                s.Count = 1234;
                s.Word = word;
                s.PinYin = codes.ToArray();
                result += BuildWLString(s) + "\r\n";
            }
            selfFactory = temp;
            return result;
        }

        //没有什么思路，接下来的代码写得乱七八糟的，但是好像还是对的。zengyi20101114
        //如果wl中提供了拼音数组，而且自定义格式也是拼音格式，那么就只转换格式即可。
        public string BuildWLString(WordLibrary wl)
        {
            string py = "", cp = "";
            var sb = new StringBuilder();
            if (ContainCode)
            {
                if (IsPinyinFormat)
                {
                    py = wl.GetPinYinString(CodeSplitString, CodeSplitType);
                }
                else
                {
                    selfFactory.MutiWordCodeFormat = MutiWordCodeFormat;
                    py = selfFactory.GetCodeOfString(wl.Word)[0];
                }
            }
            if (ContainRank)
            {
                cp = wl.Count.ToString();
            }
            var dic = new Dictionary<int, string>();
            dic.Add(Sort[0], py);
            dic.Add(Sort[1], wl.Word);
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

        public WordLibrary BuildWordLibrary(string line)
        {
            var wl = new WordLibrary();
            string[] strlist = line.Split(new[] {SplitString}, StringSplitOptions.RemoveEmptyEntries);
            var newSort = new List<int>(Sort);
            newSort.Sort();
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

            wl.PinYin = wl.PinYinString.Split(new[] {CodeSplitString}, StringSplitOptions.RemoveEmptyEntries);
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