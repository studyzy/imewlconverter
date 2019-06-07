using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studyzy.IMEWLConverter.Filters
{
    public class EmojiReplacer: IReplaceFilter
    {
        private Dictionary<string, string> mapping = new Dictionary<string, string>();
        public EmojiReplacer(string path)
        {
            string str = FileOperationHelper.ReadFile(path);
            foreach (var line in str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var arr = line.Split('\t');
                var emoji = arr[0];
                var word = arr[1];
                mapping[word] = emoji;
            }
        }

        public bool ReplaceAfterCode => true;

        public void Replace(WordLibrary wl)
        {
            if (mapping.ContainsKey(wl.Word))
            {
                wl.Word = mapping[wl.Word];
            }
        }
    }
}
