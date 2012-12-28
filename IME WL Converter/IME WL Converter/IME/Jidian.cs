using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 极点的词库格式为“编码 词语 词语 词语”\r\n
    /// </summary>
    [ComboBoxShow(ConstantString.JIDIAN, ConstantString.JIDIAN_C, 190)]
    public class Jidian : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        protected virtual bool IsWubi {get { return false; } }

        #region IWordLibraryImport 成员

        private readonly IWordCodeGenerater pinyinFactory = new PinyinGenerater();

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
                wlList.AddWordLibraryList(ImportLine(line));
                CurrentStatus = i;
            }
            return wlList;
        }

        public virtual WordLibraryList ImportLine(string line)
        {
            var wlList = new WordLibraryList();
            string[] strs = line.Split(' ');

            for (int i = 1; i < strs.Length; i++)
            {
                string word = strs[i].Replace("，", ""); //把汉字中带有逗号的都去掉逗号
                var list = pinyinFactory.GetCodeOfString(word);
                for (int j = 0; j < list.Count; j++)
                {
                    var wl = new WordLibrary();
                    wl.Word = word;
                    if (IsWubi)
                    {
                        wl.AddCode(CodeType.Wubi, strs[0]);
                    }
                    wl.PinYin = CollectionHelper.ToArray(list);
                    wlList.Add(wl);
                }
            }
            return wlList;
        }



        #endregion


        #region IWordLibraryExport 成员

        private readonly IWordCodeGenerater wubiFactory = new PinyinGenerater();

        public virtual string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            if(string.IsNullOrEmpty(wl.WubiCode))
            {
                sb.Append(wubiFactory.GetCodeOfString(wl.Word)[0]);
            }
            else
            {
                sb.Append(wl.WubiCode);
            }
            sb.Append(" ");
            sb.Append(wl.Word);

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