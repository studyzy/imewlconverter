using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class BaiduWordRankGenerater: IWordRankGenerater
    {
        private static string API = "http://www.baidu.com/s?wd={0}";
        private static Regex regex = new Regex("百度为您找到相关结果约([0-9\\,]+)个");
       public int GetRank(string word)
       {
           try
           {
               var result = HttpHelper.GetHtml(string.Format(API, word));
                if(regex.IsMatch(result))
                {
                    var num = regex.Match(result).Groups[1].Value;
                    return Convert.ToInt32(num.Replace(",",""));
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
