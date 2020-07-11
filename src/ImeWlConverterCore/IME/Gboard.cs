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
    public class Gboard : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
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
            FileOperationHelper.WriteFile(tempPath, Encoding.UTF8, sb.ToString());
            string zipPath = Path.Combine(FileOperationHelper.GetCurrentFolderPath(), "Gboard词库.zip");
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            FileOperationHelper.ZipFile(tempPath, zipPath);
            return new List<string>() { "词库文件在：" + zipPath };
            //return new List<string>() { sb.ToString() };
        }


        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;

            }
        }

        #endregion

        #region IWordLibraryImport 成员



        public override WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split('\t');
            var wl = new WordLibrary();
            wl.Word = c[1];
            //wl.Rank = Convert.ToInt32(c[1]);
            //wl.PinYin = c[2].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
        public override CodeType CodeType { get => CodeType.UserDefinePhrase; set => base.CodeType = value; }
        #endregion
    }
}