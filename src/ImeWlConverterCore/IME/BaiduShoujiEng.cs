using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     百度手机输入法支持单独的英语词库，格式“单词Tab词频”
    /// </summary>
    [ComboBoxShow(ConstantString.BAIDU_SHOUJI_ENG, ConstantString.BAIDU_SHOUJI_ENG_C, 1010)]
    public class BaiduShoujiEng : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            return wl.Word + "\t" + (54999 + wl.Rank);
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

   

        public override CodeType CodeType
        {
            get { return CodeType.English; }
        }

        #endregion
        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
        #region IWordLibraryImport 成员



        public override WordLibraryList ImportLine(string line)
        {
            string[] wp = line.Split('\t');

            string word = wp[0];
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = Convert.ToInt32(wp[1]);
            wl.PinYin = new string[] {};
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}