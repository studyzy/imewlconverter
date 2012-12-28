using System.Collections.Generic;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class SelfDefiningCodeGenerater : IWordCodeGenerater
    {
        #region IWordCodeGenerater Members
        public bool Is1Char1Code { get { return true; } }
        public string GetDefaultCodeOfChar(char str)
        {
            string s = UserCodingHelper.GetCharCoding(str);
            return s;
        }

        public IList<string> GetCodeOfString(string str, string charCodeSplit = "")
        {
            var list = new List<string>();
            foreach (char c in str)
            {
                list.Add(GetDefaultCodeOfChar(c));
            }
            return list;
        }

        public IList<string> GetCodeOfChar(char str)
        {
            throw new System.NotImplementedException();
        }

      

        public bool Is1CharMutiCode { get { return false; } }

        #endregion
    }
}