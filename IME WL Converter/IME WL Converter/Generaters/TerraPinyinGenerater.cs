using System.Collections.Generic;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    ///     地球拼音输入法，就是带音调的拼音输入法
    /// </summary>
    public class TerraPinyinGenerater : PinyinGenerater
    {
        private static readonly Regex regex = new Regex(@"^[a-zA-Z]+\d$");

        public override bool IsBaseOnOldCode
        {
            get { return true; }
        }

        public override IList<string> GetAllCodesOfChar(char str)
        {
            return PinyinHelper.GetPinYinWithToneOfChar(str);
        }

        public override IList<string> GetCodeOfString(string str, string charCodeSplit = "",
            BuildType buildType = BuildType.None)
        {
            IList<string> py = base.GetCodeOfString(str, charCodeSplit);
            var result = new List<string>();
            for (int i = 0; i < str.Length; i++)
            {
                string p = py[i];
                result.Add(PinyinHelper.AddToneToPinyin(str[i], p));
            }
            return result;
        }

        public override IList<string> GetCodeOfWordLibrary(WordLibrary wl, string charCodeSplit = "")
        {
            if (wl.CodeType == CodeType.Pinyin) //如果本来就是拼音输入法导入的，那么就用其拼音，不过得加上音调
            {
                IList<string> pinyin = new List<string>();
                for (int i = 0; i < wl.PinYin.Length; i++)
                {
                    if (regex.IsMatch(wl.PinYin[i]))
                    {
                        pinyin.Add(wl.PinYin[i]);
                    }
                    else
                    {
                        pinyin.Add(PinyinHelper.AddToneToPinyin(wl.Word[i], wl.Codes[i][0]));
                    }
                }
                return pinyin;
            }
            return base.GetCodeOfWordLibrary(wl, charCodeSplit);
        }
    }
}