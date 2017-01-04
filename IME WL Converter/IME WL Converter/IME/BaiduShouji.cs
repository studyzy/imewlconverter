using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.BAIDU_SHOUJI, ConstantString.BAIDU_SHOUJI_C, 1000)]
    public class BaiduShouji : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员
        /// <summary>
        /// 百度手机的格式为 中文(pin|yin) 词频
        /// </summary>
        /// <param name="wl"></param>
        /// <returns></returns>
        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("(");
            sb.Append(wl.GetPinYinString("|", BuildType.None));
            sb.Append(") 20000");

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
            var wll = new WordLibraryList();
            try
            {
                var array1 = line.Split('(');
                string word = array1[0];
                string py = array1[1].Split(')')[0];

                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = 1;
                wl.PinYin = py.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                wll.Add(wl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(line + "\t" + ex.Message);
            }
            return wll;
        }

        #endregion
    }
}