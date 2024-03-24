using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    ///     过滤首字符非中日韩的词（实际上非常粗糙）
    /// </summary>
    public class FirstCJKFilter : ISingleFilter
    {
        public bool ReplaceAfterCode => false;

        public bool IsKeep(WordLibrary wl)
        {
            char c = wl.Word[0];
            return c >= 0x2e80 && c <= 0x9fff;
        }
    }
}
