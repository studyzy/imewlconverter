using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 过滤包含英文的词
    /// </summary>
    public class EnglishFilter:ISingleFilter
    {
        private readonly Regex englishRegex = new Regex("[a-zA-Z]", RegexOptions.IgnoreCase);
        public bool IsKeep(WordLibrary wl)
        {
            return !englishRegex.IsMatch(wl.Word);
        }
    }
}
