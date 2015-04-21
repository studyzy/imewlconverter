using System;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.SOUGOU_PINYIN, ConstantString.SOUGOU_PINYIN_C, 10)]
    public class SougouPinyin : BaseImport, IWordLibraryExport, IWordLibraryTextImport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            //StringBuilder sb = new StringBuilder();

            string str = wl.GetPinYinString("'", BuildType.LeftContain) + " " + wl.Word;

            return str;
        }

        

        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                //sb.Append(" ");
                //sb.Append(wlList[i].Word);

                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public Encoding Encoding
        {
            get { return Encoding.GetEncoding("GBK"); }
        }

        #endregion

        #region IWordLibraryImport 成员

        public WordLibraryList ImportLine(string line)
        {
            if (line.IndexOf("'") == 0)
            {
                string py = line.Split(' ')[0];
                string word = line.Split(' ')[1];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Count = 1;
                wl.PinYin = py.Split(new[] {'\''}, StringSplitOptions.RemoveEmptyEntries);
                var wll = new WordLibraryList();
                wll.Add(wl);
                return wll;
            }
            return null;
        }

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
                if (line.IndexOf("'") == 0)
                {
                    wlList.AddWordLibraryList(ImportLine(line));
                }
            }
            return wlList;
        }

        #endregion
    }
}