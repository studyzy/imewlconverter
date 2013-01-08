using System;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.QQ_PINYIN, ConstantString.QQ_PINYIN_C, 50)]
    public class QQPinyin : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            try
            {
                var py = wl.GetPinYinString("'", BuildType.None);
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
            sb.Append(" ");
            sb.Append(wl.Word);
            sb.Append(" ");
            sb.Append(wl.Count);
            return sb.ToString();
        }

        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count - 1; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                    sb.Append("\r\n");
                }
            }
            WordLibrary last = wlList[wlList.Count - 1];
            sb.Append(ExportLine(last));
            sb.Append(", ");
            sb.Append(last.GetPinYinString("'", BuildType.None));
            sb.Append(" ");
            sb.Append(last.Count);
            sb.Append("\r\n");
            return sb.ToString();
        }

        #endregion

        public WordLibraryList ImportLine(string line)
        {
            line = line.Split(',')[0]; //如果有逗号，就只取第一个
            string[] sp = line.Split(' ');
            string py = sp[0];
            string word = sp[1];
            int count = Convert.ToInt32(sp[2]);
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Count = count;
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
    }
}