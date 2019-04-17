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


        public override IList<string> GetAllCodesOfChar(char str)
        {
            return PinyinHelper.GetPinYinWithToneOfChar(str);
        }

        public override Code GetCodeOfString(string str)
        {
            var py = base.GetCodeOfString(str);
            var result = new List<string>();
            for (int i = 0; i < str.Length; i++)
            {
                var prow = py[i];
                foreach (var p in prow)
                {
                    result.Add(PinyinHelper.AddToneToPinyin(str[i], p));
                }
              
            }
            return new Code( result,true);
        }

        public override void GetCodeOfWordLibrary(WordLibrary wl)
        {
            if (wl.CodeType == CodeType.TerraPinyin)
            {
                return;
            }
            if (wl.CodeType == CodeType.Pinyin) //如果本来就是拼音输入法导入的，那么就用其拼音，不过得加上音调
            {

                for (int i = 0; i < wl.Codes.Count; i++)
                {
                    var row = wl.Codes[i];
                    for (int j = 0; j < row.Count; j++)
                    {
                        string s = row[j];
                        string py =PinyinHelper.AddToneToPinyin(wl.Word[i], s); //add tone
                        wl.Codes[i][j] = py;
                    }
                }

               
                return ;
            }
            base.GetCodeOfWordLibrary(wl);
        }
    }
}