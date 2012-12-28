using System;
using System.Collections.Generic;
using System.Text;

namespace Studyzy.IMEWLConverter.Filters
{
    public interface ISingleFilter
    {
        bool IsKeep(WordLibrary wl);
    }
}
