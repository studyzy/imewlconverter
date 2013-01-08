using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Entities
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
            Factory = new PinyinGenerater();
        }

        public bool ContainPinyin { get; set; }
        public bool ContainCipin { get; set; }
        public string PinyinSplitString { get; set; }
        public string SplitString { get; set; }
        public BuildType PinyinSplitType { get; set; }
        public List<int> Sort { get; set; }
        public IWordCodeGenerater Factory { get; set; }

        public string BuildWLStringSample()
        {
            return BuildWLString(sample);
        }

        //没有什么思路，接下来的代码写得乱七八糟的，但是好像还是对的。zengyi20101114
        public string BuildWLString(WordLibrary wl)
        {
            string py = "", cp = "";
            var sb = new StringBuilder();
            if (ContainPinyin)
            {
                CodingString(wl, Factory);
                py = wl.GetPinYinString(PinyinSplitString, PinyinSplitType);
            }
            if (ContainCipin)
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
            if (index1 == 0 && ContainPinyin) //第一个是拼音
            {
                wl.PinYinString = strlist[0];
            }
            if (index1 == 1)
            {
                wl.Word = strlist[0];
            }
            if (index1 == 2 && ContainCipin)
            {
                wl.Count = Convert.ToInt32(strlist[0]);
            }
            if (strlist.Length > 1)
            {
                int index2 = Sort.FindIndex(i => i == newSort[1]); //中间的一个
                if (index2 == 0 && ContainPinyin) //第一个是拼音
                {
                    wl.PinYinString = strlist[1];
                }
                if (index2 == 1)
                {
                    wl.Word = strlist[1];
                }
                if (index2 == 2 && ContainCipin)
                {
                    wl.Count = Convert.ToInt32(strlist[1]);
                }
            }
            if (strlist.Length > 2)
            {
                int index2 = Sort.FindIndex(i => i == newSort[2]); //最大的一个
                if (index2 == 0 && ContainPinyin) //第一个是拼音
                {
                    wl.PinYinString = strlist[2];
                }
                if (index2 == 1)
                {
                    wl.Word = strlist[2];
                }
                if (index2 == 2 && ContainCipin)
                {
                    wl.Count = Convert.ToInt32(strlist[2]);
                }
            }

            wl.PinYin = wl.PinYinString.Split(new[] {PinyinSplitString}, StringSplitOptions.RemoveEmptyEntries);
            return wl;
        }

        public void CodingString(WordLibrary wl, IWordCodeGenerater factory)
        {
            var codes = new List<string>();
            foreach (char c in wl.Word)
            {
                string code = factory.GetDefaultCodeOfChar(c);
                codes.Add(code);
            }
            wl.PinYin = codes.ToArray();
        }
    }
}