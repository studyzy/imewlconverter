using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.Language;

namespace Studyzy.IMEWLConverter
{
    class MainBody
    {
        public MainBody()
        {
            Filters = new List<ISingleFilter>();
            selectedConverter = new SystemKernel();
            selectedTranslate = ChineseTranslate.NotTrans;

        }

        private IWordLibraryImport import;
        private IWordLibraryExport export;
        private IChineseConverter selectedConverter;
        private ChineseTranslate selectedTranslate;
        public IWordLibraryImport Import
        {
            get { return import; }
            set { import = value; }
        }

        public IWordLibraryExport Export
        {
            get { return export; }
            set { export = value; }
        }

        public IChineseConverter SelectedConverter
        {
            get { return selectedConverter; }
            set { selectedConverter = value; }
        }

        public ChineseTranslate SelectedTranslate
        {
            get { return selectedTranslate; }
            set { selectedTranslate = value; }
        }

        private int count;
        public int Count
        {
            get { return count; }
        }


        public IList<ISingleFilter> Filters { get; set; }
        private WordLibraryList allWlList = new WordLibraryList();
        public string Convert(IList<string> filePathes)
        {
            allWlList.Clear();
            foreach (string file in filePathes)
            {
                WordLibraryList wlList = import.Import(file);
                wlList = Filter(wlList);
                allWlList.AddRange(wlList);
            }
            if (selectedTranslate != ChineseTranslate.NotTrans)
            {
              
                allWlList = ConvertChinese(allWlList);
              
            }
            count = allWlList.Count;
            return export.Export(allWlList);
        }
        public void StreamConvert(IList<string> filePathes,string outPath)
        {
            var textImport = import as IWordLibraryTextImport;
            if (textImport == null)
            {
                throw new Exception("流转换,只有文本类型的才支持。");
            }
            StreamWriter stream = FileOperationHelper.GetWriteFileStream(outPath, export.Encoding);
            foreach (string filePath in filePathes)
            {
                var wlStream = new WordLibraryStream(import, export, filePath, textImport.Encoding, stream);
                //wlStream.ConvertWordLibrary(WordFilterRetain);
            }
       
            stream.Close();
        }
        private WordLibraryList Filter(WordLibraryList list)
        {
            WordLibraryList result=new WordLibraryList();
            foreach (WordLibrary wordLibrary in list)
            {
                if (IsKeep(wordLibrary))
                {
                    result.Add(wordLibrary);
                }
            }
            return result;
        }
        private bool IsKeep(WordLibrary wordLibrary)
        {
            foreach (ISingleFilter filter in Filters)
            {
                if (!filter.IsKeep(wordLibrary))
                {
                    return false;
                }
            }
            return true;
        }
        private WordLibraryList ConvertChinese(WordLibraryList wordLibraryList)
        {
            var sb = new StringBuilder();
            int count = wordLibraryList.Count;
            foreach (WordLibrary wordLibrary in wordLibraryList)
            {
                sb.Append(wordLibrary.Word + "\r");
            }
            string result = "";
            if (selectedTranslate == ChineseTranslate.Trans2Chs)
            {
                result = selectedConverter.ToChs(sb.ToString());
            }
            else if (selectedTranslate == ChineseTranslate.Trans2Cht)
            {
                result = selectedConverter.ToCht(sb.ToString());
            }
            string[] newList = result.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (newList.Length != count)
            {
                throw new Exception("简繁转换时转换失败，请更改简繁转换设置");
            }
            for (int i = 0; i < count; i++)
            {
                WordLibrary wordLibrary = wordLibraryList[i];
                wordLibrary.Word = newList[i];
            }
            return wordLibraryList;
        }
    }
}
