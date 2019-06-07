using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public interface IReplaceFilter
    {
        bool ReplaceAfterCode { get; }
        void Replace(WordLibrary wl);
    }
}