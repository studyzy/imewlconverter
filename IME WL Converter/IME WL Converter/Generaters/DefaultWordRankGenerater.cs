using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class DefaultWordRankGenerater: IWordRankGenerater
    {
        public DefaultWordRankGenerater()
        {
            Rank = 1;
        }
        public int Rank { get; set; }
       public int GetRank(string word)
       {
           return Rank;
       }
    }
}
