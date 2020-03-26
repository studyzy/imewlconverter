using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.BING_PINYIN, ConstantString.BING_PINYIN_C, 135)]
    public class BingPinyin : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append(" ");
            sb.Append(wl.GetPinYinString(" ", BuildType.None));

            return sb.ToString();
        }


        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }
            return new List<string>() { sb.ToString() };
        }

     

        #endregion

        public override WordLibraryList ImportLine(string line)
        {
            if (line.Length > 0 && line[0] == ';')
                return null;
            string[] sp = line.Split(' ');

            string word = sp[0];
            var py = new string[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                py[i] = sp[i + 1];
            }
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = 1;
            wl.PinYin = py;
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }
    }
}