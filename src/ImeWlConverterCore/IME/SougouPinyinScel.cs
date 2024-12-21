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
    ///     搜狗细胞词库
    /// </summary>
    [ComboBoxShow(ConstantString.SOUGOU_XIBAO_SCEL, ConstantString.SOUGOU_XIBAO_SCEL_C, 20)]
    public class SougouPinyinScel : BaseImport, IWordLibraryImport
    {
        #region IWordLibraryImport 成员

        //public bool OnlySinglePinyin { get; set; }

        public WordLibraryList Import(string path)
        {
            //var str = ReadScel(path);
            //var wlList = new WordLibraryList();
            //string[] lines = str.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            //CountWord = lines.Length;
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    CurrentStatus = i;
            //    string line = lines[i];
            //    if (line.IndexOf("'") == 0)
            //    {
            //        wlList.AddWordLibraryList(ImportLine(line));
            //    }
            //}
            //return wlList;
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

        public static Dictionary<string, string> ReadScelInfo(string path)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            fs.Position = 0x124;
            var CountWord = BinFileHelper.ReadInt32(fs);
            info.Add("CountWord", CountWord.ToString());

            info.Add("Name", readScelFieldText(fs, 0x130));
            info.Add("Type", readScelFieldText(fs, 0x338));
            info.Add("Info", readScelFieldText(fs, 0x540, 1024));
            info.Add("Sample", readScelFieldText(fs, 0xd40, 1024));

            fs.Close();
            return info;
        }

        private static string readScelFieldText(FileStream fs, long seek, int length = 64)
        {
            long oldSeek = fs.Position;
            if (seek > fs.Length)
                throw new ArgumentException("地址超过文件长度");
            fs.Seek(seek, SeekOrigin.Begin);
            var bytes = new byte[length];
            fs.Read(bytes, 0, length);
            string value = Encoding.Unicode.GetString(bytes);
            int end = value.IndexOf('\0');
            if (end < 0)
                throw new ArgumentException("未找到\\0，可能索求长度不足");
            string text = value.Substring(0, end);
            fs.Position = oldSeek;
            return text;
        }

        private WordLibraryList ReadScel(string path)
        {
            pyDic = new Dictionary<int, string>();
            //Dictionary<string, string> pyAndWord = new Dictionary<string, string>();
            var pyAndWord = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var str = new byte[128];
            var outstr = new byte[128];

            // 未展开的词条数（同音词算1个
            fs.Position = 0x120;
            var dictLen = BinFileHelper.ReadInt32(fs);

            // 拼音表的长度
            fs.Position = 0x1540;
            var pyDicLen = BinFileHelper.ReadInt32(fs);

            str = new byte[4];
            for (int i = 0; i < pyDicLen; i++)
            {
                var idx = BinFileHelper.ReadInt16(fs);
                var size = BinFileHelper.ReadInt16(fs);
                str = new byte[size];
                fs.Read(str, 0, size);
                string py = Encoding.Unicode.GetString(str);
                pyDic.Add(idx, py);
            }

            var s = new StringBuilder();
            foreach (string value in pyDic.Values)
            {
                s.Append(value + "\",\"");
            }
            Debug.WriteLine(s.ToString());

            for (int i = 0; i < dictLen; i++)
            {
                try
                {
                    pyAndWord.AddRange(ReadAPinyinWord(fs));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
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
            int samePYcount = num[0] + num[1] * 256;
            int count = num[2] + num[3] * 256;
            //接下来读拼音
            var str = new byte[256];
            for (int i = 0; i < count; i++)
            {
                str[i] = (byte)fs.ReadByte();
            }
            var wordPY = new List<string>();
            for (int i = 0; i < count / 2; i++)
            {
                int key = str[i * 2] + str[i * 2 + 1] * 256;
                if (key < pyDic.Count)
                    wordPY.Add(pyDic[key]);
                else
                    wordPY.Add(((char)(key - pyDic.Count + 97)).ToString());
            }
            //wordPY = wordPY.Remove(wordPY.Length - 1); //移除最后一个单引号
            //接下来读词语
            var pyAndWord = new List<WordLibrary>();
            for (int s = 0; s < samePYcount; s++) //同音词，使用前面相同的拼音
            {
                num = new byte[2];
                fs.Read(num, 0, 2);
                int hzBytecount = num[0] + num[1] * 256;
                str = new byte[hzBytecount];
                fs.Read(str, 0, hzBytecount);
                string word = Encoding.Unicode.GetString(str);
                short unknown1 = BinFileHelper.ReadInt16(fs); //全部是10,肯定不是词频，具体是什么不知道
                int unknown2 = BinFileHelper.ReadInt32(fs); //每个字对应的数字不一样，不知道是不是词频
                pyAndWord.Add(
                    new WordLibrary
                    {
                        Word = word,
                        PinYin = wordPY.ToArray(),
                        Rank = DefaultRank
                    }
                );
                CurrentStatus++;
                //接下来10个字节什么意思呢？暂时先忽略了
                var temp = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    temp[i] = (byte)fs.ReadByte();
                }
            }
            return pyAndWord;
        }
    }
}
