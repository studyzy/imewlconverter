using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public class RankPercentageFilter : IBatchFilter
    {
        public int Percentage { get; set; }

        public WordLibraryList Filter(WordLibraryList list)
        {
            if (Percentage == 100)
            {
                return list;
            }
            int count = list.Count*Percentage/100;
            list.Sort((a, b) => a.Rank - b.Rank);
            var result = new WordLibraryList();
            for (int i = 0; i < count; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }
    }
}