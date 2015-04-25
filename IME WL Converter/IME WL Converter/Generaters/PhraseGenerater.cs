using System;
using System.Collections.Generic;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /*
     * 二字词：取每个字的前两位编码。例如“计算”取“JP”+“SQ”，即：“JPSQ”。
　　三字词：取第一字的前二位编码和最后两个字的第一码。例如“计算机”取“JPSJ”。
　　四字词：取每个字的第一码。例如“兴高采烈”取“XGCL”。
　　多字词（四字以上词）：取前三字和最后一字的第一码（前三末一）。
     */

    public class PhraseGenerater : IWordCodeGenerater
    {
        public bool IsBaseOnOldCode
        {
            get { return true; }
        }


        public bool Is1Char1Code
        {
            get { return false; }
        }

        public string GetDefaultCodeOfChar(char str)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetCodeOfString(string str, string charCodeSplit = "", BuildType buildType = BuildType.None)
        {
            throw new NotImplementedException("please call GetCodeOfWordLibrary method");
        }

        public IList<string> GetAllCodesOfChar(char str)
        {
            throw new NotImplementedException();
        }


        public bool Is1CharMutiCode
        {
            get { return true; }
        }


        public IList<string> GetCodeOfWordLibrary(WordLibrary str, string charCodeSplit = "")
        {
            if (str.CodeType == CodeType.Pinyin)
            {
                return new List<string> {str.GetPinYinString("", BuildType.None)};
            }
            return CollectionHelper.Descartes(str.Codes);
        }
    }
}