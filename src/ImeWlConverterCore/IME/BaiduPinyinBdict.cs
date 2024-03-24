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
    [ComboBoxShow(ConstantString.BAIDU_BDICT, ConstantString.BAIDU_BDICT_C, 100)]
    public class BaiduPinyinBdict : BaseImport, IWordLibraryImport
    {
        private readonly List<string> Shengmu = new List<string>
        {
            "c",
            "d",
            "b",
            "f",
            "g",
            "h",
            "ch",
            "j",
            "k",
            "l",
            "m",
            "n",
            "",
            "p",
            "q",
            "r",
            "s",
            "t",
            "sh",
            "zh",
            "w",
            "x",
            "y",
            "z"
        };

        private readonly List<string> Yunmu = new List<string>
        {
            "uang",
            "iang",
            "iong",
            "ang",
            "eng",
            "ian",
            "iao",
            "ing",
            "ong",
            "uai",
            "uan",
            "ai",
            "an",
            "ao",
            "ei",
            "en",
            "er",
            "ua",
            "ie",
            "in",
            "iu",
            "ou",
            "ia",
            "ue",
            "ui",
            "un",
            "uo",
            "a",
            "e",
            "i",
            "o",
            "u",
            "v"
        };

        #region IWordLibraryImport Members

        public override bool IsText
        {
            get { return false; }
        }

        #endregion

        public WordLibraryList Import(string path)
        {
            int endPosition = 0;
            var wordLibraryList = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = 0x60;
            endPosition = BinFileHelper.ReadInt32(fs);
            fs.Position = 0x350;
            CurrentStatus = 0;
            do
            {
                //CurrentStatus++;
                try
                {
                    WordLibrary wl = ImportWord(fs);
                    if (wl == null)
                    {
                        break;
                    }
                    if (wl.Word != "" && wl.PinYin.Length > 0)
                    {
                        wordLibraryList.Add(wl);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            } while (fs.Position != endPosition); //< fs.Length
            fs.Close();
            //StreamWriter sw=new StreamWriter("D:\\py.txt",true,Encoding.Unicode);
            //SinglePinyin singlePinyin=new SinglePinyin();

            //foreach (var cpy in CharAndPinyin)
            //{
            //    var py = "";
            //    try
            //    {
            //        py = singlePinyin.GetPinYinOfChar(cpy.Key)[0];
            //    }
            //    catch
            //    {
            //        Debug.Write(cpy.Key);
            //    }
            //    sw.WriteLine(cpy.Key+"\t"+ py+"\t"+cpy.Value);
            //}
            //sw.Close();

            //wordLibraryList.ForEach(delegate(WordLibrary wl) { if(wl.Word==""||wl.PinYin.Length==0)
            //{
            //    Debug.WriteLine(wl.ToDisplayString());
            //}
            //});

            return wordLibraryList;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException();
        }

        //public Dictionary<char,string > CharAndPinyin=new Dictionary<char, string>();
        //private void AddWordAndPinyin(char word,string pinyin)
        //{
        //    if (!CharAndPinyin.ContainsKey(word))
        //    {
        //        CharAndPinyin.Add(word,pinyin);
        //    }
        //}
        /// <summary>
        ///     读取一个词语，格式为：
        ///     4字节的长度len
        ///     len*2字节的拼音（对于每个字，声母、韵母各1字节）
        ///     len*2字节的汉字（Unicode编码）
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        private WordLibrary ImportWord(FileStream fs)
        {
            int show = 0;
            var wordLibrary = new WordLibrary();
            var temp = new byte[4];
            fs.Read(temp, 0, 4);
            int len = BitConverter.ToInt32(temp, 0);
            if (len > 1000)
            {
                throw new Exception("有异常的词库，解析失败");
            }
            if (len == 0)
            {
                Debug.WriteLine(fs.Position);
                return null;
                //return SpecialWord(fs);
            }
            var pinyinList = new List<string>();
            for (int i = 0; i < len; i++)
            {
                temp = new byte[2];
                fs.Read(temp, 0, 2);
                try
                {
                    string sm = Shengmu[temp[0]];
                    string ym = Yunmu[temp[1]];

                    pinyinList.Add(sm + ym);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    show = temp[0];
                }
            }
            wordLibrary.PinYin = pinyinList.ToArray();
            temp = new byte[2 * len];
            fs.Read(temp, 0, 2 * len);
            wordLibrary.Word = Encoding.Unicode.GetString(temp);
            //for (var i = 0; i < wordLibrary.Word.Length;i++ )
            //{
            //    AddWordAndPinyin(wordLibrary.Word[i], wordLibrary.PinYin[i]);
            //}
            if (show > 0)
            {
                Debug.WriteLine(show + "  " + wordLibrary.Word + "----" + wordLibrary.PinYinString);
            }
            return wordLibrary;
        }

        private WordLibrary SpecialWord(FileStream fs)
        {
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            short pinyinLen = BitConverter.ToInt16(temp, 0);
            fs.Read(temp, 0, 2);
            short wordLen = BitConverter.ToInt16(temp, 0);
            temp = new byte[pinyinLen * 2];
            fs.Read(temp, 0, pinyinLen * 2);
            string pinyinString = Encoding.Unicode.GetString(temp);
            temp = new byte[wordLen * 2];
            fs.Read(temp, 0, wordLen * 2);
            string word = Encoding.Unicode.GetString(temp);
            var wordLibrary = new WordLibrary();
            wordLibrary.Word = word;
            wordLibrary.PinYin = pinyinString.Split('\'');
            return wordLibrary;
        }
    }
}
