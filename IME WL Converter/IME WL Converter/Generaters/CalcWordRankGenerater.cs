using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class CalcWordRankGenerater: IWordRankGenerater
    {
        public CalcWordRankGenerater()
        {
   
        }
   
       public int GetRank(string word)
       {
           double x = 1;
           foreach (var c in word)
           {
               var freq = DictionaryHelper.GetCode(c).Freq;
               x += freq;
           }
           return (int)x;

       }
    }
}
