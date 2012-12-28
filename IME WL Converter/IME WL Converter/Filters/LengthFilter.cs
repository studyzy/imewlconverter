using System;
using System.Collections.Generic;
using System.Text;

namespace Studyzy.IMEWLConverter.Filters
{
    public class LengthFilter:ISingleFilter
    {
        public LengthFilter()
        {
            MinLength = 1;
            MaxLength = 9999;
        }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool IsKeep(WordLibrary wl)
        {
            return (wl.Word.Length >= MinLength && wl.Word.Length <= MaxLength);
        }
    }
}
