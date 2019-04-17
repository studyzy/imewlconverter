using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class GoogleWordRankGenerater : IWordRankGenerater
    {
        private static string API = "https://www.google.com/search?q={0}";
        private static readonly Regex regex = new Regex("estimatedResultCount: \"(\\d+)\"");
        public bool ForceUse { get; set; }
        public int GetRank(string word)
        {
            try
            {
                string result = HttpHelper.GetHtml(string.Format(API, word));
                if (regex.IsMatch(result))
                {
                    string num = regex.Match(result).Groups[1].Value;
                    return Convert.ToInt32(num);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}