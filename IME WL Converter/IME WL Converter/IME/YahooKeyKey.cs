using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 雅虎奇摩输入法
    /// </summary>
    [ComboBoxShow(ConstantString.YAHOO_KEYKEY, ConstantString.YAHOO_KEYKEY_C, 110)]
    public class YahooKeyKey : BaseImport, IWordLibraryExport, IWordLibraryTextImport
    {
        private static Regex regex = new Regex(@"^[a-zA-Z]+\d$");
        #region IWordLibraryExport 成员
        private IWordCodeGenerater generater=new ZhuyinGenerater();
        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("\t");
            IList<string> zhuyins = null;
            if (wl.OtherCode.Key == CodeType.Pinyin)//如果本来就是拼音输入法导入的，那么就用其拼音，不过得加上音调
            {
                IList<string> pinyin=new List<string>();
                for (var i = 0; i < wl.PinYin.Length; i++)
                {
                    if (regex.IsMatch(wl.PinYin[i]))
                    {
                        pinyin.Add(wl.PinYin[i]);
                    }
                    else
                    {
                        pinyin.Add(PinyinHelper.AddToneToPinyin(wl.Word[i], wl.PinYin[i]));

                    }
                }
                zhuyins = ZhuyinHelper.GetZhuyin(pinyin);
            }
            else
            {
                zhuyins = generater.GetCodeOfString(wl.Word);
            }
            sb.Append(CollectionHelper.ListToString( zhuyins,","));
            sb.Append("\t");
            sb.Append("-1.0");
            sb.Append("\t");
            sb.Append("0.0");
            return sb.ToString();
        }

        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                try
                {
                    sb.Append(ExportLine(wlList[i]));
                    sb.Append("\r\n");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(wlList[i]+ ex.Message);
                }
            }
            return sb.ToString();
        }

        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion

        #region IWordLibraryImport 成员

      

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] lines = str.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                wlList.AddWordLibraryList(ImportLine(line));
            }
            return wlList;
        }

     


        public WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split('\t');
            var wl = new WordLibrary();
            wl.Word = c[0];
            wl.Count = Convert.ToInt32(c[1]);
            wl.PinYin = c[2].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}