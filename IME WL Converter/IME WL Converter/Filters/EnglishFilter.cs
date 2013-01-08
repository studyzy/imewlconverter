using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 过滤包含英文的词
    /// </summary>
    public class EnglishFilter : ISingleFilter
    {
        private readonly Regex englishRegex = new Regex("[a-zA-Z]", RegexOptions.IgnoreCase);

        public bool IsKeep(WordLibrary wl)
        {
            return !englishRegex.IsMatch(wl.Word);
        }
    }
}