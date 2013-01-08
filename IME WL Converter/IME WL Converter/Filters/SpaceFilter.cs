using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 过滤包含空格的词
    /// </summary>
    internal class SpaceFilter : ISingleFilter
    {
        #region ISingleFilter Members

        public bool IsKeep(WordLibrary wl)
        {
            return wl.Word.IndexOf(' ') < 0;
        }

        #endregion
    }
}