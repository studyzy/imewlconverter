using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    ///     过滤包含英文标点的词
    /// </summary>
    public class EnglishPunctuationFilter : ISingleFilter, IReplaceFilter
    {
        #region ISingleFilter Members

        private static readonly Regex regex = new Regex("[-,~.?:;'\"" + @"!`\^]|(-{2})|(/.{3})|(/(/))|(/[/])|({})");

        public void Replace(WordLibrary wl)
        {
            wl.Word = regex.Replace(wl.Word, "");
        }

        public bool IsKeep(WordLibrary wl)
        {
            return !regex.IsMatch(wl.Word);
        }

        #endregion
    }
}