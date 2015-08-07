using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.Language;

namespace Studyzy.IMEWLConverter
{
    internal class MainBody
    {
        private WordLibraryList allWlList = new WordLibraryList();
        private int count;
        private int countWord;
        private int currentStatus;
        private IWordLibraryExport export;
        private IWordLibraryImport import;
        private bool isImportProgress;
        private string processMessage;
        private IChineseConverter selectedConverter;
        private ChineseTranslate selectedTranslate;
        private IWordRankGenerater wordRankGenerater;

        public MainBody()
        {
            Filters = new List<ISingleFilter>();
            BatchFilters = new List<IBatchFilter>();
            selectedConverter = new SystemKernel();
            selectedTranslate = ChineseTranslate.NotTrans;
            wordRankGenerater = new DefaultWordRankGenerater();
        }

        public int CurrentStatus
        {
            get
            {
                if (isImportProgress)
                {
                    return import.CurrentStatus;
                }
                return currentStatus;
            }
            set { currentStatus = value; }
        }

        public int CountWord
        {
            get
            {
                if (isImportProgress)
                {
                    return import.CountWord;
                }
                return countWord;
            }
            set { countWord = value; }
        }

        /// <summary>
        ///     进度信息
        /// </summary>
        public string ProcessMessage
        {
            get
            {
                if (isImportProgress)
                {
                    return "转换进度：" + CurrentStatus + "/" + CountWord;
                }
                return processMessage;
            }
            set { processMessage = value; }
        }

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

        public IWordRankGenerater SelectedWordRankGenerater
        {
            get { return wordRankGenerater; }
            set { wordRankGenerater = value; }
        }

        public ChineseTranslate SelectedTranslate
        {
            get { return selectedTranslate; }
            set { selectedTranslate = value; }
        }

        public int Count
        {
            get { return count; }
        }

        public IList<IReplaceFilter> ReplaceFilters { get; set; }

        public IList<ISingleFilter> Filters { get; set; }

        public IList<IBatchFilter> BatchFilters { get; set; }

        /// <summary>
        ///     转换多个文件成一个文件
        /// </summary>
        /// <param name="filePathes"></param>
        /// <returns></returns>
        public string Convert(IList<string> filePathes)
        {
            allWlList.Clear();
            isImportProgress = true;
            foreach (string file in filePathes)
            {
                WordLibraryList wlList = import.Import(file);
                wlList = Filter(wlList);
                allWlList.AddRange(wlList);
            }
            isImportProgress = false;
            if (selectedTranslate != ChineseTranslate.NotTrans)
            {
                allWlList = ConvertChinese(allWlList);
            }
            GenerateWordRank(allWlList);
            if (import.CodeType != export.CodeType)
            {
                GenerateDestinationCode(allWlList, export.CodeType);
            }
            count = allWlList.Count;
            return export.Export(RemoveEmptyCodeData(allWlList));
        }

        private WordLibraryList RemoveEmptyCodeData(WordLibraryList wordLibraryList)
        {
            var list = new WordLibraryList();
            foreach (WordLibrary wordLibrary in wordLibraryList)
            {
                if (!string.IsNullOrEmpty(wordLibrary.SingleCode))
                {
                    list.Add(wordLibrary);
                }
            }
            return list;
        }

        private void GenerateWordRank(WordLibraryList wordLibraryList)
        {
            countWord = wordLibraryList.Count;
            currentStatus = 0;
            foreach (WordLibrary wordLibrary in wordLibraryList)
            {
                if (wordLibrary.Rank == 0)
                {
                    wordLibrary.Rank = wordRankGenerater.GetRank(wordLibrary.Word);
                }
                currentStatus++;
                processMessage = "生成词频：" + currentStatus + "/" + countWord;
            }
        }

        private void GenerateDestinationCode(WordLibraryList wordLibraryList, CodeType codeType)
        {
            IWordCodeGenerater generater = CodeTypeHelper.GetGenerater(codeType);
            if (generater == null) //未知编码方式，则不进行编码。
                return;
            countWord = wordLibraryList.Count;
            currentStatus = 0;
            foreach (WordLibrary wordLibrary in wordLibraryList)
            {
                currentStatus++;
                processMessage = "生成目标编码：" + currentStatus + "/" + countWord;
                if (wordLibrary.CodeType == codeType)
                {
                    continue;
                }
                generater.GetCodeOfWordLibrary(wordLibrary);
                if(codeType!=CodeType.Unknown)
                wordLibrary.CodeType = codeType;
            }
        }

        /// <summary>
        ///     转换多个文件为对应文件名的多个文件
        /// </summary>
        /// <param name="filePathes"></param>
        /// <param name="outputDir"></param>
        public void Convert(IList<string> filePathes, string outputDir)
        {
            int c = 0;
            foreach (string file in filePathes)
            {
                WordLibraryList wlList = import.Import(file);
                wlList = Filter(wlList);
                if (selectedTranslate != ChineseTranslate.NotTrans)
                {
                    wlList = ConvertChinese(wlList);
                }
                c += wlList.Count;
                GenerateWordRank(wlList);
                string result = export.Export(RemoveEmptyCodeData(wlList));
                string exportPath = outputDir + (outputDir.EndsWith("\\") ? "" : "\\") +
                                    Path.GetFileNameWithoutExtension(file) + ".txt";
                FileOperationHelper.WriteFile(exportPath, export.Encoding, result);
            }
            count = c;
        }

        public void StreamConvert(IList<string> filePathes, string outPath)
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
                wlStream.ConvertWordLibrary(w => IsKeep(w));
            }

            stream.Close();
        }

        private WordLibraryList Filter(WordLibraryList list)
        {
            var result = new WordLibraryList();
            foreach (WordLibrary wordLibrary in list)
            {
                if (IsKeep(wordLibrary))
                {
                    foreach (IReplaceFilter replaceFilter in ReplaceFilters)
                    {
                        replaceFilter.Replace(wordLibrary);
                    }
                    if (wordLibrary.Word != string.Empty)
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
            string[] newList = result.Split(new[] {'\r'}, StringSplitOptions.RemoveEmptyEntries);
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