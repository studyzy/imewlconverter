using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.WORD_ONLY, ConstantString.WORD_ONLY_C, 2010)]
    public class NoPinyinWordOnly : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        //private IWordCodeGenerater pinyinFactory;
        public override CodeType CodeType
        {
            get { return CodeType.NoCode; }
        }

        #region IWordLibraryImport 成员

        /// <summary>
        ///     将一行纯文本转换为对象
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public override WordLibraryList ImportLine(string line)
        {
            //IList<string> py = pinyinFactory.GetCodeOfString(line);
            var wl = new WordLibrary();
            wl.Word = line;
            wl.CodeType = CodeType;
            //wl.PinYin = CollectionHelper.ToArray(py);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

     

        #endregion

        #region IWordLibraryExport 成员

        #region IWordLibraryExport Members

        public virtual string ExportLine(WordLibrary wl)
        {
            return wl.Word;
        }


        public virtual IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(wlList[i].Word);
                sb.Append("\r\n");
            }
           
            return new List<string>() { sb.ToString()};
        }

        #endregion

        #region IWordLibraryTextImport Members

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion

        #endregion
    }
}