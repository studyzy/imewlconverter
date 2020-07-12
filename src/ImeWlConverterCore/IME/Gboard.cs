using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Gboard输入法
    /// </summary>
    [ComboBoxShow(ConstantString.GBOARD, ConstantString.GBOARD_C, 111)]
    public class Gboard : BaseImport, IWordLibraryExport, IWordLibraryImport
    {
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();           
            sb.Append(wl.GetPinYinString("", BuildType.None));
            sb.Append("\t");
            sb.Append(wl.Word);
            sb.Append("\tzh-CN");
            return sb.ToString();
        }


        public IList<string> Export(WordLibraryList wlList)
        {
            string tempPath = Path.Combine(FileOperationHelper.GetCurrentFolderPath(), "dictionary.txt");
            if (File.Exists(tempPath)) { File.Delete(tempPath); }
            var sb = new StringBuilder();
            sb.Append("# Gboard Dictionary version:1\n");
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\n");
            }
            FileOperationHelper.WriteFile(tempPath, new UTF8Encoding(false), sb.ToString());
            string zipPath = Path.Combine(FileOperationHelper.GetCurrentFolderPath(), "Gboard词库.zip");
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            FileOperationHelper.ZipFile(tempPath, zipPath);
            return new List<string>() { "词库文件在：" + zipPath };
            //return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryImport 成员



        public  WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split('\t');
            var wl = new WordLibrary();
            wl.Word = c[1];
            wl.CodeType = CodeType.UserDefinePhrase;
            //wl.Rank = Convert.ToInt32(c[1]);
            //wl.PinYin = c[2].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        public WordLibraryList Import(string path)
        {
            var tempUnzipFolder = FileOperationHelper.GetCurrentFolderPath();
            FileOperationHelper.UnZip(path, tempUnzipFolder);
            var tempFilePath= Path.Combine(FileOperationHelper.GetCurrentFolderPath(), "dictionary.txt");
            string str = FileOperationHelper.ReadFile(tempFilePath,new UTF8Encoding(false));
            File.Delete(tempFilePath);
            return ImportText(str);
        }
        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] lines = str.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                try
                {
                    wlList.AddWordLibraryList(ImportLine(line));
                }
                catch
                {
                    SendImportLineErrorNotice("无效的词条，解析失败：" + line);
                }
            }
            return wlList;
        }
        public override CodeType CodeType { get => CodeType.UserDefinePhrase; set => base.CodeType = value; }

        public Encoding Encoding => throw new NotImplementedException();
        #endregion
    }
}