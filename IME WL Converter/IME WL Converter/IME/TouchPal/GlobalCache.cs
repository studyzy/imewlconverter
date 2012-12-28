using System;
using System.Collections.Generic;

namespace Studyzy.IMEWLConverter.IME.TouchPal
{
    internal class GlobalCache
    {
        public static Dictionary<int, TouchPalChar> CharList = new Dictionary<int, TouchPalChar>();
        public static Dictionary<int, TouchPalWord> WordList = new Dictionary<int, TouchPalWord>();
        public static Stack<TouchPalChar> Stackes = new Stack<TouchPalChar>();
        public static Stack<TouchPalChar> ExportStackes = new Stack<TouchPalChar>();
        private static readonly Dictionary<int, string> pinyinMapping = new Dictionary<int, string>();
        public static TouchPalChar JumpChar;

        private static readonly Dictionary<string, int> pinyinIndexMapping = new Dictionary<string, int>();

        public static Dictionary<int, string> PinyinMapping
        {
            get
            {
                if (pinyinMapping.Count == 0)
                {
                    string[] lines = Dictionaries.TouchPalPinyinDic.Split(new[] {"\r\n"},
                                                                          StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        string[] pycd = line.Split(',');
                        int id = Convert.ToInt32(pycd[0]);
                        string py = pycd[1];
                        pinyinMapping.Add(id, py);
                    }
                }
                return pinyinMapping;
            }
        }

        public static Dictionary<string, int> PinyinIndexMapping
        {
            get
            {
                if (pinyinIndexMapping.Count == 0)
                {
                    string[] lines = Dictionaries.TouchPalPinyinDic.Split(new[] {"\r\n"},
                                                                          StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        string[] pycd = line.Split(',');
                        int id = Convert.ToInt32(pycd[0]);
                        string py = pycd[1];
                        pinyinIndexMapping.Add(py, id);
                    }
                }
                return pinyinIndexMapping;
            }
        }
    }
}