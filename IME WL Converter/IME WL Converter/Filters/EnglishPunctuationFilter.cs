using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 过滤包含英文标点的词
    /// </summary>
    internal class EnglishPunctuationFilter : ISingleFilter
    {
        #region ISingleFilter Members
        private static Regex regex = new Regex("[-,~.?:;'\""+@"!`\^]|(-{2})|(/.{3})|(/(/))|(/[/])|({})");
        public bool IsKeep(WordLibrary wl)
        {
            return !regex.IsMatch(wl.Word);
        }

        #endregion
    }
}