using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.CANGJIE_PLATFORM, ConstantString.CANGJIE_PLATFORM_C, 230)]
    public class CangjiePlatform : BaseImport, IWordLibraryExport, IWordLibraryTextImport
    {
     
        public override CodeType CodeType
        {
            get
            {
                return CodeType.Cangjie;
            }
        }
        #region IWordLibraryExport 成员

        //private readonly IWordCodeGenerater codeGenerater = new Cangjie5Generater();

        public Encoding Encoding
        {
            get { return Encoding.GetEncoding("GBK"); }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            IList<string> codes = wl.Codes[0];// codeGenerater.GetCodeOfString(wl.Word);
            int i = 0;
            foreach (string code in codes)
            {
                sb.Append(code);
                sb.Append(" ");
                sb.Append(wl.Word);
                i++;
                if (i != codes.Count)
                    sb.Append("\r\n");
            }
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

        #endregion

        #region IWordLibraryImport 成员

        private readonly IWordCodeGenerater pyGenerater = new PinyinGenerater();

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
            string[] c = line.Split(' ');
            var wl = new WordLibrary();
            string code = c[0];
            wl.Word = c[1];
            wl.Count = DefaultRank;
            wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(wl.Word));
            wl.SetCode(CodeType, code);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}