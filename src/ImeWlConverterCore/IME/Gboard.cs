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
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Gboard输入法
    /// </summary>
    [ComboBoxShow(ConstantString.GBOARD, ConstantString.GBOARD_C, 111)]
    public class Gboard : BaseImport, IWordLibraryExport, IWordLibraryImport
    {
        public Gboard()
        {
            this.PinyinType = PinyinType.FullPinyin;
        }

        #region IWordLibraryExport 成员
        public PinyinType PinyinType { get; set; }

        private WordLibraryList Filter(WordLibraryList wlList)
        {
            var result = new WordLibraryList();
            IReplaceFilter replace = null;
            if (PinyinType != PinyinType.FullPinyin)
            {
                replace = new ShuangpinReplacer(PinyinType);
            }
            foreach (var wl in wlList)
            {
                if (replace != null)
                {
                    replace.Replace(wl);
                }

                //if (wl.GetPinYinLength() > 32)
                //    continue;
                //if (wl.Word.Length > 64)
                //    continue;

                result.Add(wl);
            }
            return result;
        }

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
            //对全拼方案进行编码转换
            wlList = Filter(wlList);
            string tempPath = Path.Combine(
                FileOperationHelper.GetCurrentFolderPath(),
                "dictionary.txt"
            );
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            var sb = new StringBuilder();
            sb.Append("# Gboard Dictionary version:1\n");
            for (int i = 0; i < wlList.Count; i++)
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\n");
            }
            FileOperationHelper.WriteFile(tempPath, new UTF8Encoding(false), sb.ToString());
            string zipPath = Path.Combine(
                FileOperationHelper.GetCurrentFolderPath(),
                "Gboard词库.zip"
            );
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            FileOperationHelper.ZipFile(tempPath, zipPath);
            return new List<string>() { "词库文件在：" + zipPath };
            //return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryImport 成员



        public WordLibraryList ImportLine(string line)
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
            var tempFilePath = Path.Combine(
                FileOperationHelper.GetCurrentFolderPath(),
                "dictionary.txt"
            );
            string str = FileOperationHelper.ReadFile(tempFilePath, new UTF8Encoding(false));
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

        public override CodeType CodeType
        {
            get => CodeType.UserDefinePhrase;
            set => base.CodeType = value;
        }

        public Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                try
                {
                    return Encoding.GetEncoding("GBK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        ex.Message + " Your system doesn't support GBK, try to use GB2312."
                    );
                    return Encoding.GetEncoding("GB2312");
                }
            }
        }
        #endregion
    }
}
