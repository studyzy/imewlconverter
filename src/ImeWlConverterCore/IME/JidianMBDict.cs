/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)
 *   Copyright © 2022 yfdyh000

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
    /// 极点五笔词库，.mb扩展名，文件头 Freeime Dictionary V6.2
    /// https://github.com/studyzy/imewlconverter/issues/180
    /// http://www.freewb.org/
    /// </summary>
    [ComboBoxShow(ConstantString.JIDIAN_MBDICT, ConstantString.JIDIAN_MBDICT_C, 190)]
    public class Jidian_MBDict : IWordLibraryImport
    {
        private enum DictCodeType
        {
            Pinyin = 1,
            Wubi98 = 2
        }

        public event Action<string> ImportLineErrorNotice;

        public Jidian_MBDict()
        {
            this.CodeType = CodeType.UserDefinePhrase;
            this.PinyinType = PinyinType.FullPinyin;
        }

        public PinyinType PinyinType { get; set; }
        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public int CountWord { get; set; }
        public int CurrentStatus { get; set; }

        public bool IsText => false;

        public CodeType CodeType { get; set; }

        public WordLibraryList Import(string path)
        {
            var pyAndWord = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = 0x00;
            var headerstr = "Freeime Dictionary";
            var header = Encoding.ASCII.GetString(BinFileHelper.ReadArray(fs, headerstr.Length));
            Debug.Assert(header.Equals(headerstr));

            DictCodeType curType;

            fs.Position = 0x23;
            var headerTypeBytes = BinFileHelper.ReadArray(fs, 4);
            var headerTypeStr = Encoding.Unicode.GetString(headerTypeBytes);
            if (headerTypeStr.Equals("拼音"))
                curType = DictCodeType.Pinyin;
            else if (headerTypeStr.Equals("五笔"))
                curType = DictCodeType.Wubi98;
            else
                throw new NotImplementedException("未知词库，请在反馈中提交文件");

            var phrase_start = 0x1B620; // 'a'词条所在
            fs.Position = phrase_start;
            while (true)
            {
                var wl = ReadOnePhrase(fs, curType);
                if (wl != null)
                {
                    pyAndWord.Add(wl);
                }

                if (fs.Length == fs.Position) //文件结束
                {
                    fs.Close();
                    break;
                }
            }
            return pyAndWord;
        }

        private WordLibrary ReadOnePhrase(FileStream fs, DictCodeType type)
        {
            WordLibrary wl = new WordLibrary();
            var codeBytesLen = fs.ReadByte();
            var wordBytesLen = fs.ReadByte();
            var split = fs.ReadByte();
            // 0x64对应正常词组（包含中英混拼，如"阿Q"）。
            Debug.Assert(
                split.Equals(0x64)
                    || split.Equals(0x32)
                    || split.Equals(0x10)
                    || split.Equals(0x66)
                    || split.Equals(0x67)
            ); // 0x67: "$X[计算器]calc"
            var codeBytes = BinFileHelper.ReadArray(fs, codeBytesLen);
            var codeStr = Encoding.ASCII.GetString(codeBytes);

            var wordBytes = BinFileHelper.ReadArray(fs, wordBytesLen);
            var word = Encoding.Unicode.GetString(wordBytes);

            if (split.Equals(0x32)) // 如“醃(腌)”，后者是相应简化字？
            {
                word = word.Substring(0, 1); // 暂定只取首字
            }
            Debug.Assert(word.IndexOf("(") < 0);
            wl.Word = word;
            try
            {
                if (type == DictCodeType.Pinyin)
                {
                    wl.CodeType = CodeType.Pinyin;
                    wl.SetPinyinString(codeStr);
                }
                else if (type == DictCodeType.Wubi98)
                {
                    wl.CodeType = CodeType.Wubi98;
                    wl.SetCode(CodeType.Wubi98, codeStr);
                }
            }
            catch
            {
                wl.CodeType = CodeType.NoCode;
                ImportLineErrorNotice?.Invoke(wl.Word + " 的编码缺失");
            }
            return wl;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }

        public event Action<string> ExportErrorNotice;

        public IList<string> Export(WordLibraryList wlList)
        {
            throw new NotImplementedException("暂不支持");
        }

        public string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }
    }
}
