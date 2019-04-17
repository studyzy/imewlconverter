using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public interface IReplaceFilter
    {
        void Replace(WordLibrary wl);
    }
}