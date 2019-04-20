using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Chinese-pyim
    /// http://tumashu.github.io/chinese-pyim
    /// </summary>
    [ComboBoxShow(ConstantString.CHINESE_PYIM, ConstantString.CHINESE_PYIM_C, 177)]
    public class ChinesePyim : BaseImport, IWordLibraryTextImport, IWordLibraryExport
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
            string[] lines = str.Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                wlList.AddWordLibraryList(ImportLine(line));
            }
            return wlList;
        }


        public WordLibraryList ImportLine(string line)
        {
            var array = line.Split(' ');
            var py = array[0].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            for (var i = 1; i < array.Length; i++)
            {
                string word = line.Split(' ')[i];
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = 1;
                wl.PinYin = py;

                wll.Add(wl);
            }
            return wll;
        }

        #endregion

        #region IWordLibraryExport 成员

        #region IWordLibraryExport Members

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append(wl.GetPinYinString("-", BuildType.None));
            sb.Append(" ");
            sb.Append(wl.Word);

            return sb.ToString();
        }


        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            sb.Append(";; -*- coding: utf-8 -*--\n");
   
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\n");
            }
            return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryTextImport Members

        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion

        #endregion
    }
}