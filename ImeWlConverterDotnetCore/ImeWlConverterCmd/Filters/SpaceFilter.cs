using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    ///     过滤包含空格的词
    /// </summary>
    public class SpaceFilter : ISingleFilter, IReplaceFilter
    {
        #region ISingleFilter Members

        private readonly Regex regex = new Regex(@"\s");

        public bool IsKeep(WordLibrary wl)
        {
            return wl.Word.IndexOf(' ') < 0;
        }

        #endregion

        public void Replace(WordLibrary wl)
        {
            wl.Word = regex.Replace(wl.Word, "");
        }
    }
}