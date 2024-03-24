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
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Win10微软拼音
    /// </summary>
    [ComboBoxShow(ConstantString.WIN10_MS_WUBI, ConstantString.WIN10_MS_WUBI_C, 131)]
    public class Win10MsWubi : IWordLibraryExport, IWordLibraryImport
    {
        public event Action<string> ImportLineErrorNotice;

        /*
         * _X 做后缀的字段表示 win10 1703 与 1607 有改动的部分

        # win10 1703
        #           proto8                   unknown_X   version
        # 00000000  6d 73 63 68 78 75 64 70  02 00 60 00 01 00 00 00  |mschxudp..`.....|
        #           phrase_offset_start
        #                       phrase_start phrase_end  phrase_count
        # 00000010  40 00 00 00 48 00 00 00  98 00 00 00 02 00 00 00  |@...H...........|
        #           timestamp
        # 00000020  49 4e 06 59 00 00 00 00  00 00 00 00 00 00 00 00  |IN.Y............|
        # 00000030  00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00  |................|
        #                                                      candidate2
        #           phrase_offsets[]         magic_X     phrase_offset2
        # 00000040  00 00 00 00 24 00 00 00  10 00 10 00 18 00 06 06  |....$...........|
        #           phrase_unknown8_X        pinyin
        # 00000050  00 00 00 00 96 0a 99 20  61 00 61 00 61 00 00 00  |....... a.a.a...|
        #           phrase                               magic_X
        # 00000060  61 00 61 00 61 00 61 00  61 00 00 00 10 00 10 00  |a.a.a.a.a.......|
        #                       phrase_unknown8_X
        #                 candidate2
        #           offset2                        pinyin
        # 00000070  1a 00 07 06 00 00 00 00  a6 0a 99 20 62 00 62 00  |........... b.b.|
        #                             phrase
        # 00000080  62 00 62 00 00 00 62 00  62 00 62 00 62 00 62 00  |b.b...b.b.b.b.b.|
        # 00000090  62 00 62 00 62 00 00 00                           |b.b.b...|
        # 00000098

        # win10 1607
        #           proto8                   version     phrase_offset_start
        # 00000000  6d 73 63 68 78 75 64 70  01 00 00 00 40 00 00 00  |mschxudp....@...|
        #          phrase_start phrase_end   phrase_count unknown_X
        # 00000010  48 00 00 00 7e 00 00 00  02 00 00 00 00 00 00 00  |H...~...........|
        #           timestamp
        # 00000020  29 b8 cc 58 00 00 00 00  00 00 00 00 00 00 00 00  |)..X............|
        # 00000030  00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00  |................|
        #                                                      candidate2
        #           phrase_offsets[]         magic       offset2
        # 00000040  00 00 00 00 1c 00 00 00  08 00 08 00 10 00 01 06  |................|
        #           pinyin                   phrase
        # 00000050  61 00 61 00 61 00 00 00  61 00 61 00 61 00 61 00  |a.a.a...a.a.a.a.|
        #                                                pinyin
        #                                          candidate2
        #                       magic        offset2
        # 00000060  61 00 00 00 08 00 08 00  10 00 05 06 62 00 62 00  |a...........b.b.|
        #                       phrase
        # 00000070  62 00 00 00 62 00 62 00  62 00 62 00 00 00        |b...b.b.b.b...|
        # 0000007e
        proto : 'mschxudp'
        phrase_offset_start + 4*phrase_count == phrase_start
        phrase_start + phrase_offsets[N] == magic(0x00080008)
        pinyin&phrase: utf16-le string
        hanzi_offset = 8 + len(pinyin)
        phrase_offsets[N] + offset + len(phrase) == phrase_offsets[N+1]
        candidate 第一个字节代表短语在候选框位置

            */

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public int CountWord { get; set; }
        public int CurrentStatus { get; set; }

        public bool IsText => false;

        public CodeType CodeType
        {
            get { return CodeType.Wubi98; }
        }

        public WordLibraryList Import(string path)
        {
            var pyAndWord = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = 0x10;
            var phrase_offset_start = BinFileHelper.ReadInt32(fs);
            var phrase_start = BinFileHelper.ReadInt32(fs);
            var phrase_end = BinFileHelper.ReadInt32(fs);
            var phrase_count = BinFileHelper.ReadInt32(fs);

            fs.Position = phrase_offset_start;
            var offsets = ReadOffsets(fs, phrase_count);
            offsets.Add(phrase_end - phrase_start);

            fs.Position = phrase_start;
            for (var i = 0; i < phrase_count; i++)
            {
                var wl = ReadOnePhrase(fs, phrase_start + offsets[i + 1]);
                if (wl != null)
                {
                    pyAndWord.Add(wl);
                }
            }
            return pyAndWord;
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
            var unknown8 = BinFileHelper.ReadInt64(fs); //新增的，不知道什么意思
            var pyBytesLen = hanzi_offset - 18;
            var pyBytes = BinFileHelper.ReadArray(fs, pyBytesLen);
            var wubiStr = Encoding.Unicode.GetString(pyBytes);
            var split = BinFileHelper.ReadInt16(fs); //00 00 分割拼音和汉字
            var wordBytesLen = nextStartPosition - (int)fs.Position - 2; //结尾还有个00 00
            var wordBytes = BinFileHelper.ReadArray(fs, wordBytesLen);
            BinFileHelper.ReadInt16(fs); //00 00分割
            var word = Encoding.Unicode.GetString(wordBytes);
            wl.Word = word;
            try
            {
                wl.SetCode(CodeType.Wubi98, wubiStr);
            }
            catch
            {
                return null;
            }
            wl.CodeType = CodeType.Wubi98;
            return wl;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }

        public event Action<string> ExportErrorNotice;

        public IList<string> Export(WordLibraryList wlList)
        {
            //Win10拼音只支持最多32个字符的编码
            wlList = Filter(wlList);
            string tempPath = Path.Combine(
                FileOperationHelper.GetCurrentFolderPath(),
                "Win10微软五笔词库.dat"
            );
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Encoding.ASCII.GetBytes("mschxudp")); //proto8
            bw.Write(BitConverter.GetBytes(0x00600002)); //Unknown
            bw.Write(BitConverter.GetBytes(1)); //version
            bw.Write(BitConverter.GetBytes(0x40)); //phrase_offset_start
            bw.Write(BitConverter.GetBytes(0x40 + 4 * wlList.Count)); //phrase_start=phrase_offset_start + 4*phrase_count
            bw.Write(BitConverter.GetBytes(0)); //phrase_end input after process all!
            bw.Write(BitConverter.GetBytes(wlList.Count)); //phrase_count
            bw.Write(BitConverter.GetBytes(DateTime.Now.Ticks)); //timestamp
            bw.Write(BitConverter.GetBytes((long)0)); //0
            bw.Write(BitConverter.GetBytes((long)0)); //0
            bw.Write(BitConverter.GetBytes((long)0)); //0
            int offset = 0;
            for (var i = 0; i < wlList.Count; i++)
            {
                bw.Write(BitConverter.GetBytes(offset));
                var wl = wlList[i];
                offset += 8 + 8 + wl.Word.Length * 2 + 2 + wl.GetPinYinLength() * 2 + 2;
            }
            for (var i = 0; i < wlList.Count; i++)
            {
                bw.Write(BitConverter.GetBytes(0x00100010)); //magic
                var wl = wlList[i];
                var hanzi_offset = 8 + 8 + wl.GetPinYinLength() * 2 + 2;
                bw.Write(BitConverter.GetBytes((short)hanzi_offset));
                bw.Write((byte)wl.Rank); //1是詞頻
                bw.Write((byte)0x6); //6不知道
                bw.Write(BitConverter.GetBytes(0x00000000)); //Unknown
                bw.Write(BitConverter.GetBytes(0xE679CD20)); //Unknown
                var py = wl.GetPinYinString("", BuildType.None);
                bw.Write(Encoding.Unicode.GetBytes(py));
                bw.Write(BitConverter.GetBytes((short)0));
                bw.Write(Encoding.Unicode.GetBytes(wl.Word));
                bw.Write(BitConverter.GetBytes((short)0));
            }

            fs.Position = 0x18;
            fs.Write(BitConverter.GetBytes(fs.Length), 0, 4);

            fs.Close();
            return new List<string>() { "词库文件在：" + tempPath };
        }

        private WordLibraryList Filter(WordLibraryList wlList)
        {
            var result = new WordLibraryList();
            //var key = new List<string>();
            foreach (var wl in wlList)
            {
                if (wl.GetPinYinLength() > 32)
                    continue;
                if (wl.Word.Length > 64)
                    continue;
                //var py = wl.GetPinYinString("", BuildType.None);
                //if (!key.Contains(py))
                //{
                result.Add(wl);
                //    key.Add(py);
                //}
            }
            return result;
        }

        public string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }
    }
}
