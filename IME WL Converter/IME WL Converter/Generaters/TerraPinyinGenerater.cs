using System.Collections.Generic;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    /// 地球拼音输入法，就是带音调的拼音输入法
    /// </summary>
    public class TerraPinyinGenerater : PinyinGenerater
    {
        public override IList<string> GetCodeOfChar(char str)
        {
            return PinyinHelper.GetPinYinWithToneOfChar(str);
        }

        public override IList<string> GetCodeOfString(string str, string charCodeSplit = "")
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
    }
}