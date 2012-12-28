using System;
using System.Collections.Generic;
using System.Text;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 过滤包含空格的词
    /// </summary>
    class SpaceFilter:ISingleFilter
    {
        public bool IsKeep(WordLibrary wl)
        {
            return wl.Word.IndexOf(' ') < 0;
        }
    }
}
