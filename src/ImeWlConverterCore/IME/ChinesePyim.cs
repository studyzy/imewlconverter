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
    public class ChinesePyim : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryImport 成员

     

        public override WordLibraryList ImportLine(string line)
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

   
        #endregion

        #endregion     
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

    }
}