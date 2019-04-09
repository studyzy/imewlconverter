using System.Collections.Generic;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class Wubi86Generater : IWordCodeGenerater
    {
        #region IWordCodeGenerater Members

        public virtual string GetDefaultCodeOfChar(char str)
        {
            return DictionaryHelper.GetCode(str).Wubi86;
        }

        public Code GetCodeOfString(string str)
        {
            return new Code(GetStringWubiCode(str));
        }

        public virtual IList<string> GetAllCodesOfChar(char str)
        {
            return new List<string> {DictionaryHelper.GetCode(str).Wubi86};
        }

        public bool Is1CharMutiCode
        {
            get { return false; }
        }

        public bool Is1Char1Code
        {
            get { return false; }
        }

        #endregion


        public virtual void GetCodeOfWordLibrary(WordLibrary wl)
        {
            wl.Codes = GetCodeOfString(wl.Word);
        }

        private string GetStringWubiCode(string str)
        {
            if (str.Length == 1)
            {
                return GetDefaultCodeOfChar(str[0]);
            }
            if (str.Length == 2)
            {
                string code1 = GetDefaultCodeOfChar(str[0]);
                string code2 = GetDefaultCodeOfChar(str[1]);
                return code1.Substring(0, 2) + code2.Substring(0, 2);
            }
            if (str.Length == 3)
            {
                string code1 = GetDefaultCodeOfChar(str[0]);
                string code2 = GetDefaultCodeOfChar(str[1]);
                string code3 = GetDefaultCodeOfChar(str[2]);
                return code1[0].ToString() + code2[0] + code3.Substring(0, 2);
            }
            else
            {
                string code1 = GetDefaultCodeOfChar(str[0]);
                string code2 = GetDefaultCodeOfChar(str[1]);
                string code3 = GetDefaultCodeOfChar(str[2]);
                string code4 = GetDefaultCodeOfChar(str[str.Length - 1]);
                return code1[0].ToString() + code2[0] + code3[0] + code4[0];
            }
        }
    }
}