using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.LIBPINYIN, ConstantString.LIBPINYIN_C, 175)]
    public class Libpinyin : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append(wl.Word);
            sb.Append(" ");
            try
            {
                string py = wl.GetPinYinString("'", BuildType.None);
                if (string.IsNullOrEmpty(py))
                {
                    return "";
                }
                sb.Append(py);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return sb.ToString();
        }


        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count - 1; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                    sb.Append("\n");
                }
            }

            return new List<string>() { sb.ToString() };
        }

        public Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }

        #endregion

        public WordLibraryList ImportLine(string line)
        {
            string[] sp = line.Split(' ');
            string py = sp[1];
            string word = sp[0];

            var wl = new WordLibrary {CodeType = CodeType.Pinyin};
            wl.Word = word;
            wl.Rank = DefaultRank;
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
    }
}