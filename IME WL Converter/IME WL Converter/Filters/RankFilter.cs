using System;
using System.Collections.Generic;
using System.Text;

namespace Studyzy.IMEWLConverter.Filters
{
    public class RankFilter:ISingleFilter
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
            return (wl.Count >= MinLength && wl.Count <= MaxLength);
        }
    }
}
