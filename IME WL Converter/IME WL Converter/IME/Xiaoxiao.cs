using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.XIAOXIAO, ConstantString.XIAOXIAO_C, 100)]
    public class Xiaoxiao : BaseImport, IWordLibraryExport, IWordLibraryTextImport, IMultiCodeType
    {
        public Xiaoxiao()
        {
            form=new XiaoxiaoConfigForm();
            form.Closed += new EventHandler(form_Closed);
        }

        void form_Closed(object sender, EventArgs e)
        {
            this.CodeType = form.SelectedCodeType;
        }   
        #region IWordLibraryExport 成员

        private IWordCodeGenerater codeGenerater;
        private XiaoxiaoConfigForm form;
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

        public Encoding Encoding
        {
            get { return Encoding.GetEncoding("GB18030"); }
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
                sb.Append(wl.Codes);
            }
            else
            {
                sb.Append(CollectionHelper.ListToString(CodeGenerater.GetCodeOfString(wl.Word)));
            }
            sb.Append(" ");
            sb.Append(wl.Word);
            return sb.ToString();
        }

        public Form ExportConfigForm { get { return form; } }
        public override Form ImportConfigForm
        {
            get { return form; }
        }
        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            //sb.Append(GetFileHeader());
            IDictionary<string, string> xiaoxiaoDic = new Dictionary<string, string>();

            for (int i = 0; i < wlList.Count; i++)
            {
                string key = "";
                var wl = wlList[i];
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
                    IList<string> codes = CodeGenerater.GetCodeOfString(wl.Word);
                    if (CodeGenerater.Is1CharMutiCode)
                    {
                        foreach (string code in codes)
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
                        continue;
                    }
                    else
                    {
                        key = (CollectionHelper.ListToString(codes));
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

            return sb.ToString();
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

        public WordLibraryList ImportText(string text)
        {
            var list = new WordLibraryList();
            string[] lines = text.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            CurrentStatus = 0;
            foreach (string s in lines)
            {
                CurrentStatus++;
                if (IsContent(s))
                {
                    list.AddWordLibraryList(ImportLine(s));
                }
            }
            return list;
        }

        private bool IsContent(string line)
        {
            return regex.IsMatch(line);
        }

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        //private IWordCodeGenerater pyGenerater = new PinyinGenerater();
        public WordLibraryList ImportLine(string str)
        {
            var list = new WordLibraryList();
            string[] words = str.Split(' ');
            for (int i = 1; i < words.Length; i++)
            {
                string word = words[i];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Count = DefaultRank;
                wl.SetCode(CodeType, words[0]);
                //wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(word));
                list.Add(wl);
            }
            return list;
        }
    }
}