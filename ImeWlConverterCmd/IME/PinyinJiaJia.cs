using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// 
    /// </summary>
    [ComboBoxShow(ConstantString.PINYIN_JIAJIA, ConstantString.PINYIN_JIAJIA_C, 120)]
    public class PinyinJiaJia : BaseImport, IWordLibraryExport, IWordLibraryTextImport
    {
        #region IWordLibraryExport 成员

        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wlList.Count; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                    sb.Append("\r\n");
                }
            }
            return new List<string>() { sb.ToString() };
        }

        public string ExportLine(WordLibrary wl)
        {
            try
            {
                var sb = new StringBuilder();

                string str = wl.Word;
                for (int j = 0; j < str.Length; j++)
                {
                    sb.Append(str[j] + wl.PinYin[j]);
                }

                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region IWordLibraryImport 成员

        private readonly IWordCodeGenerater single = new PinyinGenerater();


        /// <summary>
        ///     形如：冷血xue动物
        ///     只有多音字才注音，一般的字不注音，就使用默认读音即可
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] words = str.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            CountWord = words.Length;
            for (int i = 0; i < words.Length; i++)
            {
                CurrentStatus = i;
                try
                {
                    wlList.AddWordLibraryList(ImportLine(words[i]));
                }
                catch
                {
                }
            }
            return wlList;
        }

        public WordLibraryList ImportLine(string word)
        {
            string hz = "";
            var py = new List<string>();
            int j;
            for (j = 0; j < word.Length - 1; j++)
            {
                hz += word[j];
                if (word[j + 1] > 'z') //而且后面跟的不是拼音
                {
                    py.Add(single.GetAllCodesOfChar(word[j])[0]);
                }
                else //后面跟拼音
                {
                    int k = 1;
                    string py1 = "";
                    while (j + k != word.Length && word[j + k] <= 'z')
                    {
                        py1 += word[j + k];
                        k++;
                    }
                    py.Add(py1);
                    j += k - 1; //减1是因为接下来会运行j++
                }
            }
            if (j == word.Length - 1) //最后一个字是汉字
            {
                hz += word[j];
                py.Add(single.GetAllCodesOfChar(word[j])[0]);
            }
            var wl = new WordLibrary();
            wl.PinYin = py.ToArray();
            wl.Word = hz;
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        #endregion
    }
}