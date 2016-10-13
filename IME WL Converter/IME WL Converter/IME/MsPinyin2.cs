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
    ///     搜狗细胞词库
    /// </summary>
    [ComboBoxShow(ConstantString.WIN10_MS_PINYIN, ConstantString.WIN10_MS_PINYIN_C, 130)]
    public class MsPinyin2 : BaseImport, IWordLibraryImport, IWordLibraryExport
    {
        #region IWordLibraryImport 成员

        //public bool OnlySinglePinyin { get; set; }

        public WordLibraryList Import(string path)
        {
          
            return ReadScel(path);
        }

        #endregion

        private Dictionary<int, string> pyDic = new Dictionary<int, string>();

        #region IWordLibraryImport Members

        public override bool IsText
        {
            get { return false; }
        }

        #endregion

        public WordLibraryList ImportLine(string line)
        {
            throw new Exception("Scel格式是二进制文件，不支持流转换");
        }

        private WordLibraryList ReadScel(string path)
        {
            pyDic = new Dictionary<int, string>();
            //Dictionary<string, string> pyAndWord = new Dictionary<string, string>();
            var pyAndWord = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var str = new byte[128];
            var outstr = new byte[128];
            byte[] num;
            //以下代码调试用的
            //fs.Position = 0x2628;
            //byte[] debug = new byte[50000];
            //fs.Read(debug, 0, 50000);
            //string txt = Encoding.Unicode.GetString(debug);

            //调试用代码结束

            int hzPosition = 0;
            fs.Read(str, 0, 128); //\x40\x15\x00\x00\x44\x43\x53\x01
            if (str[4] == 0x44)
            {
                hzPosition = 0x2628;
            }
            if (str[4] == 0x45)
            {
                hzPosition = 0x26C4;
            }

            fs.Position = 0x124;
            CountWord = BinFileHelper.ReadInt32(fs);
            CurrentStatus = 0;

            //fs.Position = 0x130;
            //fs.Read(str, 0, 64);
            //string txt = Encoding.Unicode.GetString(str);
            ////Console.WriteLine("字库名称:" + txt);
            //fs.Position = 0x338;
            //fs.Read(str, 0, 64);
            ////Console.WriteLine("字库类别:" + Encoding.Unicode.GetString(str));

            //fs.Position = 0x540;
            //fs.Read(str, 0, 64);
            ////Console.WriteLine("字库信息:" + Encoding.Unicode.GetString(str));

            //fs.Position = 0xd40;
            //fs.Read(str, 0, 64);
            ////Console.WriteLine("字库示例:" + Encoding.Unicode.GetString(str));

            fs.Position = 0x1540;
            str = new byte[4];
            fs.Read(str, 0, 4); //\x9D\x01\x00\x00
            while (true)
            {
                num = new byte[4];
                fs.Read(num, 0, 4);
                int mark = num[0] + num[1]*256;
                str = new byte[128];
                fs.Read(str, 0, (num[2]));
                string py = Encoding.Unicode.GetString(str);
                py = py.Substring(0, py.IndexOf('\0'));
                pyDic.Add(mark, py);
                if (py == "zuo") //最后一个拼音
                {
                    break;
                }
            }
            var s = new StringBuilder();
            foreach (string value in pyDic.Values)
            {
                s.Append(value + "\",\"");
            }
            Debug.WriteLine(s.ToString());


            //fs.Position = 0x2628;
            fs.Position = hzPosition;

            while (true)
            {
                try
                {
                    pyAndWord.AddRange(ReadAPinyinWord(fs));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                if (fs.Length == fs.Position) //判断文件结束
                {
                    fs.Close();
                    break;
                }
            }
            return pyAndWord;
            //var sb = new StringBuilder();
            //foreach (WordLibrary w in pyAndWord)
            //{
            //    sb.AppendLine("'" + w.PinYinString + " " + w.Word); //以搜狗文本词库的方式返回
            //}
            //return sb.ToString();
        }

        private IList<WordLibrary> ReadAPinyinWord(FileStream fs)
        {
            var num = new byte[4];
            fs.Read(num, 0, 4);
            int samePYcount = num[0] + num[1]*256;
            int count = num[2] + num[3]*256;
            //接下来读拼音
            var str = new byte[256];
            for (int i = 0; i < count; i++)
            {
                str[i] = (byte) fs.ReadByte();
            }
            var wordPY = new List<string>();
            for (int i = 0; i < count/2; i++)
            {
                int key = str[i*2] + str[i*2 + 1]*256;
                wordPY.Add(pyDic[key]);
            }
            //wordPY = wordPY.Remove(wordPY.Length - 1); //移除最后一个单引号
            //接下来读词语
            var pyAndWord = new List<WordLibrary>();
            for (int s = 0; s < samePYcount; s++) //同音词，使用前面相同的拼音
            {
                num = new byte[2];
                fs.Read(num, 0, 2);
                int hzBytecount = num[0] + num[1]*256;
                str = new byte[hzBytecount];
                fs.Read(str, 0, hzBytecount);
                string word = Encoding.Unicode.GetString(str);
                short unknown1 = BinFileHelper.ReadInt16(fs); //全部是10,肯定不是词频，具体是什么不知道
                int unknown2 = BinFileHelper.ReadInt32(fs); //每个字对应的数字不一样，不知道是不是词频
                pyAndWord.Add(new WordLibrary {Word = word, PinYin = wordPY.ToArray(), Rank = DefaultRank});
                CurrentStatus++;
                //接下来10个字节什么意思呢？暂时先忽略了
                var temp = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    temp[i] = (byte) fs.ReadByte();
                }
            }
            return pyAndWord;
        }

        public Encoding Encoding { get; }

        public string Export(WordLibraryList wlList)
        {
            string tempPath = "C:\\temp\\test.txt";

            var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(new byte[] {0x6D, 0x73, 0x63, 0x68, 0x78, 0x75, 0x64, 0x70, 0x1, 0, 0, 0, 0x40, 0, 0, 0});
            bw.Write(new byte[] {0x44, 0, 0, 0});
            int len = 100;
            bw.Write(BitConverter.GetBytes(len));
            bw.Write(BitConverter.GetBytes((long) wlList.Count));
            bw.Write(new byte[] {0xF, 0xBD, 0xFD, 0x57}); //好像是校驗
            for (var x = 0; x < 8; x++)
            {
                bw.Write(BitConverter.GetBytes(0));
            }
          
            foreach (var wl in wlList)
            {
                bw.Write(BitConverter.GetBytes((short)8));
                bw.Write(BitConverter.GetBytes((short)8));
                var py = wl.GetPinYinString("", BuildType.None);
                var offset = py.Length*2 + 10;
                bw.Write(BitConverter.GetBytes((short)offset));
                bw.Write(new byte[] { 1, 6 });//1是詞頻6不知道
                bw.Write(Encoding.Unicode.GetBytes(py));
                bw.Write(BitConverter.GetBytes((short)0));
                bw.Write(Encoding.Unicode.GetBytes(wl.Word));
                bw.Write(BitConverter.GetBytes((short)0));
            }
            fs.Position = 0x14;
            fs.Write(BitConverter.GetBytes(fs.Length),0,4);
           
            fs.Close();
            return tempPath;
        }

        public string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException();
        }
    }
}