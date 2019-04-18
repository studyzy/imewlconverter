using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class ZhuyinHelper
    {
        private static readonly Regex regex = new Regex(@"^[a-zA-Z]+\d$");
        private static IDictionary<string, string> zhuyinDic;
        private static IDictionary<string, string> pinyinDic;

        private static IDictionary<string, string> ZhuyinDic
        {
            get
            {
                if (zhuyinDic == null)
                {
                    zhuyinDic = new Dictionary<string, string>();
                    foreach (
                        string line in
                            Helpers.DictionaryHelper.GetResourceContent("Zhuyin.txt").Split(new[] {"\r","\n"}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] arr = line.Split('\t');

                        string zhuyinCode = arr[0];
                        string pinyin = arr[1];

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

        private static IDictionary<string, string> PinyinDic
        {
            get
            {
                if (pinyinDic == null)
                {
                    pinyinDic = new Dictionary<string, string>();
                    foreach (
                        string line in
                            Helpers.DictionaryHelper.GetResourceContent("Zhuyin.txt").Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] arr = line.Split('\t');

                        string zhuyinCode = arr[0];
                        string pinyin = arr[1];

                        if (!pinyinDic.ContainsKey(zhuyinCode))
                        {
                            pinyinDic.Add(zhuyinCode, pinyin);
                        }
                        else
                        {
                            Debug.WriteLine(pinyin + " mapping more than 1 pinyin");
                        }
                    }
                }
                return pinyinDic;
            }
        }

        public static string GetZhuyin(string pinyin)
        {
            if (string.IsNullOrEmpty(pinyin))
            {
                throw new Exception("找不到拼音");
            }
            int yindiao = 10;
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
            string zy = ZhuyinDic[pinyin] + GetYindiaoZhuyin(yindiao);
            Debug.WriteLine("Pinyin:" + pinyin + ",Zhuyin:" + zy);
            return zy;
        }

        public static IList<string> GetZhuyin(IList<string> pinyins)
        {
            var result = new List<string>();
            foreach (string code in pinyins)
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

        private static int GetYindiaoPinyin(char yindiao)
        {
            switch (yindiao)
            {
                case 'ˊ':
                    return 2;
                case 'ˇ':
                    return 3;
                case 'ˋ':
                    return 4;
                case '·':
                    return 5;
                default:
                    return 1;
            }
        }

        /// <summary>
        ///     根据注音获得不包含音调的拼音
        /// </summary>
        /// <param name="zhuyin"></param>
        /// <returns></returns>
        public static string GetPinyin(string zhuyin)
        {
            char lastChar = zhuyin[zhuyin.Length - 1];
            int yindiao = GetYindiaoPinyin(lastChar);
            if (yindiao != 1)
            {
                zhuyin = zhuyin.Substring(0, zhuyin.Length - 1);
            }
            if (PinyinDic.ContainsKey(zhuyin))
            {
                return PinyinDic[zhuyin];
            }
            Debug.WriteLine("can not fine the pinyin of zhuyin:" + zhuyin);
            return null;
        }
    }
}