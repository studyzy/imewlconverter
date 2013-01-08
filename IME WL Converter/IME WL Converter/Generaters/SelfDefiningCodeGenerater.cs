using System;
using System.Collections.Generic;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    /// 根据指定的Mapping表生成Code
    /// </summary>
    public class SelfDefiningCodeGenerater : IWordCodeGenerater
    {
        #region IWordCodeGenerater Members
        /// <summary>
        /// 自定义编码必须是1字1码
        /// </summary>
        public bool Is1Char1Code
        {
            get { return true; }
        }

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
            throw new NotImplementedException();
        }


        public bool Is1CharMutiCode
        {
            get { return false; }
        }

        #endregion
    }
}