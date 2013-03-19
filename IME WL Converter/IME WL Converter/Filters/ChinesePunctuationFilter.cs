using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 过滤包含中文标点的词
    /// </summary>
    internal class ChinesePunctuationFilter : ISingleFilter
    {
        #region ISingleFilter Members
        private static Regex regex = new Regex(@"[\u3002\uff1b\uff0c\uff1a\u201c\u201d\uff08\uff09\u3001\uff1f\u300a\u300b]");
        public bool IsKeep(WordLibrary wl)
        {
            return !regex.IsMatch(wl.Word);
        }

        #endregion
    }
}