using System;
using System.Text;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.SINA_PINYIN, ConstantString.SINA_PINYIN_C, 180)]
    public class SinaPinyin : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        #region IWordLibraryExport Members

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.GetPinYinString("'", BuildType.None));
            sb.Append("\t");
            sb.Append(wl.Word);
            return sb.ToString();
        }

        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\n");
            }
            return sb.ToString();
        }

        #endregion

        #region IWordLibraryTextImport Members

        public Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        #endregion

        #endregion

        #region IWordLibraryImport 成员

     

        public WordLibraryList ImportLine(string line)
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

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] lines = str.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                CurrentStatus = i;
                wlList.AddWordLibraryList(ImportLine(line));
            }
            return wlList;
        }

        #endregion
    }
}