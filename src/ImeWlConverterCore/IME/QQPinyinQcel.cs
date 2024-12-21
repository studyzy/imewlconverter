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
    [ComboBoxShow(ConstantString.QQ_PINYIN_QCEL, ConstantString.QQ_PINYIN_QCEL_C, 60)]
    public class QQPinyinQcel : BaseImport, IWordLibraryImport
    {
        #region IWordLibraryImport 成员

        //public bool OnlySinglePinyin { get; set; }

        public WordLibraryList Import(string path)
        {
            return ReadQcel(path);
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
            throw new Exception("Qcel格式是二进制文件，不支持流转换");
        }

        public static Dictionary<string, string> ReadQcelInfo(string path)
        {
            return SougouPinyinScel.ReadScelInfo(path);
        }

        private WordLibraryList ReadQcel(string path)
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

            // int hzPosition = 0;
            fs.Read(str, 0, 128); //\x40\x15\x00\x00\x44\x43\x53\x01
            // if (str[4] == 0x44)
            // {
            //     hzPosition = 0x2628;
            // }
            // if (str[4] == 0x45)
            // {
            //     hzPosition = 0x26C4;
            // }

            fs.Position = 0x124;
            CountWord = BinFileHelper.ReadInt32(fs);
            CurrentStatus = 0;

            fs.Position = 0x1540;
            str = new byte[4];
            fs.Read(str, 0, 4); //\x9D\x01\x00\x00
            while (true)
            {
                num = new byte[4];
                fs.Read(num, 0, 4);
                int mark = num[0] + num[1] * 256;
                str = new byte[num[2]];
                fs.Read(str, 0, (num[2]));
                string py = Encoding.Unicode.GetString(str);
                //py = py.Substring(0, py.IndexOf('\0'));
                pyDic.Add(mark, py);
                if (py == "zuo") //最后一个拼音
                {
                    Debug.WriteLine(fs.Position);
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
            //fs.Position = hzPosition;

            while (true)
            {
                try
                {
                    var data = ReadAPinyinWord(fs);
                    if (data is null)
                        break;

                    pyAndWord.AddRange(data);
                }
                catch (Exception)
                {
                    throw;
                }
                if (CurrentStatus == CountWord || fs.Length == fs.Position) //判断文件结束
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
            int samePYcount = num[0] + num[1] * 256;
            int pinyinLen = num[2] + num[3] * 256;
            //接下来读拼音
            var str = new byte[256];
            for (int i = 0; i < pinyinLen; i++)
            {
                str[i] = (byte)fs.ReadByte();
            }
            var wordPY = new List<string>();
            for (int i = 0; i < pinyinLen / 2; i++)
            {
                int key = str[i * 2] + str[i * 2 + 1] * 256;
                //Debug.Assert(key < pyDic.Count);
                if (key < pyDic.Count)
                    wordPY.Add(pyDic[key]);
                else
                    wordPY.Add(((char)(key - pyDic.Count + 97)).ToString());
                //return null; // 用于调试，忽略编码异常的记录，不中止运行
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
