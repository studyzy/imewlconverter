using System;
using System.Collections.Generic;

namespace Studyzy.IMEWLConverter.Helpers
{
    internal class DictionaryHelper
    {
        private static readonly Dictionary<char, ChineseCode> dictionary = new Dictionary<char, ChineseCode>();

        private static Dictionary<char, ChineseCode> Dict
        {
            get
            {
                if (dictionary.Count == 0)
                {
                    string allPinYin = Dictionaries.ChineseCode;
                    string[] pyList = allPinYin.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < pyList.Length; i++)
                    {
                        string[] hzpy = pyList[i].Split('\t');
                        char hz = Convert.ToChar(hzpy[1]);

                        dictionary.Add(hz, new ChineseCode
                            {
                                Code = hzpy[0],
                                Word = hzpy[1][0],
                                Wubi86 = hzpy[2],
                                Wubi98 = (hzpy[3] == "" ? hzpy[2] : hzpy[3]),
                                Pinyins = hzpy[4]
                            });
                    }
                }
                return dictionary;
            }
        }

        public static ChineseCode GetCode(char c)
        {
            return Dict[c];
        }

        public static List<ChineseCode> GetAll()
        {
            return new List<ChineseCode>(Dict.Values);
        }
    }

    internal struct ChineseCode
    {
        public string Code { get; set; }
        public char Word { get; set; }
        public string Wubi86 { get; set; }
        public string Wubi98 { get; set; }
        public string Pinyins { get; set; }
    }
}