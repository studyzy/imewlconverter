using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.Language;
using System.Linq;
using System.Timers;

namespace Studyzy.IMEWLConverter
{
    public delegate void ProcessNotice(string message);
    public class MainBody:IDisposable
    {
        public event ProcessNotice ProcessNotice;
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
        private Timer timer;


        public IList<string> ExportContents { get; set; } 
        public MainBody()
        {
            Filters = new List<ISingleFilter>();
            BatchFilters = new List<IBatchFilter>();
            selectedConverter = new SystemKernel();
            selectedTranslate = ChineseTranslate.NotTrans;
            wordRankGenerater = new DefaultWordRankGenerater();
            InitTimer();
        }
        /// <summary>
        /// 初始化Timer控件
        /// </summary>
        private void InitTimer()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 3000;
            timer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            timer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            timer.Enabled = true;
            //绑定Elapsed事件
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);
        }

        /// <summary>
        /// Timer类执行定时到点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ProcessNotice(this.processMessage);
//                this.Invoke(new SetControlValue(SetTextBoxText), currentCount.ToString());
            }
            catch (Exception ex)
            {
                ProcessNotice("执行定时到点事件失败:" + ex.Message);
            }
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

        public void StopNotice()
        {
            this.timer.Stop();
        }
        public void StartNotice()
        {
            this.timer.Start();
        }
        //public List<string> GetRealPath(IList<string> filePathes)
        //{
        //    var list = new List<string>();

        //    filePathes.ToList().ForEach(x =>
        //    {
        //        var dic = Path.GetDirectoryName(x);
        //        var filen = Path.GetFileName(x);
        //        if (filen.Contains("*"))
        //        {
        //            var files = Directory.GetFiles(dic, filen, SearchOption.AllDirectories);
        //            list.AddRange(files);
        //        }
        //        else
        //        {
        //            list.Add(x);
        //        }

        //    });


        //    return list;
        //}

        /// <summary>
        /// 转换多个文件成一个文件
        /// </summary>
        /// <param name="filePathes"></param>
        /// <returns></returns>
        public string Convert(IList<string> filePathes)
        {
            this.timer.Start();
            ExportContents=new List<string>();
            allWlList.Clear();
            isImportProgress = true;

            //filePathes = GetRealPath(filePathes);

            foreach (string file in filePathes)
            {
                WordLibraryList wlList = import.Import(file);
                wlList = Filter(wlList);
                allWlList.AddRange(wlList);

            }
            isImportProgress = false;
            if (selectedTranslate != ChineseTranslate.NotTrans)
            {
                ProcessNotice("开始繁简转换...");

                allWlList = ConvertChinese(allWlList);
            }
            if (export.CodeType != CodeType.NoCode)
            {
                ProcessNotice("开始生成词频...");
                GenerateWordRank(allWlList);
            }
            if (import.CodeType != export.CodeType)
            {
                ProcessNotice("开始生成目标编码...");

                GenerateDestinationCode(allWlList, export.CodeType);
            }
            count = allWlList.Count;
            if (export.CodeType != CodeType.NoCode)
            {
                allWlList = RemoveEmptyCodeData(allWlList);
            }

            ReplaceAfterCode(allWlList);
             ExportContents = export.Export(allWlList);
            this.timer.Stop();
            return string.Join("\r\n", ExportContents.ToArray());
        }

        private WordLibraryList RemoveEmptyCodeData(WordLibraryList wordLibraryList)
        {
            var list = new WordLibraryList();
            foreach (WordLibrary wordLibrary in wordLibraryList)
            {

                if (!string.IsNullOrEmpty(wordLibrary.SingleCode))//没有编码，则不保留
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
                if (wordLibrary.Rank == 0|| wordRankGenerater.ForceUse )
                {
                    wordLibrary.Rank = wordRankGenerater.GetRank(wordLibrary.Word);
                }
                currentStatus++;
                processMessage = "生成词频：" + currentStatus + "/" + countWord;
            }
        }

        private void GenerateDestinationCode(WordLibraryList wordLibraryList, CodeType codeType)
        {
            if (wordLibraryList.Count == 0) return;
            if(wordLibraryList[0].CodeType== CodeType.NoCode&& codeType == CodeType.UserDefinePhrase)
            {
                codeType = CodeType.Pinyin;
            }
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
                if (wordLibrary.CodeType == CodeType.English)
                {
                    wordLibrary.SetCode(CodeType.English, wordLibrary.Word.ToLower());
                    continue;
                }
                try
                {
                    generater.GetCodeOfWordLibrary(wordLibrary);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("生成编码失败"+ex.Message);
                }
                if (codeType != CodeType.Unknown)
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
            this.timer.Start();
            ExportContents=new List<string>();
            int c = 0;

            //filePathes = GetRealPath(filePathes);
            int fileCount = filePathes.Count;
            var fileProcessed = 0;
            foreach (string file in filePathes)
            {
                fileProcessed++;
                DateTime start = DateTime.Now;
                try
                {
                    WordLibraryList wlList = import.Import(file);
                    wlList = Filter(wlList);
                    if (selectedTranslate != ChineseTranslate.NotTrans)
                    {
                        wlList = ConvertChinese(wlList);
                    }
                    c += wlList.Count;
                    GenerateWordRank(wlList);
                    wlList= RemoveEmptyCodeData(wlList);
                    ReplaceAfterCode(wlList);
                    ExportContents = export.Export(wlList);
                    for (var i = 0; i < ExportContents.Count; i++)
                    {
                        string exportPath = outputDir + (outputDir.EndsWith("\\") ? "" : "\\") +
                                            Path.GetFileNameWithoutExtension(file) + (i == 0 ? "" : i.ToString()) +
                                            ".txt";
                        FileOperationHelper.WriteFile(exportPath, export.Encoding, ExportContents[i]);
                    }
                    var costSeconds = (DateTime.Now - start).TotalSeconds;
                    ProcessNotice?.Invoke(fileProcessed + "/" + fileCount + "\t" + Path.GetFileName(file) + "\t转换完成，耗时：" +
                                          costSeconds + "秒\r\n");
                }
                catch (Exception ex)
                {
                    ProcessNotice?.Invoke(fileProcessed + "/" + fileCount + "\t" + Path.GetFileName(file) + "\t处理时发生异常：" +
                                         ex.Message + "\r\n");
                }
            }
            count = c;
            this.timer.Stop();
        }
        public void ExportToFile(string filePath)
        {
            var outputDir = Path.GetDirectoryName(filePath);
            for (var i = 0; i < ExportContents.Count; i++)
            {
                string exportPath = outputDir + (outputDir.EndsWith("\\") ? "" : "\\") +
                               Path.GetFileNameWithoutExtension(filePath) + (i == 0 ? "" : i.ToString()) + ".txt";
                FileOperationHelper.WriteFile(exportPath, export.Encoding, ExportContents[i]);
            }
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
        private void ReplaceAfterCode(WordLibraryList list)
        {
            foreach (WordLibrary wordLibrary in list)
            {
                if (ReplaceFilters != null)
                    foreach (IReplaceFilter replaceFilter in ReplaceFilters)
                    {
                        if (replaceFilter.ReplaceAfterCode)
                            replaceFilter.Replace(wordLibrary);
                    }
            }
        }
        private WordLibraryList Filter(WordLibraryList list)
        {
            var result = new WordLibraryList();
            foreach (WordLibrary wordLibrary in list)
            {
                if (IsKeep(wordLibrary))
                {
                    if (ReplaceFilters != null)
                        foreach (IReplaceFilter replaceFilter in ReplaceFilters)
                        {
                            if (!replaceFilter.ReplaceAfterCode)
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

        public void Dispose()
        {
            this.timer.Stop();
        }
    }
}
