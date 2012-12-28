using System;
using System.Text;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.ZIGUANG_PINYIN, ConstantString.ZIGUANG_PINYIN_C, 170)]
    public class ZiGuangPinyin : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
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
            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                wlList.AddWordLibraryList(ImportLine(line));
            }
            return wlList;
        }

    

        public WordLibraryList ImportLine(string line)
        {
            string py = line.Split('\t')[1];
            string word = line.Split('\t')[0];
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Count = 1;
            wl.PinYin = py.Split(new[] {'\''}, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion

        #region IWordLibraryExport 成员

        #region IWordLibraryExport Members

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("\t");
            sb.Append(wl.GetPinYinString("'", BuildType.None));
            sb.Append("\t100000");

            return sb.ToString();
        }

        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            sb.Append("名称=用户词库\r\n");
            sb.Append("作者=深蓝词库转换\r\n");
            sb.Append("编辑=1\r\n\r\n");
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        #endregion

        #region IWordLibraryTextImport Members

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion

        #endregion
    }
}