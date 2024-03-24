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

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.BAIDU_BCD, ConstantString.BAIDU_BCD_C, 1020)]
    public class BaiduShoujiBcd : BaseImport, IWordLibraryImport
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
            var wordLibraryList = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = 0x350;
            do
            {
                try
                {
                    WordLibrary wl = ImportWord(fs);
                    if (wl.Word != "" && wl.PinYin.Length > 0)
                    {
                        wordLibraryList.Add(wl);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            } while (fs.Position != fs.Length);
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

        //public Dictionary<char,string > CharAndPinyin=new Dictionary<char, string>();
        //private void AddWordAndPinyin(char word,string pinyin)
        //{
        //    if (!CharAndPinyin.ContainsKey(word))
        //    {
        //        CharAndPinyin.Add(word,pinyin);
        //    }
        //}

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException();
        }

        private WordLibrary ImportWord(FileStream fs)
        {
            var wordLibrary = new WordLibrary();
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            short len = BitConverter.ToInt16(temp, 0);
            fs.Read(temp, 0, 2); //what's the meaning of these 2 bytes?
            var pinyinList = new List<string>();
            for (int i = 0; i < len; i++)
            {
                temp = new byte[2];
                fs.Read(temp, 0, 2);

                pinyinList.Add(Shengmu[temp[0]] + Yunmu[temp[1]]);
            }
            wordLibrary.PinYin = pinyinList.ToArray();
            temp = new byte[2 * len];
            fs.Read(temp, 0, 2 * len);
            wordLibrary.Word = Encoding.Unicode.GetString(temp);
            //for (var i = 0; i < wordLibrary.Word.Length;i++ )
            //{
            //    AddWordAndPinyin(wordLibrary.Word[i], wordLibrary.PinYin[i]);
            //}
            return wordLibrary;
        }
    }
}
