using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters
{
    public interface IReplaceFilter
    {
        void Replace(WordLibrary wl);
    }
}
