using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.RIME, ConstantString.RIME_C, 150)]
    public class Rime : BaseImport, IWordLibraryTextImport, IWordLibraryExport, IMultiCodeType
    {
        private string lineSplitString;
      

        private OperationSystem os;
        public OperationSystem OS
        {
            get { return os; }
            set
            {
                os = value;
                lineSplitString = GetLineSplit(os);
            } 
        }

        private string GetLineSplit(OperationSystem os)
        {
            switch (os)
            {
                case OperationSystem.Windows:
                    return "\r\n";

                case OperationSystem.MacOS:
                    return "\r";

                case OperationSystem.Linux:
                    return "\n";
            }
            return "\r\n";
        }

        #region IWordLibraryExport 成员

        private IWordCodeGenerater codeGenerater;
        //private RimeConfigForm form;
        public Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            if (codeGenerater == null)
            {
                codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
            }
            if (codeGenerater.Is1CharMutiCode)
            {
                IList<string> codes = codeGenerater.GetCodeOfString(wl.Word);
                int i = 0;
                foreach (string code in codes)
                {
                    sb.Append(wl.Word);
                    sb.Append("\t");
                    sb.Append(code);
                    sb.Append("\t");
                    sb.Append(wl.Rank);
                    i++;
                    if (i != codes.Count)
                        sb.Append(lineSplitString);
                }
            }
            else
            {
                sb.Append(wl.Word);
                sb.Append("\t");
                if (CodeType == CodeType.Pinyin||CodeType==CodeType.TerraPinyin)
                {
                    sb.Append(wl.GetPinYinString(" ", BuildType.None));
                }
                else if (CodeType == wl.CodeType)
                {
                    sb.Append(wl.Codes[0][0]);
                }
                else
                {
                    if (codeGenerater.Is1Char1Code)
                    {
                        sb.Append(CollectionHelper.ListToString(codeGenerater.GetCodeOfString(wl.Word), " "));
                    }
                    else
                    {
                        sb.Append(CollectionHelper.ListToString(codeGenerater.GetCodeOfString(wl.Word)));
                    }
                }
                sb.Append("\t");
                sb.Append(wl.Rank);
            }
            return sb.ToString();
        }

        
        public string Export(WordLibraryList wlList)
        {
            codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append(lineSplitString);
            }
            return sb.ToString();
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
            string[] lines = str.Split(new[] {"\r","\n"}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                if (line.StartsWith("#"))
                {
                    continue;
                }
                wlList.AddWordLibraryList(ImportLine(line));
            }
            return wlList;
        }

        //private IWordCodeGenerater pyGenerater=new PinyinGenerater();
        public WordLibraryList ImportLine(string line)
        {
            string[] lineArray = line.Split('\t');

            string word = lineArray[0];
            string code = lineArray[1];
            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = Convert.ToInt32(lineArray[2]);
            if (CodeType == CodeType.Pinyin)
            {
                wl.PinYin = code.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                //wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(wl.Word));
                wl.SetCode(CodeType, code);
            }


            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}