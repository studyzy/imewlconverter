using System;
using System.Text;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.RIME, ConstantString.RIME_C, 150)]
    public class Rime : BaseImport, IWordLibraryTextImport, IWordLibraryExport, IMultiCodeType
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            if (codeGenerater.Is1CharMutiCode)
            {
                var codes = codeGenerater.GetCodeOfString(wl.Word);
                int i = 0;
                foreach (var code in codes)
                {
                    sb.Append(wl.Word);
                    sb.Append("\t");
                    sb.Append(code);
                    sb.Append("\t");
                    sb.Append(wl.Count);
                    i++;
                    if (i != codes.Count)
                        sb.Append("\r\n");
                }
            }
            else
            {
                sb.Append(wl.Word);
                sb.Append("\t");
                if (CodeType == CodeType.Pinyin)
                {
                    sb.Append(wl.GetPinYinString(" ", BuildType.None));
                }
                else if (CodeType == wl.OtherCode.Key)
                {
                    sb.Append(wl.OtherCode.Value[0][0]);
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
                sb.Append(wl.Count);
            }
            return sb.ToString();
        }

        private IWordCodeGenerater codeGenerater;
        public string Export(WordLibraryList wlList)
        {
            codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
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
            get { return new UTF8Encoding(false); }
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
            wl.Count = Convert.ToInt32(lineArray[2]);
            if (CodeType == CodeType.Pinyin)
            {
                wl.PinYin = code.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                //wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(wl.Word));
                wl.AddCode(CodeType, code);
            }
          

            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}