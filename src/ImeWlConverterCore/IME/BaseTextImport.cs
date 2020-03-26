using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studyzy.IMEWLConverter.IME
{
    public abstract class BaseTextImport:BaseImport
    {
        public abstract WordLibraryList ImportLine(string line);
        public abstract Encoding Encoding
        {
            get;
        }
        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }
        protected virtual  bool IsContent(string line)
        {
            return true;
        }

        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] lines = str.Split(new[] { "\r","\n" }, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                try
                {
                    if (IsContent(line))
                    {
                        wlList.AddWordLibraryList(ImportLine(line));
                    }
                }
                catch
                {
                    SendImportLineErrorNotice("无效的词条，解析失败：" + line);
                }
            }
            return wlList;
        }
    }
}
