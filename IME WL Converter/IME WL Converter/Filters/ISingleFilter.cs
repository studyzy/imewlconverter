using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public interface ISingleFilter
    {
        bool IsKeep(WordLibrary wl);
    }
}