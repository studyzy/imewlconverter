using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public interface IBatchFilter
    {
        WordLibraryList Filter(WordLibraryList list);
    }
}