using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.SELF_DEFINING, ConstantString.SELF_DEFINING_C, 2000)]
    public class SelfDefining : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        public override CodeType CodeType
        {
            get
            {
                return CodeType.UserDefine;
            }
        }
        public SelfDefining()
        {
            CodeType=CodeType.Unknown;
            exportForm=new SelfDefiningConfigForm();
            importForm = new SelfDefiningConfigForm();
            exportForm.IsImport = false;
            exportForm.Closed += new EventHandler(exportForm_Closed);
            importForm.IsImport = true;
            importForm.Closed += new EventHandler(importForm_Closed);
        }

        void exportForm_Closed(object sender, EventArgs e)
        {
            Global.ExportSelfDefiningPattern = exportForm.SelectedParsePattern;
            this.UserDefiningPattern = exportForm.SelectedParsePattern;
        }
        void importForm_Closed(object sender, EventArgs e)
        {
            Global.ImportSelfDefiningPattern = importForm.SelectedParsePattern;
            this.UserDefiningPattern = importForm.SelectedParsePattern;
        }
        private SelfDefiningConfigForm exportForm;
        private SelfDefiningConfigForm importForm;
        public ParsePattern UserDefiningPattern { get; set; }

        //private SelfDefiningCodeGenerater codeGenerater = new SelfDefiningCodeGenerater();

        #region IWordLibraryExport Members
        /// <summary>
        /// 导出词库为自定义格式。
        /// 如果没有指定自定义编码文件，而且词库是包含拼音编码的，那么就按拼音编码作为每个字的码。
        /// 如果导出指定了自定义编码文件，那么就忽略词库的已有编码，使用自定义编码文件重新生成编码。
        /// 如果词库没有包含拼音编码，而且导出也没有指定编码文件，那就抛错吧~~~~
        /// </summary>
        /// <param name="wlList"></param>
        /// <returns></returns>
        public string Export(WordLibraryList wlList)
        {
            if (string.IsNullOrEmpty(UserDefiningPattern.MappingTablePath))
            {
                if (wlList.Count ==0 || wlList[0].CodeType != CodeType.Pinyin)
                {
                    throw new Exception("未指定字符编码映射文件，无法对词库进行自定义编码的生成");
                }
            }
            else
            {
                //var dict = UserCodingHelper.GetCodingDict(UserDefiningPattern.MappingTablePath);
                //codeGenerater.MappingDictionary = dict;
                //codeGenerater.MutiWordCodeFormat = UserDefiningPattern.MutiWordCodeFormat;
            }
            var sb = new StringBuilder();
            foreach (WordLibrary wordLibrary in wlList)
            {
                try
                {
                    sb.Append(ExportLine(wordLibrary));
                    sb.Append("\r\n");
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return sb.ToString();
        }

        public string ExportLine(WordLibrary wl)
        {
            //if (string.IsNullOrEmpty(UserDefiningPattern.MappingTablePath))
            //{
            //    if (wl.CodeType != CodeType.Pinyin)
            //    {
            //        throw new Exception("未指定字符编码映射文件，无法对词库进行自定义编码的生成");
            //    }
            //    else if (wl.Codes.Count == 0 || wl.Codes[0].Count == 0)
            //    {//是拼音，但是没有给出拼音
            //        throw new Exception("未指定字符编码映射文件，无法对词库进行自定义编码的生成");
            //    }
            //    //自定义拼音格式
            //    IDictionary<char,string> dic=new Dictionary<char, string>();
            //    for (var i=0;i< wl.Word.Length;i++)
            //    {
            //        if(!dic.ContainsKey(wl.Word[i]))
            //        dic.Add(wl.Word[i],wl.PinYin[i]);
            //    }
            //    return UserDefiningPattern.BuildWLString(dic,wl.Count);
            //}
            //else//自定义编码模式
            //{
            //    //var codes = codeGenerater.GetCodeOfString(wl.Word);
            //    //return UserDefiningPattern.BuildWLString(wl.Word, codes[0], wl.Count);
            //    return null;
            //}
            return UserDefiningPattern.BuildWlString(wl);
        }

        public Form ExportConfigForm { get { return exportForm; } }
        public override Form ImportConfigForm
        {
            get { return importForm; }
        }
        #endregion

        #region IWordLibraryTextImport Members

        public Encoding Encoding
        {
            get { return Encoding.GetEncoding("GBK"); }
        }


        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path);
            return ImportText(str);
        }

        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] lines = str.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                wlList.AddWordLibraryList(ImportLine(line));
                CurrentStatus = i;
            }
            return wlList;
        }

        public WordLibraryList ImportLine(string line)
        {
            var wlList = new WordLibraryList();
            WordLibrary wl = UserDefiningPattern.BuildWordLibrary(line);
            wlList.Add(wl);
            return wlList;
        }
       

        #endregion


    }
}