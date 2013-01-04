using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 紫光Uwl格式
    /// </summary>
    [ComboBoxShow(ConstantString.ZIGUANG_PINYIN_UWL, ConstantString.ZIGUANG_PINYIN_UWL_C, 171)]
    public class ZiGuangPinyinUwl : BaseImport, IWordLibraryImport
    {

        //{0x05 2word

        //4字节使用同一个拼音的词条数x，2字节拼音长度n，n字节拼音的编号，（2字节汉字的长度y，y*2字节汉字的内容Unicode编码，2字节词频，2字节未知，4字节未知）*x

        #region IWordLibraryImport Members

        public WordLibraryList Import(string path)
        {
            var pyAndWord = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = 0x44;
            CountWord = BinFileHelper.ReadInt32(fs);
            var segmentCount = BinFileHelper.ReadInt32(fs); //分为几段
            CurrentStatus = 0;
            for (int i = 0; i < segmentCount; i++)
            {
                try
                {
                    fs.Position = 0xC00 + 1024 * i;
                    Segment segment = new Segment(fs);
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


        public override bool IsText
        {
            get { return false; }
        }

        public WordLibraryList ImportLine(string line)
        {
            throw new NotImplementedException("搜狗Bin文件为二进制文件，不支持");
        }

        #endregion
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
        private readonly IList<string> Shengmu = new List<string>()
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

        private readonly IList<string> Yunmu = new  List<string>
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

        public Segment(Stream stream)
        {
            IndexNumber = BinFileHelper.ReadInt32(stream);
            var ff = BinFileHelper.ReadInt32(stream);
            WordLenEnums = BinFileHelper.ReadInt32(stream);
            WordByteLen = BinFileHelper.ReadInt32(stream);

            WordLibraryList = new WordLibraryList();
            int lenB = 0;
            long startP = stream.Position;
            do
            {
                int l;
                var wl = Parse(stream, out l);
                lenB += l;
                if(wl!=null)
                {
                    WordLibraryList.Add(wl);
                }
            } while (lenB<WordByteLen );

        }

        private IList<byte> LenDic = new List<byte>() {0x05, 0x87, 0x09, 0x8B, 0x0D, 0x8F, 0x11,0x13,0x15,0x17,0x19};
        public int IndexNumber { get; set; }
        //FF
        public int WordLenEnums { get; set; }
        public int WordByteLen { get; set; }

        public WordLibraryList WordLibraryList { get; set; }

        private WordLibrary Parse(Stream stream, out int lenByte)
        {
            if (stream.Position == 0x6664)
            {
                Debug.WriteLine("Debug");
            }
            WordLibrary wl = new WordLibrary();
            int lenCode = stream.ReadByte();
            var len = LenDic.IndexOf((byte) lenCode) + 2;
               lenByte = len*4 + 4;
            byte[] rankB = new byte[4];
            for (int i = 0; i < 3; i++)
            {
                var b = (byte) stream.ReadByte();
                rankB[i] = b;
            }
            wl.Count = (BitConverter.ToInt32(rankB, 0) - 1)/32;
            //py
            var pyLen = Math.Min(8, len);//拼音最大支持8个字的拼音
            wl.PinYin = new string[pyLen];
            for (int i = 0; i < pyLen; i++)
            {
                var smB = stream.ReadByte();
                var ymB = stream.ReadByte();
                var smIndex = smB & 31;
                //var ymPre = smB & 224;
                var ymIndex = (smB >> 5) + (ymB << 3);
                //拼音编码对应的拼音
                //wl.PinYin[i] = smIndex + "~" + ymIndex;
                try
                {
                    wl.PinYin[i] = Shengmu[smIndex] + Yunmu[ymIndex];
                }
                catch
                {
#if DEBUG
                    wl.PinYin[i] = smIndex + "~" + ymIndex;
                    Debug.WriteLine(stream.Position);
#else
                    return null;
#endif
                }
            }
            //hz
            byte[] hzB = new byte[len*2];
            stream.Read(hzB, 0, len*2);
            wl.Word = Encoding.Unicode.GetString(hzB);
         
            return wl;
        }
    }
}