using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Win10微软拼音
    /// </summary>
    [ComboBoxShow(ConstantString.WIN10_MS_PINYIN, ConstantString.WIN10_MS_PINYIN_C, 130)]
    public class Win10MsPinyin : IWordLibraryExport //BaseImport, IWordLibraryImport, 
    {
        /*
        mschxudp file format

#           proto8                   version     phrase_offset_start
# 00000000  6d 73 63 68 78 75 64 70  01 00 00 00 40 00 00 00  |mschxudp....@...|
#          phrase_start phrase_end   phrase_count
# 00000010  48 00 00 00 7e 00 00 00  02 00 00 00 00 00 00 00  |H...~...........|
#           timestamp
# 00000020  29 b8 cc 58 00 00 00 00  00 00 00 00 00 00 00 00  |)..X............|
# 00000030  00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00  |................|
#                                                      candidate2
#           phrase_offsets[]         magic       hanzi_offset2
# 00000040  00 00 00 00 1c 00 00 00  08 00 08 00 10 00 01 06  |................|
#           pinyin                   phrase
# 00000050  61 00 61 00 61 00 00 00  61 00 61 00 61 00 61 00  |a.a.a...a.a.a.a.|
#                                                pinyin
#                                          candidate2
#                       magic        hanzi_offset2
# 00000060  61 00 00 00 08 00 08 00  10 00 05 06 62 00 62 00  |a...........b.b.|
#                       phrase
# 00000070  62 00 00 00 62 00 62 00  62 00 62 00 00 00        |b...b.b.b.b...|
# 0000007e
proto : 'mschxudp'
phrase_offset_start + 4*phrase_count == phrase_start
phrase_start + phrase_offsets[N] == magic(0x00080008)
pinyin&phrase: utf16-le string
hanzi_offset = 8 + len(pinyin)
phrase_offsets[N] + hanzi_offset + len(phrase) == phrase_offsets[N + 1]

*/
        //#region IWordLibraryImport 成员

        ////public bool OnlySinglePinyin { get; set; }

        //public WordLibraryList Import(string path)
        //{
          
        //    return ReadFile(path);
        //}

        //#endregion

        //private Dictionary<int, string> pyDic = new Dictionary<int, string>();



        //public WordLibraryList ImportLine(string line)
        //{
        //    throw new Exception("Win10微软拼音格式是二进制文件，不支持流转换");
        //}

        //private WordLibrary ImportOnePhrase(FileStream fs)
        //{
        //    var position = fs.Position;
        // var magic=   BinFileHelper.ReadInt32(fs);// magic(0x00080008)
        //    var hzoffset= BinFileHelper.ReadInt16(fs);
        //    var rank = fs.ReadByte();
        //    var unknow = fs.ReadByte();
        //    var pinyinByteLen = hzoffset - 10;
        //    var pinyin = Encoding.Unicode.GetString(BinFileHelper.ReadArray(fs, pinyinByteLen));
        //    //TODO
        //    return new WordLibrary();
        //}
        //private WordLibraryList ReadFile(string path)
        //{
        //    pyDic = new Dictionary<int, string>();
        //    //Dictionary<string, string> pyAndWord = new Dictionary<string, string>();
        //    var pyAndWord = new WordLibraryList();
        //    var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
         
        //    fs.Position = 0x10;
        //    var phrase_start = BinFileHelper.ReadInt32(fs);
        //    var phrase_end= BinFileHelper.ReadInt32(fs);
        //    CountWord= (int)BinFileHelper.ReadInt64(fs);
        //    CurrentStatus = 0;
        //    fs.Position = phrase_start;
        //    for (int i = 0; i < CountWord; i++)
        //    {
        //        pyAndWord.Add(ImportOnePhrase(fs));
        //    }

        //    fs.Close();
        //    return pyAndWord;

        //}

      

        public Encoding Encoding { get; }

        public CodeType CodeType
        {
            get
            {
                return CodeType.Pinyin;
            }
        }

        public IList<string> Export(WordLibraryList wlList)
        {
          
            string tempPath =Path.GetDirectoryName( Application.ExecutablePath)+ "\\Win10微软拼音词库.txt";

            var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Encoding.ASCII.GetBytes("mschxudp"));//proto8
            bw.Write(BitConverter.GetBytes(1));//version
            bw.Write(BitConverter.GetBytes(0x40));//phrase_offset_start
            bw.Write(BitConverter.GetBytes(0x40+4*wlList.Count));//phrase_start=phrase_offset_start + 4*phrase_count
            bw.Write(BitConverter.GetBytes(0));//phrase_end input after process all!
            bw.Write(BitConverter.GetBytes((long)wlList.Count));//phrase_count
            bw.Write(BitConverter.GetBytes(DateTime.Now.Ticks));//timestamp
            bw.Write(BitConverter.GetBytes((long)0));//0
            bw.Write(BitConverter.GetBytes((long)0));//0
            bw.Write(BitConverter.GetBytes((long)0));//0
            int offset = 0;
            for (var i = 0; i < wlList.Count; i++)
            {
                bw.Write(BitConverter.GetBytes(offset));
                var wl = wlList[i];
                offset += 8 + wl.Word.Length*2 + 2 + wl.GetPinYinLength()*2 + 2;
            }
            for (var i = 0; i < wlList.Count; i++)
            {
                bw.Write(BitConverter.GetBytes(0x00080008)); //magic
                var wl = wlList[i];
                var hanzi_offset = 8+wl.GetPinYinLength()*2+2;
                bw.Write(BitConverter.GetBytes((short)hanzi_offset));
                bw.Write((byte)0x1);//1是詞頻
                bw.Write((byte)0x6);//6不知道
                var py = wl.GetPinYinString("", BuildType.None);
                bw.Write(Encoding.Unicode.GetBytes(py));
                bw.Write(BitConverter.GetBytes((short)0));
                bw.Write(Encoding.Unicode.GetBytes(wl.Word));
                bw.Write(BitConverter.GetBytes((short)0));
            }
           
            fs.Position = 0x14;
            fs.Write(BitConverter.GetBytes(fs.Length),0,4);
           
            fs.Close();
            return new List<string>() {"词库文件在："+ tempPath };
        }

        public string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException();
        }
    }
}