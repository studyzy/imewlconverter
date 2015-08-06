using System;
using System.Text;
using System.Collections.Generic;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 超音速录输入法
    /// </summary>
	[ComboBoxShow(ConstantString.CHAO_YIN, ConstantString.CHAO_YIN_C, 190)]
    internal class Chaoyin : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {

		private static IDictionary<string,string> pinyinCodeMapping;

		private  IDictionary<string,string> PinyinCodeMapping
		{
			get{
				if (pinyinCodeMapping == null)
				{
					pinyinCodeMapping = new Dictionary<string,string> ();
					//todo
				}
				return pinyinCodeMapping;
			}
		}

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


        public string Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion
    }
}