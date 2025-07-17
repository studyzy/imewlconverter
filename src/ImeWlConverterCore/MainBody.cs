/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.Language;

namespace Studyzy.IMEWLConverter;

public class MainBody : IDisposable
{
    private static readonly Regex Num2ChsRegex = new("[1-9].+(0{2,100})");
    private int countWord;
    private int currentStatus;
    private bool isImportProgress;
    private string processMessage;
    private Timer timer;

    public MainBody()
    {
        Filters = new List<ISingleFilter>();
        FilterConfig = new FilterConfig();
        BatchFilters = new List<IBatchFilter>();
        SelectedConverter = new SystemKernel();
        SelectedTranslate = ChineseTranslate.NotTrans;
        SelectedWordRankGenerater = new DefaultWordRankGenerater();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        InitTimer();
    }

    public IList<string> ExportContents { get; set; }

    public int CurrentStatus
    {
        get
        {
            if (isImportProgress) return Import.CurrentStatus;
            return currentStatus;
        }
        set => currentStatus = value;
    }

    public int CountWord
    {
        get
        {
            if (isImportProgress) return Import.CountWord;
            return countWord;
        }
        set => countWord = value;
    }

    /// <summary>
    ///     进度信息
    /// </summary>
    public string ProcessMessage
    {
        get
        {
            if (isImportProgress) return "转换进度：" + CurrentStatus + "/" + CountWord;
            return processMessage;
        }
        set => processMessage = value;
    }

    public IWordLibraryImport Import { get; set; }

    public IWordLibraryExport Export { get; set; }

    public IChineseConverter SelectedConverter { get; set; }

    public IWordRankGenerater SelectedWordRankGenerater { get; set; }

    public ChineseTranslate SelectedTranslate { get; set; }

    public int Count { get; private set; }

    public IList<IReplaceFilter> ReplaceFilters { get; set; }

    public FilterConfig FilterConfig { get; set; }

    public IList<ISingleFilter> Filters { get; set; }
    public SortType SortType { get; set; }
    public bool SortDesc { get; set; }
    public IList<IBatchFilter> BatchFilters { get; set; }

    public void Dispose()
    {
        timer.Stop();
    }

    public event Action<string> ProcessNotice;

    /// <summary>
    ///     初始化Timer控件
    /// </summary>
    private void InitTimer()
    {
        //设置定时间隔(毫秒为单位)
        var interval = 3000;
        timer = new Timer(interval);
        //设置执行一次（false）还是一直执行(true)
        timer.AutoReset = true;
        //设置是否执行System.Timers.Timer.Elapsed事件
        timer.Enabled = true;
        //绑定Elapsed事件
        timer.Elapsed += TimerUp;
    }

    /// <summary>
    ///     Timer类执行定时到点事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimerUp(object sender, ElapsedEventArgs e)
    {
        if (string.IsNullOrEmpty(processMessage))
            return;
        try
        {
            ProcessNotice(processMessage);
        }
        catch (Exception ex)
        {
            ProcessNotice("执行定时到点事件失败:" + ex.Message);
        }
    }

    public void StopNotice()
    {
        timer.Stop();
    }

