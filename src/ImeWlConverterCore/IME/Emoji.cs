using Studyzy.IMEWLConverter;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Emoji表情，格式为：第一个字符是表情，Tab键，后面字符是汉字
    /// 😀   汉字
    /// </summary>
    [ComboBoxShow(ConstantString.EMOJI, ConstantString.EMOJI_C, 999)]
    public class Emoji : BaseImport, IWordLibraryTextImport
    {
        public override CodeType CodeType
        {
            get { return CodeType.NoCode; }
        }
        public Encoding Encoding => Encoding.UTF8;

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        public WordLibraryList ImportLine(string line)
        {
            var wl = new WordLibrary();
            wl.Word = line.Split('\t')[1];
            wl.CodeType = CodeType;
            wl.IsEnglish = IsEnglish(wl.Word);
            if (wl.IsEnglish)
            {
                wl.SetCode(CodeType.English, wl.Word);
            }
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }
        private static Regex regex = new Regex("^[a-zA-Z]+$");
        private bool IsEnglish(string word)
        {
            return regex.IsMatch(word);
        }

        public WordLibraryList ImportText(string text)
        {
            var wlList = new WordLibraryList();
            string[] lines = text.Split(new[] { "\r","\n" }, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                wlList.AddWordLibraryList(ImportLine(line));
            }
            return wlList;
        }
    }
}
