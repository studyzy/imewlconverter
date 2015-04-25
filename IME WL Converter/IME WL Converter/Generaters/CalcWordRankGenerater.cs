using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class CalcWordRankGenerater : IWordRankGenerater
    {
        public int GetRank(string word)
        {
            double x = 1;
            foreach (char c in word)
            {
                double freq = DictionaryHelper.GetCode(c).Freq;
                x += freq;
            }
            return (int) x;
        }
    }
}