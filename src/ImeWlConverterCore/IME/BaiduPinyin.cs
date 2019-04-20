using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     百度PC输入法，中文词库和英文词库放在同一个文件，中文词库比如“跨年	kua'nian'	1”，英文词库比如“Jira	1”
    /// </summary>
    [ComboBoxShow(ConstantString.BAIDU_PINYIN, ConstantString.BAIDU_PINYIN_C, 90)]
    public class BaiduPinyin : BaseImport, IWordLibraryTextImport, IWordLibraryExport
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
            var wl = new WordLibrary();
            string[] array = line.Split('\t');

            wl.Word = array[0];
            if (array.Length == 2) //English
            {
                wl.IsEnglish = true;
                wl.Rank = Convert.ToInt32(array[1]);
            }
            else
            {
                string py = line.Split('\t')[1];
                wl.PinYin = py.Split(new[] {'\''}, StringSplitOptions.RemoveEmptyEntries);
                wl.Rank = Convert.ToInt32(array[2]);
            }

            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion

        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("\t");
            if (!wl.IsEnglish)
            {
                sb.Append(wl.GetPinYinString("'", BuildType.RightContain));
                sb.Append("\t");
            }
            sb.Append(wl.Rank);
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

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion
    }
}