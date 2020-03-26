using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Win10微软拼音
    /// </summary>
    [ComboBoxShow(ConstantString.WIN10_MS_PINYIN_SELF_STUDY, ConstantString.WIN10_MS_PINYIN_SELF_STUDY_C, 131)]
    public class Win10MsPinyinSelfStudy : IWordLibraryExport, IWordLibraryImport
    {
        public event Action<string> ImportLineErrorNotice;
        public Win10MsPinyinSelfStudy()
        {
            this.CodeType = CodeType.NoCode;
            this.PinyinType = PinyinType.FullPinyin;
        }
      
        public PinyinType PinyinType
        {
            get;set;
        }
        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public int CountWord { get; set; }
        public int CurrentStatus { get; set; }

        public bool IsText => false;

        public CodeType CodeType
        {
            get;set;
        }
        /// <summary>
        /// 小端数字到int
        /// </summary>
        /// <param name="src">数组</param>
        /// <param name="offset">从数组offset开始</param>
        /// <param name="len">长len个字节</param>
        /// <returns></returns>
        private int bytesToIntLittle(byte[] src, int offset, int len)
        {
            int value = 0, sf = 0;
            for (; len > 0; offset++, len--, sf += 8)
                value |= (src[offset] & 0xFF) << sf;
            return value;
        }
        public WordLibraryList Import(string path)
        {
            WordLibraryList re = new WordLibraryList();
            FileStream fp = File.OpenRead(path);
            int user_word_base = 0x2400;
            //get word num
            byte[] bytes = new byte[50];
            fp.Seek(12, SeekOrigin.Begin);
            fp.Read(bytes, 0, 4);
            int cnt = bytesToIntLittle(bytes, 0, 4);
            //get each word
            for (int i = 0; i < cnt; i++)
            {
                int cur_idx = user_word_base + i * 60;
                //get word len
                fp.Seek(cur_idx + 10, SeekOrigin.Begin);
                fp.Read(bytes, 0, 1);
                int wordLen = bytesToIntLittle(bytes, 0, 1);
                //get word
                fp.Seek(cur_idx + 12, SeekOrigin.Begin);
                fp.Read(bytes, 0, wordLen * 2);
                string word = Encoding.Unicode.GetString(bytes, 0, wordLen * 2);
                re.Add(new WordLibrary() { Word=word,CodeType=this.CodeType, });
            }
            fp.Close();
            return re;
        }

      
        private IList<int> ReadOffsets(FileStream fs, int count)
        {
            var result = new List<int>();

            for (var i = 0; i < count; i++)
            {
                var offset = BinFileHelper.ReadInt32(fs);
                result.Add(offset);
            }
            return result;
        }

        private WordLibrary ReadOnePhrase(FileStream fs, int nextStartPosition)
        {
            WordLibrary wl = new WordLibrary();
            var magic = BinFileHelper.ReadInt32(fs);
            var hanzi_offset = BinFileHelper.ReadInt16(fs);
            wl.Rank = fs.ReadByte();
            var x6 = fs.ReadByte(); //不知道干啥的
            var unknown8 = BinFileHelper.ReadInt64(fs);//新增的，不知道什么意思
            var pyBytesLen = hanzi_offset - 18;
            var pyBytes = BinFileHelper.ReadArray(fs, pyBytesLen);
            var pyStr = Encoding.Unicode.GetString(pyBytes);
            var split = BinFileHelper.ReadInt16(fs); //00 00 分割拼音和汉字
            var wordBytesLen = nextStartPosition - (int) fs.Position - 2; //结尾还有个00 00
            var wordBytes = BinFileHelper.ReadArray(fs, wordBytesLen);
            BinFileHelper.ReadInt16(fs); //00 00分割
            var word = Encoding.Unicode.GetString(wordBytes);
            wl.Word = word;
            try
            {
                wl.SetPinyinString(pyStr);
                wl.CodeType = CodeType.Pinyin;
            }
            catch
            {
                wl.CodeType = CodeType.NoCode;
            }
         
            return wl;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            //Win10拼音只支持最多32个字符的编码
            wlList = Filter(wlList);
           
            string tempPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Win10微软拼音词库.dat";
            if (File.Exists(tempPath)) { File.Delete(tempPath); }
            var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Encoding.ASCII.GetBytes("mschxudp")); //proto8
            bw.Write(BitConverter.GetBytes(0x00600002));//Unknown
            bw.Write(BitConverter.GetBytes(1)); //version
            bw.Write(BitConverter.GetBytes(0x40)); //phrase_offset_start
            bw.Write(BitConverter.GetBytes(0x40 + 4*wlList.Count)); //phrase_start=phrase_offset_start + 4*phrase_count
            bw.Write(BitConverter.GetBytes(0)); //phrase_end input after process all!
            bw.Write(BitConverter.GetBytes(wlList.Count)); //phrase_count
            bw.Write(BitConverter.GetBytes(DateTime.Now.Ticks)); //timestamp
            bw.Write(BitConverter.GetBytes((long) 0)); //0
            bw.Write(BitConverter.GetBytes((long) 0)); //0
            bw.Write(BitConverter.GetBytes((long) 0)); //0
            int offset = 0;
            for (var i = 0; i < wlList.Count; i++)
            {
                bw.Write(BitConverter.GetBytes(offset));
                var wl = wlList[i];
                offset += 8 +8+ wl.Word.Length*2 + 2 + wl.GetPinYinLength()*2 + 2;
            }

            for (var i = 0; i < wlList.Count; i++)
            {
                bw.Write(BitConverter.GetBytes(0x00100010)); //magic
                var wl = wlList[i];
                var hanzi_offset = 8 +8+ wl.GetPinYinLength()*2 + 2;
                bw.Write(BitConverter.GetBytes((short) hanzi_offset));
                bw.Write((byte) wl.Rank); //1是詞頻
                bw.Write((byte) 0x6); //6不知道
                bw.Write(BitConverter.GetBytes(0x00000000));//Unknown
                bw.Write(BitConverter.GetBytes(0xE679CD20));//Unknown
                
                var py = wl.GetPinYinString("", BuildType.None);
                bw.Write(Encoding.Unicode.GetBytes(py));
                bw.Write(BitConverter.GetBytes((short) 0));
                bw.Write(Encoding.Unicode.GetBytes(wl.Word));
                bw.Write(BitConverter.GetBytes((short) 0));
            }

            fs.Position = 0x18;
            fs.Write(BitConverter.GetBytes(fs.Length), 0, 4);

            fs.Close();
            return new List<string>() {"词库文件在：" + tempPath};
        }

        private WordLibraryList Filter(WordLibraryList wlList)
        {
            var result = new WordLibraryList();
            IReplaceFilter replace = null;
            if (PinyinType != PinyinType.FullPinyin)
            {
                replace = new ShuangpinReplacer(PinyinType);
            }
            foreach (var wl in wlList)
            {
                if(replace!=null)
                {
                    replace.Replace(wl);
                }

                if (wl.GetPinYinLength() > 32)
                    continue;
                if (wl.Word.Length > 64)
                    continue;
             
                result.Add(wl);
      
            }
            return result;
        }

        public string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }
    }
}
 