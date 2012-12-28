using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class ZhuyinHelper
    {
        private static Regex regex = new Regex(@"^[a-zA-Z]+\d$");
        public static string GetZhuyin(string pinyin)
        {
          
            var yindiao = 10;
            if (regex.IsMatch(pinyin)) //包含音调
            {
                yindiao = Convert.ToInt32(pinyin[pinyin.Length - 1].ToString());
                pinyin = pinyin.Substring(0, pinyin.Length - 1);
            }
            
            if (!ZhuyinDic.ContainsKey(pinyin))
            {
                Debug.WriteLine("Can not find zhuyin by pinyin=" + pinyin);
                return null;
            }
            var zy = ZhuyinDic[pinyin] + GetYindiaoZhuyin(yindiao);
            Debug.WriteLine("Pinyin:" + pinyin+",Zhuyin:" + zy);
            return zy;
        }

        public static IList<string> GetZhuyin(IList<string> pinyins )
        {
            var result = new List<string>();
            foreach (var code in pinyins)
            {
                result.Add(GetZhuyin(code));
            }
            return result;
        }

        private static string GetYindiaoZhuyin(int yindiao)
        {
            switch (yindiao)
            {
                case 1:
                    return "";
                case 2:
                    return "ˊ";
                case 3:
                    return "ˇ";
                case 4:
                    return "ˋ";
                case 5:
                    return "·";
                default:
                    return "";
            }

        }

        private static IDictionary<string, string> ZhuyinDic
        {
            get
            {
                if (zhuyinDic == null)
                {
                    zhuyinDic = new Dictionary<string, string>();
                    foreach (
                        var line in
                            Dictionaries.Zhuyin.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var arr = line.Split('\t');

                        var zhuyinCode = arr[0];
                        var pinyin = arr[1];

                        if (!zhuyinDic.ContainsKey(pinyin))
                        {
                            zhuyinDic.Add(pinyin, zhuyinCode);
                        }
                        else
                        {
                            Debug.WriteLine(pinyin + " mapping more than 1 zhuyin");
                        }
                    }

                }
                return zhuyinDic;
            }
        }

        private static IDictionary<string, string> zhuyinDic = null;
    }
}