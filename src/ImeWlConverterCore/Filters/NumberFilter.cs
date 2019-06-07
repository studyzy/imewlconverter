using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    ///     过滤包含英文的词
    /// </summary>
    public class NumberFilter : ISingleFilter, IReplaceFilter
    {
        public bool ReplaceAfterCode => false;
        private readonly Regex regex = new Regex(@"\d");

        public void Replace(WordLibrary wl)
        {
            wl.Word = regex.Replace(wl.Word, "");
        }

        public bool IsKeep(WordLibrary wl)
        {
            return !regex.IsMatch(wl.Word);
        }
    }
}