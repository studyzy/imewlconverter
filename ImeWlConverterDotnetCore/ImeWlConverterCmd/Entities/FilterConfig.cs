namespace Studyzy.IMEWLConverter.Entities
{
    public class FilterConfig
    {
        public FilterConfig()
        {
            WordLengthFrom = 1;
            WordLengthTo = 100;
            WordRankFrom = 1;
            WordRankTo = 999999;
            WordRankPercentage = 100;
            IgnoreEnglish = true;
            IgnoreSpace = true;
            IgnorePunctuation = true;
            IgnoreNumber = true;
            NoFilter = false;
        }

        public bool NoFilter { get; set; }
        public int WordLengthFrom { get; set; }
        public int WordLengthTo { get; set; }

        public int WordRankFrom { get; set; }
        public int WordRankTo { get; set; }

        public int WordRankPercentage { get; set; }
        public bool IgnoreEnglish { get; set; }
        public bool IgnoreNumber { get; set; }
        public bool IgnoreSpace { get; set; }
        public bool IgnorePunctuation { get; set; }

        public bool ReplaceNumber { get; set; }
        public bool ReplaceEnglish { get; set; }
        public bool ReplaceSpace { get; set; }
        public bool ReplacePunctuation { get; set; }
    }
}