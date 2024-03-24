/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     紫光Uwl格式
    /// </summary>
    [ComboBoxShow(ConstantString.ZIGUANG_PINYIN_UWL, ConstantString.ZIGUANG_PINYIN_UWL_C, 171)]
    public class ZiGuangPinyinUwl : BaseImport, IWordLibraryImport
    {
        #region IWordLibraryImport Members

        public override bool IsText
        {
            get { return false; }
        }

        #endregion

        //{0x05 2word

        //4字节使用同一个拼音的词条数x，2字节拼音长度n，n字节拼音的编号，（2字节汉字的长度y，y*2字节汉字的内容Unicode编码，2字节词频，2字节未知，4字节未知）*x

        public WordLibraryList Import(string path)
        {
            var pyAndWord = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = 0x02;
            int enc = fs.ReadByte();
            Encoding encoding = (enc == 0x09) ? Encoding.Unicode : Encoding.GetEncoding("GB18030");

            fs.Position = 0x44;
            CountWord = BinFileHelper.ReadInt32(fs);
            int segmentCount = BinFileHelper.ReadInt32(fs); //分为几段
            CurrentStatus = 0;
            for (int i = 0; i < segmentCount; i++)
            {
                try
                {
                    fs.Position = 0xC00 + 1024 * i;
                    var segment = new Segment(fs, encoding);
                    pyAndWord.AddWordLibraryList(segment.WordLibraryList);
                    CurrentStatus += segment.WordLibraryList.Count;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return pyAndWord;
        }

        public WordLibraryList ImportLine(string line)
        {
            throw new NotImplementedException("华宇紫光uwl文件为二进制文件，不支持");
        }
    }

    public class Segment
    {
        #region Init

        //private readonly Dictionary<int, string> Shengmu = new Dictionary<int, string>()
        //    {
        //        {0, ""},
        //        {1, "b"},
        //        {2, "c"},
        //        {3, "ch"},
        //        {4, "d"},
        //        {5, "f"},
        //        {6, "g"},
        //        {7, "h"},
        //        {8, "j"},
        //        {9, "k"},
        //        {10, "l"},
        //        {11, "m"},
        //        {12, "n"},
        //        {13, "p"},
        //        {14, "q"},
        //        {15, "r"},
        //        {16, "s"},
        //        {17, "sh"},
        //        {18, "t"},
        //        {19, "w"},
        //        {20, "x"},
        //        {21, "y"},
        //        {22, "z"},
        //        {23, "zh"},

        //    };

        //private readonly Dictionary<int, string> Yunmu = new Dictionary<int, string>
        //    {
        //        {0, "ang"},
        //        {1, "a"},
        //        {2, "ai"},
        //        {3, "an"},
        //        {4, "ang"},
        //        {5, "ao"},
        //        {6, "e"},
        //        {7, "ei"},
        //        {8, "en"},
        //        {9, "eng"},
        //        {10, "er"},
        //        {11, "i"},
        //        {12, "ia"},
        //        {13, "ian"},
        //        {14, "iang"},
        //        {15, "iao"},
        //        {16, "ie"},
        //        {17, "in"},
        //        {18, "ing"},
        //        {19, "iong"},
        //        {20, "iu"},
        //        {21, "o"},
        //        {22, "ong"},
        //        {23, "ou"},
        //        {24, "u"},
        //        {25, "ua"},
        //        {26, "uai"},
        //        {27, "uan"},
        //        {28, "uang"},
        //        {29, "ue"},
        //        {30, "ui"},
        //        {31, "un"},
        //        {32, "uo"},
        //        {33, "v"},
        //    };
        private readonly IList<string> Shengmu = new List<string>
        {
            "",
            "b",
            "c",
            "ch",
            "d",
            "f",
            "g",
            "h",
            "j",
            "k",
            "l",
            "m",
            "n",
            "p",
            "q",
            "r",
            "s",
            "sh",
            "t",
            "w",
            "x",
            "y",
            "z",
            "zh",
        };

        private readonly IList<string> Yunmu = new List<string>
        {
            "ang",
            "a",
            "ai",
            "an",
            "ang",
            "ao",
            "e",
            "ei",
            "en",
            "eng",
            "er",
            "i",
            "ia",
            "ian",
            "iang",
            "iao",
            "ie",
            "in",
            "ing",
            "iong",
            "iu",
            "o",
            "ong",
            "ou",
            "u",
            "ua",
            "uai",
            "uan",
            "uang",
            "ue",
            "ui",
            "un",
            "uo",
            "v",
        };

        #endregion

        //private readonly IList<byte> LenDic = new List<byte>
        //{0x05, 0x87, 0x09, 0x8B, 0x0D, 0x8F, 0x11, 0x13, 0x15, 0x17, 0x19};

        public Segment(Stream stream, Encoding encoding)
        {
            UwlEncoding = encoding;
            IndexNumber = BinFileHelper.ReadInt32(stream);
            int ff = BinFileHelper.ReadInt32(stream);
            WordLenEnums = BinFileHelper.ReadInt32(stream);
            WordByteLen = BinFileHelper.ReadInt32(stream);

            WordLibraryList = new WordLibraryList();
            int lenB = 0;
            long startP = stream.Position;
            do
            {
                int l;
                WordLibrary wl = Parse(stream, out l);
                lenB += l;
                if (wl != null)
                {
                    WordLibraryList.Add(wl);
                }
            } while (lenB < WordByteLen);
        }

        public int IndexNumber { get; set; }

        //FF
        public int WordLenEnums { get; set; }
        public int WordByteLen { get; set; }
        public Encoding UwlEncoding { get; set; }

        public WordLibraryList WordLibraryList { get; set; }

        private WordLibrary Parse(Stream stream, out int lenByte)
        {
            if (stream.Position == 0x6664)
            {
                Debug.WriteLine("Debug");
            }
            var wl = new WordLibrary();
            int lenWord = stream.ReadByte();
            int lenCode = stream.ReadByte();
            lenCode = lenCode % 0x10 * 2 + lenWord / 0x80;
            lenWord = lenWord % 0x80 - 1;
            lenByte = 4 + lenWord + lenCode * 2;

            wl.Rank += stream.ReadByte();
            wl.Rank += stream.ReadByte() << 8;

            //py
            //int pyLen = Math.Min(8, len); //拼音最大支持8个字的拼音
            var wlPinYin = new string[lenCode];
            for (int i = 0; i < lenCode; i++)
            {
                int smB = stream.ReadByte();
                int ymB = stream.ReadByte();
                int smIndex = smB & 31;
                //var ymPre = smB & 224;
                int ymIndex = (smB >> 5) + (ymB << 3);
                //拼音编码对应的拼音
                //wl.PinYin[i] = smIndex + "~" + ymIndex;
                try
                {
                    wlPinYin[i] = Shengmu[smIndex] + Yunmu[ymIndex];
                }
                catch
                {
#if DEBUG
                    wlPinYin[i] = smIndex + "~" + ymIndex;
                    Debug.WriteLine(stream.Position);
#else
                    return null;
#endif
                }
            }
            wl.PinYin = wlPinYin;
            //hz
            var hzB = new byte[lenWord];
            stream.Read(hzB, 0, lenWord);
            wl.Word = UwlEncoding.GetString(hzB);

            return wl;
        }
    }
}
