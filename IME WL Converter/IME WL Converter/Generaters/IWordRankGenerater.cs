using System;
using System.Collections.Generic;
using System.Text;

namespace Studyzy.IMEWLConverter.Generaters
{
    public interface IWordRankGenerater
    {
        int GetRank(string word);
    }
}
