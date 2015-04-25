using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    ///     判断一个词是否过滤或保留
    /// </summary>
    public interface ISingleFilter
    {
        bool IsKeep(WordLibrary wl);
    }
}