using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public class RankFilter : ISingleFilter
    {
        public RankFilter()
        {
            MinLength = 1;
            MaxLength = 999999;
        }

        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        public bool IsKeep(WordLibrary wl)
        {
            return (wl.Rank >= MinLength && wl.Rank <= MaxLength);
        }
    }
}