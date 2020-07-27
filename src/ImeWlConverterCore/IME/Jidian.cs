using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     极点五笔/郑码的词库格式为“编码 词语 词语 词语”\r\n
    /// </summary>
    [ComboBoxShow(ConstantString.JIDIAN, ConstantString.JIDIAN_C, 190)]
    public class Jidian : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
    {
        //protected virtual bool IsWubi
        //{
        //    get { return true; }
        //}

        public override CodeType CodeType
        {
            get { return CodeType.Wubi; }
        }

        #region IWordLibraryImport 成员

      
        public override WordLibraryList ImportLine(string line)
        {
            var wlList = new WordLibraryList();
            string[] strs = line.Split(' ');

            for (int i = 1; i < strs.Length; i++)
            {
                string oriWord = strs[i];
                string word = oriWord.Replace("，", ""); //把汉字中带有逗号的都去掉逗号
                //var list = pinyinFactory.GetCodeOfString(word);
                //for (int j = 0; j < list.Count; j++)
                //{
                var wl = new WordLibrary();
                wl.Word = oriWord;
                //if (IsWubi)
                //{
                //    wl.SetCode(CodeType.Wubi, strs[0]);
                //}
                //wl.PinYin = CollectionHelper.ToArray(list);
                wl.SetCode(CodeType, strs[0]);
                wlList.Add(wl);
                //}
            }
            return wlList;
        }

        #endregion

        #region IWordLibraryExport 成员

       public virtual string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException("极点输入法词库不支持流转换");
        }

        public string ExportLine(string code,WordLibraryList wll)
        {
            var sb = new StringBuilder();
            sb.Append(code);
            foreach (var wl in wll)
            {
                sb.Append(" ");
                sb.Append(wl.Word);
            }
            return sb.ToString();
        }


        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            var dict = new Dictionary<string, WordLibraryList>();
            for (int i = 0; i < wlList.Count; i++)
            {
                var wl = wlList[i];
                if(dict.ContainsKey(wl.SingleCode))
                {
                    dict[wl.SingleCode].Add(wl);
                }
                else
                {
                    dict.Add(wl.SingleCode, new WordLibraryList { wl});
                }
            }
            foreach(var key in dict.Keys)
            {

                sb.Append(ExportLine(key,dict[key]));
                sb.Append("\r\n");
            }
            return new List<string>() { sb.ToString() };
        }


        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        #endregion
    }
}