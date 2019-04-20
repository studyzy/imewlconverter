using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     基于小小输入法而制作的二笔输入法，包括超强二笔、现代二笔、青松二笔等,格式：
    ///     编码 词语1 词语2 词语3
    /// </summary>
    [ComboBoxShow(ConstantString.XIAOXIAO_ERBI, ConstantString.XIAOXIAO_ERBI_C, 100)]
    public class XiaoxiaoErbi : BaseImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        public Encoding Encoding
        {
            get
            {
                try
                {
                    return Encoding.GetEncoding("GB18030");
                }
                catch
                {
                    return Encoding.GetEncoding("GB2312");
                }
            }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append(wl.SingleCode);
            sb.Append(" ");
            sb.Append(wl.Word);
            return sb.ToString();
        }


        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();

            IDictionary<string, string> xiaoxiaoDic = new Dictionary<string, string>();

            for (int i = 0; i < wlList.Count; i++)
            {
                string key = "";
                WordLibrary wl = wlList[i];
                string value = wl.Word;
                foreach (var code in wl.Codes)
                {
                    key = code[0];
                    if (xiaoxiaoDic.ContainsKey(key))
                    {
                        xiaoxiaoDic[key] += " " + value;
                    }
                    else
                    {
                        xiaoxiaoDic.Add(key, value);
                    }
                }
            }
            foreach (var keyValuePair in xiaoxiaoDic)
            {
                sb.Append(keyValuePair.Key + " " + keyValuePair.Value + "\n");
            }

            return new List<string>() { sb.ToString() };
        }

        #endregion
    }
}