    public void StartNotice()
    {
        timer.Start();
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
    ///     转换多个文件成一个文件
    /// </summary>
    /// <param name="filePathes"></param>
    /// <returns></returns>
    public string Convert(IList<string> filePathes)
    {
        var allWlList = new WordLibraryList();

        timer.Start();
        ExportContents = new List<string>();
        isImportProgress = true;

        //filePathes = GetRealPath(filePathes);

        foreach (var file in filePathes)
        {
            if (FileOperationHelper.GetFileSize(file) == 0)
            {
                ProcessNotice("词库（" + Path.GetFileName(file) + "）为空，请检查");
                continue;
            }

            Debug.WriteLine("start process file:" + file);
            try
            {
                var wlList = Import.Import(file);
                wlList = Filter(wlList);
                allWlList.AddRange(wlList);
            }
            catch (Exception ex)
            {
                ProcessNotice("词库（" + Path.GetFileName(file) + "）处理出现异常：\n\t" + ex.Message);
                isImportProgress = false;
                timer.Stop();
                return "";
            }
        }

        isImportProgress = false;
        if (SelectedTranslate != ChineseTranslate.NotTrans)
        {
            ProcessNotice("开始繁简转换...");

            allWlList = ConvertChinese(allWlList);
        }

        if (Export.CodeType != CodeType.NoCode)
        {
            ProcessNotice("开始生成词频...");
            GenerateWordRank(allWlList);
        }

        if (Import.CodeType != Export.CodeType)
        {
            ProcessNotice("开始生成目标编码...");

            GenerateDestinationCode(allWlList, Export.CodeType);
        }

        if (Export.CodeType != CodeType.NoCode) allWlList = RemoveEmptyCodeData(allWlList);
        Count = allWlList.Count;

        ReplaceAfterCode(allWlList);
        //Sort
        //var wlDict = new Dictionary<string, WordLibrary>();
        //var sorted = allWlList.Distinct().OrderBy(w => w.PinYinString).ToList();
        //allWlList = new WordLibraryList();
        //foreach (var wl in sorted)
        //{
        //    if (!wlDict.ContainsKey(wl.Word))
        //    {
        //        wlDict.Add(wl.Word, wl);
        //        allWlList.Add(wl);
        //    }
        //}
        ExportContents = Export.Export(allWlList);

        timer.Stop();

        return string.Join("\r\n", ExportContents.ToArray());
    }

    private WordLibraryList RemoveEmptyCodeData(WordLibraryList wordLibraryList)
    {
        var list = new WordLibraryList();
        foreach (var wordLibrary in wordLibraryList)
            if (!string.IsNullOrEmpty(wordLibrary.SingleCode)) //没有编码，则不保留
                list.Add(wordLibrary);

        return list;
    }

    private void GenerateWordRank(WordLibraryList wordLibraryList)
    {
        countWord = wordLibraryList.Count;
        currentStatus = 0;
        foreach (var wordLibrary in wordLibraryList)
        {
            if (wordLibrary.Rank == 0 || SelectedWordRankGenerater.ForceUse)
                wordLibrary.Rank = SelectedWordRankGenerater.GetRank(wordLibrary.Word);
            currentStatus++;
            processMessage = "生成词频：" + currentStatus + "/" + countWord;
        }
    }

    /// 把字符串中的数字转换为汉字. 当数字不以0开头，并且以多个0结尾时，按照x千x百的方式转换。否则直接读挨个数字。
    private static string TranslateChineseNumber(string str)
    {
        var builder = new StringBuilder();
        var buffer = new StringBuilder();

        foreach (var c in str)
            if (c <= '9' && c >= '0')
            {
                buffer.Append(c);
            }
            else
            {
                if (buffer.Length > 0)
                {
                    builder.Append(Num2Chs(buffer.ToString()));
                    buffer = new StringBuilder();
                }

                builder.Append(c);
            }

        if (buffer.Length > 0)
            builder.Append(Num2Chs(buffer.ToString()));

        return builder.ToString();
    }

    private static string Num2Chs(string str)
    {
        if (Num2ChsRegex.IsMatch(str))
            return Int2Chs(long.Parse(str));

        var chars = new char[str.Length];

        for (var i = 0; i < str.Length; i++) chars[i] = Num2Char(str[i]);

        return new string(chars);
    }

    private static char Num2Char(char c)
    {
        switch (c)
        {
            case '1':
                return '一';
            case '2':
                return '二';
            case '3':
                return '三';
            case '4':
                return '四';
            case '5':
                return '五';
            case '6':
                return '六';
            case '7':
                return '七';
            case '8':
                return '八';
            case '9':
                return '九';
        }

        return '零';
    }

    // 十位以上的数字转换汉字, 来自这里 https://blog.csdn.net/iplayvs2008/article/details/89517321
    private static string Int2Chs(long input)
    {
        string ret = null;
        var input2 = Math.Abs(input);
        var resource = "零一二三四五六七八九";
        var unit = "个十百千万亿兆京垓秭穰沟涧正载极";
        if (input > Math.Pow(10, 4 * (unit.Length - unit.IndexOf('万'))))
            throw new Exception("the input is too big,input:" + input);
        Func<long, List<List<int>>> splitNumFunc = val =>
        {
            var i = 0;
            int mod;
            var val_ = val;
            var splits = new List<List<int>>();
            List<int> splitNums;
            do
            {
                mod = (int)(val_ % 10);
                val_ /= 10;
                if (i % 4 == 0)
                {
                    splitNums = new List<int>();
                    splitNums.Add(mod);
                    if (splits.Count == 0)
                        splits.Add(splitNums);
                    else
                        splits.Insert(0, splitNums);
                }
                else
                {
                    splitNums = splits[0];
                    splitNums.Insert(0, mod);
                }

                i++;
            } while (val_ > 0);

            return splits;
        };
        Func<List<List<int>>, string> hommizationFunc = data =>
        {
            var builders = new List<StringBuilder>();
            for (var i = 0; i < data.Count; i++)
            {
                var data2 = data[i];
                var newVal = new StringBuilder();
                for (var j = 0; j < data2.Count;)
                    if (data2[j] == 0)
                    {
                        var k = j + 1;
                        for (; k < data2.Count && data2[k] == 0; k++)
                            ;
                        //个位不是0，前面补一个零
                        newVal.Append('零');
                        j = k;
                    }
                    else
                    {
                        newVal.Append(resource[data2[j]]).Append(unit[data2.Count - 1 - j]);
                        j++;
                    }

                if (newVal[newVal.Length - 1] == '零' && newVal.Length > 1)
                    newVal.Remove(newVal.Length - 1, 1);
                else if (newVal[newVal.Length - 1] == '个') newVal.Remove(newVal.Length - 1, 1);

                if (i == 0 && newVal.Length > 1 && newVal[0] == '一' && newVal[1] == '十')
                    //一十 --> 十
                    newVal.Remove(0, 1);
                builders.Add(newVal);
            }

            var sb = new StringBuilder();
            for (var i = 0; i < builders.Count; i++)
                //拼接
                if (builders.Count == 1)
                {
                    //个位数
                    sb.Append(builders[i]);
                }
                else
                {
                    if (i == builders.Count - 1)
                    {
                        //万位以下的
                        if (builders[i][builders[i].Length - 1] != '零')
                            //十位以上的不拼接"零"
                            sb.Append(builders[i]);
                    }
                    else
                    {
                        //万位以上的
                        if (builders[i][0] != '零')
                            //零万零亿之类的不拼接
                            sb.Append(builders[i])
                                .Append(unit[unit.IndexOf('千') + builders.Count - 1 - i]);
                    }
                }

            return sb.ToString();
        };
        var ret_split = splitNumFunc(input2);
        ret = hommizationFunc(ret_split);
        if (input < 0)
            ret = "-" + ret;
        return ret;
    }

    private void GenerateDestinationCode(WordLibraryList wordLibraryList, CodeType codeType)
    {
        if (wordLibraryList.Count == 0)
            return;
        if (
            wordLibraryList[0].CodeType == CodeType.NoCode
            && codeType == CodeType.UserDefinePhrase
        )
            codeType = CodeType.Pinyin;
        var generater = CodeTypeHelper.GetGenerater(codeType);
        if (generater == null) //未知编码方式，则不进行编码。
            return;
        countWord = wordLibraryList.Count;
        currentStatus = 0;
        var spaceRegex = new Regex("(?=[^a-zA-Z])\\s+");
        var numberRegex = new Regex("[0-9０-９]+");
        var englishRegex = new Regex("[a-zA-Zａ-ｚＡ-Ｚ]+");
        var fullWidthRegex = new Regex("[\uff00-\uff5e]+");
        // Regex fullWidthRegex = new Regex("[ａ-ｚＡ-Ｚ０-９]+");
        // Regex punctuationRegex = new Regex("[-・·&%']");
        var punctuationRegex = new Regex(
            "[\u0021-\u002f\u003a-\u0040\u005b-\u0060\u007b-\u008f\u00a0-\u00bf\u00d7\u00f7\u2000-\u2bff\u3000-\u303f\u30a0\u30fb\uff01-\uff0f\uff1a-\uff20\uff5b-\uff65]"
        );

        foreach (var wordLibrary in wordLibraryList)
        {
            currentStatus++;
            processMessage = "生成目标编码：" + currentStatus + "/" + countWord;
            if (wordLibrary.CodeType == codeType) continue;
            if (wordLibrary.CodeType == CodeType.English)
            {
                wordLibrary.SetCode(CodeType.English, wordLibrary.Word.ToLower());
                continue;
            }

            try
            {
                var word_0 = wordLibrary.Word;
                var word = wordLibrary.Word;

                if (FilterConfig.FullWidth && fullWidthRegex.IsMatch(word))
                {
                    var c = word.ToCharArray();
                    for (var i = 0; i < c.Length; i++)
                        if (c[i] <= 0xff5e && c[i] >= 0xff00)
                            c[i] = (char)(c[i] - 65248);
                    word = new string(c);
                }

                if (FilterConfig.KeepNumber_) word = numberRegex.Replace(word, "");

                if (FilterConfig.KeepEnglish_) word = englishRegex.Replace(word, "");

                if (FilterConfig.KeepSpace_)
                {
                    if (FilterConfig.KeepSpace == false)
                        word = word.Replace(" ", "");
                    else
                        word = spaceRegex.Replace(word, "");
                }

                if (FilterConfig.KeepPunctuation_) word = punctuationRegex.Replace(word, "");

                if (FilterConfig.ChsNumber) word = TranslateChineseNumber(word);

                if (
                    (FilterConfig.KeepEnglish && englishRegex.IsMatch(word))
                    || (FilterConfig.KeepNumber && numberRegex.IsMatch(word))
                    || (FilterConfig.KeepPunctuation && punctuationRegex.IsMatch(word))
                )
                {
                    var input = new StringBuilder();
                    var output = new List<IList<string>>();

                    var clipType = -1;
                    var type = 0;

                    foreach (var c in word)
                    {
                        if (c >= 0x30 && c <= 0x39)
                            type = 1;
                        else if (c >= 0x41 && c <= 0x5a)
                            type = 2;
                        else if (c >= 0x61 && c <= 0x7a)
                            type = 2;
                        else if (FilterConfig.KeepSpace && c == 0x20 && clipType == 2)
                            type = 2;
                        else if ("-・&%'".Contains(c))
                            type = 3;
                        else if (punctuationRegex.IsMatch(c.ToString()))
                            type = 3;
                        else
                            type = 0;
                        if (input.Length < 1)
                        {
                            clipType = type;
                            input.Append(c);
                        }
                        else if (type == clipType)
                        {
                            input.Append(c);
                        }
                        else
                        {
                            if (FilterConfig.KeepEnglish && clipType == 2)
                            {
                                if (FilterConfig.needEnglishTag())
                                    output.Add(new List<string> { '_' + input.ToString() });
                                else
                                    output.Add(new List<string> { input.ToString() });
                            }
                            else if (
                                (FilterConfig.KeepNumber && clipType == 1)
                                || (FilterConfig.KeepPunctuation && clipType == 3)
                            )
                            {
                                output.Add(new List<string> { input.ToString() });
                            }
                            else
                            {
                                wordLibrary.Word = input.ToString();
                                wordLibrary.CodeType = CodeType.NoCode;
                                generater.GetCodeOfWordLibrary(wordLibrary);
                                output.AddRange(wordLibrary.Codes);
                            }

                            input.Clear();
                            input.Append(c);
                            clipType = type;
                        }
                    }

                    if (input.Length > 0)
                    {
                        if (FilterConfig.KeepEnglish && clipType == 2)
                        {
                            if (FilterConfig.needEnglishTag())
                                output.Add(new List<string> { '_' + input.ToString() });
                            else
                                output.Add(new List<string> { input.ToString() });
                        }
                        else if (
                            (FilterConfig.KeepNumber && clipType == 1)
                            || (FilterConfig.KeepPunctuation && clipType == 3)
                        )
                        {
                            output.Add(new List<string> { input.ToString() });
                        }
                        else
                        {
                            wordLibrary.Word = input.ToString();
                            wordLibrary.CodeType = CodeType.NoCode;
                            generater.GetCodeOfWordLibrary(wordLibrary);
                            output.AddRange(wordLibrary.Codes);
                        }
                    }

                    wordLibrary.Word = word_0;
                    wordLibrary.Codes = new Code(output);
                }
                else
                {
                    if (word == word_0)
                    {
                        generater.GetCodeOfWordLibrary(wordLibrary);
                    }
                    else
                    {
                        wordLibrary.Word = word;
                        generater.GetCodeOfWordLibrary(wordLibrary);
                        wordLibrary.Word = word_0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("生成编码失败" + ex.Message);
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
        timer.Start();
        ExportContents = new List<string>();
        var c = 0;

        //filePathes = GetRealPath(filePathes);
        var fileCount = filePathes.Count;
        var fileProcessed = 0;
        foreach (var file in filePathes)
        {
            fileProcessed++;
            var start = DateTime.Now;
            try
            {
                var wlList = Import.Import(file);
                wlList = Filter(wlList);
                if (SelectedTranslate != ChineseTranslate.NotTrans) wlList = ConvertChinese(wlList);
                c += wlList.Count;
                GenerateWordRank(wlList);
                wlList = RemoveEmptyCodeData(wlList);
                ReplaceAfterCode(wlList);
                ExportContents = Export.Export(wlList);
                for (var i = 0; i < ExportContents.Count; i++)
                {
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);
                    var exportPath = Path.Combine(
                        outputDir,
                        Path.GetFileNameWithoutExtension(file)
                        + (i == 0 ? "" : i.ToString())
                        + ".txt"
                    );
                    FileOperationHelper.WriteFile(
                        exportPath,
                        Export.Encoding,
                        ExportContents[i]
                    );
                }

                ExportContents = new List<string>();
                var costSeconds = (DateTime.Now - start).TotalSeconds;
                ProcessNotice?.Invoke(
                    fileProcessed
                    + "/"
                    + fileCount
                    + "\t"
                    + Path.GetFileName(file)
                    + "\t转换完成，耗时："
                    + costSeconds
                    + "秒\r\n"
                );
            }
            catch (Exception ex)
            {
                ProcessNotice?.Invoke(
                    fileProcessed
                    + "/"
                    + fileCount
                    + "\t"
                    + Path.GetFileName(file)
                    + "\t处理时发生异常："
                    + ex.Message
                    + "\r\n"
                );
                Count = c;
                timer.Stop();
            }
        }

        Count = c;
        timer.Stop();
    }

    public void ExportToFile(string filePath)
    {
        var outputDir = Path.GetDirectoryName(filePath);
        for (var i = 0; i < ExportContents.Count; i++)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            var exportPath = Path.Combine(
                outputDir,
                Path.GetFileNameWithoutExtension(filePath)
                + (i == 0 ? "" : i.ToString())
                + ".txt"
            );
            FileOperationHelper.WriteFile(exportPath, Export.Encoding, ExportContents[i]);
        }

        ExportContents = new List<string>();
    }

    public void StreamConvert(IList<string> filePathes, string outPath)
    {
        var textImport = Import as IWordLibraryTextImport;
        if (textImport == null) throw new Exception("流转换,只有文本类型的才支持。");
        var stream = FileOperationHelper.GetWriteFileStream(outPath, Export.Encoding);
        foreach (var filePath in filePathes)
        {
            var wlStream = new WordLibraryStream(
                Import,
                Export,
                filePath,
                textImport.Encoding,
                stream
            );
            wlStream.ConvertWordLibrary(w => IsKeep(w));
        }

        stream.Close();
    }

    private void ReplaceAfterCode(WordLibraryList list)
    {
        foreach (var wordLibrary in list)
            if (ReplaceFilters != null)
                foreach (var replaceFilter in ReplaceFilters)
                    if (replaceFilter.ReplaceAfterCode)
                        replaceFilter.Replace(wordLibrary);
    }

    private WordLibraryList Filter(WordLibraryList list)
    {
        var result = new WordLibraryList();
        foreach (var wordLibrary in list)
            if (IsKeep(wordLibrary))
            {
                if (ReplaceFilters != null)
                    foreach (var replaceFilter in ReplaceFilters)
                        if (!replaceFilter.ReplaceAfterCode)
                            replaceFilter.Replace(wordLibrary);
                if (wordLibrary.Word != string.Empty)
                    result.Add(wordLibrary);
            }

        return result;
    }

    private bool IsKeep(WordLibrary wordLibrary)
    {
        foreach (var filter in Filters)
            if (!filter.IsKeep(wordLibrary))
                return false;

        return true;
    }

    private WordLibraryList ConvertChinese(WordLibraryList wordLibraryList)
    {
        var sb = new StringBuilder();
        var count = wordLibraryList.Count;
        foreach (var wordLibrary in wordLibraryList) sb.Append(wordLibrary.Word + "\r");
        var result = "";
        if (SelectedTranslate == ChineseTranslate.Trans2Chs)
            result = SelectedConverter.ToChs(sb.ToString());
        else if (SelectedTranslate == ChineseTranslate.Trans2Cht) result = SelectedConverter.ToCht(sb.ToString());
        var newList = result.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (newList.Length != count) throw new Exception("简繁转换时转换失败，请更改简繁转换设置");
        for (var i = 0; i < count; i++)
        {
            var wordLibrary = wordLibraryList[i];
            wordLibrary.Word = newList[i];
        }

        return wordLibraryList;
    }
}
