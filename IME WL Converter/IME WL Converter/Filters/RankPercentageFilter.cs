using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    class RankPercentageFilter:IBatchFilter
    {
        public int Percentage { get; set; }
        public WordLibraryList Filter(WordLibraryList list)
        {
            if (Percentage == 100)
            {
                return list;
            }
            int count = list.Count*Percentage/100;
            list.Sort((a,b)=>a.Count-b.Count);
            WordLibraryList result=new WordLibraryList();
            for (var i = 0; i < count; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }
    }
}
