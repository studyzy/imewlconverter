using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 以前叫紫光输入法，现在改名叫华宇拼音输入法
    /// </summary>
    [ComboBoxShow(ConstantString.ZIGUANG_PINYIN, ConstantString.ZIGUANG_PINYIN_C, 170)]
    public class ZiGuangPinyin : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryImport 成员



        public override WordLibraryList ImportLine(string line)
        {
            string py = line.Split('\t')[1];
            string word = line.Split('\t')[0];
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = 1;
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


        public IList<string> Export(WordLibraryList wlList)
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
            return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryTextImport Members

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion

        #endregion
    }
}