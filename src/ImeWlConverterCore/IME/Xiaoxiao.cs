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

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 小小输入法是一个框架，根据不同的选项来切换不同的输入法。可以指定不同的词库。以拼音词库为例，词库格式为：
    /// 拼音（无分隔符）+空格+词语1+空格+词语2...
    /// </summary>
    [ComboBoxShow(ConstantString.XIAOXIAO, ConstantString.XIAOXIAO_C, 100)]
    public class Xiaoxiao
        : BaseTextImport,
            IWordLibraryExport,
            IWordLibraryTextImport,
            IMultiCodeType
    {
        #region IWordLibraryExport 成员

        private IWordCodeGenerater codeGenerater;

        //private XiaoxiaoConfigForm form;
        private IWordCodeGenerater CodeGenerater
        {
            get
            {
                if (codeGenerater == null)
                {
                    codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
                }
                return codeGenerater;
            }
        }

        public override Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                try
                {
                    return Encoding.GetEncoding("GB18030");
                }
                catch
                {
                    return Encoding.GetEncoding("GB2312");
                }
            }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            if (CodeType == CodeType.Pinyin)
            {
                sb.Append(wl.GetPinYinString("", BuildType.None));
            }
            else if (CodeType == wl.CodeType)
            {
                sb.Append(wl.Codes[0][0]);
            }
            else
            {
                var code = CodeGenerater.GetCodeOfString(wl.Word);
                sb.Append(code.ToCodeString());
            }
            sb.Append(" ");
            sb.Append(wl.Word);
            return sb.ToString();
        }

        //public Form ExportConfigForm { get { return form; } }
        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            //sb.Append(GetFileHeader());
            IDictionary<string, string> xiaoxiaoDic = new Dictionary<string, string>();

            for (int i = 0; i < wlList.Count; i++)
            {
                string key = "";
                WordLibrary wl = wlList[i];
                string value = wl.Word;
                if (CodeType == CodeType.Pinyin)
                {
                    key = (wl.GetPinYinString("", BuildType.None));
                }
                else if (CodeType == wl.CodeType)
                {
                    key = (wl.Codes[0][0]);
                }
                else
                {
                    var codes = CodeGenerater.GetCodeOfString(wl.Word);
                    var list = codes.ToCodeString();
                    foreach (var code in list)
                    {
                        if (xiaoxiaoDic.ContainsKey(code))
                        {
                            xiaoxiaoDic[code] += " " + value;
                        }
                        else
                        {
                            xiaoxiaoDic.Add(code, value);
                        }
                    }
                }

                if (xiaoxiaoDic.ContainsKey(key))
                {
                    xiaoxiaoDic[key] += " " + value;
                }
                else
                {
                    xiaoxiaoDic.Add(key, value);
                }
            }
            foreach (var keyValuePair in xiaoxiaoDic)
            {
                sb.Append(keyValuePair.Key + " " + keyValuePair.Value + "\n");
            }

            return new List<string>() { sb.ToString() };
        }

        #endregion

        //      private string GetFileHeader()
        //      {
        //          if (CodeType == CodeType.Pinyin)
        //          {
        //              return @"#名称：深蓝词库转换词库
        //#作者：深蓝
        //name=拼音
        //key=abcdefghijklmnopqrstuvwxyz
        //len=63
        //wildcard=?
        //pinyin=1
        //split='
        //hint=0
        //user=pinyin.usr
        //assist=mb/yong.txt 2
        //code_a1=p..
        //[DATA]
        //";
        //          }
        //          if (CodeType == CodeType.Wubi)
        //          {
        //              return @"#名称：深蓝词库转换词库
        //#作者：深蓝
        //name=五笔
        //key=abcdefghijklmnopqrstuvwxyz
        //len=4
        //assist=z mb/pinyin.txt
        //wildcard=z
        //dwf=1
        //commit=0 0 0
        //auto_clear=4
        //code_e2=p11+p12+p21+p22
        //code_e3=p11+p21+p31+p32
        //code_a4=p11+p21+p31+n11
        //[data]
        //";
        //          }
        //          return "";

        //      }

        private readonly Regex regex = new Regex(@"[^\s#]+( [\u4E00-\u9FA5]+)+");

        protected override bool IsContent(string line)
        {
            return regex.IsMatch(line);
        }

        //private IWordCodeGenerater pyGenerater = new PinyinGenerater();
        public override WordLibraryList ImportLine(string str)
        {
            var list = new WordLibraryList();
            string[] words = str.Split(' ');
            for (int i = 1; i < words.Length; i++)
            {
                string word = words[i];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = DefaultRank;
                wl.SetCode(CodeType, words[0]);
                //wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(word));
                list.Add(wl);
            }
            return list;
        }
    }
}
