using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studyzy.IMEWLConverter.Filters
{
    /// <summary>
    /// 将普通的拼音编码替换成小鹤双拼
    /// </summary>
    public class XiaoheShuangpinReplacer : IReplaceFilter
    {
        private Dictionary<string, string> mapping = new Dictionary<string, string>();
        public XiaoheShuangpinReplacer()
        {
            string pinyinMapping = DictionaryHelper.GetResourceContent("XiaoheShuangpin.txt");
            foreach (var line in pinyinMapping.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var arr = line.Split('\t');
                var pinyin = arr[0];
                var shuangpin = arr[1];
                mapping[pinyin] = shuangpin;
            }
        }

        public bool ReplaceAfterCode => true;

        public void Replace(WordLibrary wl)
        {
            if(wl.CodeType!=CodeType.Pinyin)//必须是拼音才能被双拼替换
            {
                return;
            }
           foreach(var code in wl.Codes)
            {
                for(var i=0;i<code.Count;i++)
                {
                    try
                    {
                        code[i] = mapping[code[i]];
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message + " " + code[i]);
                    }
                }
            }
        }
    }
}
