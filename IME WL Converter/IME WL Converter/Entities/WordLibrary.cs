using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Entities
{
    /// <summary>
    /// 词条类
    /// </summary>
    public class WordLibrary
    {
        public WordLibrary()
        {
            this.CodeType = CodeType.Pinyin;
            this.Codes=new List<List<string>>();
        }

        #region 基本属性

        private int count = 1;
        private bool isEnglish;
        //private string[] pinYin;
        private string pinYinString = "";
        private string word;

        /// <summary>
        /// 该词条是否是英文词条
        /// </summary>
        public bool IsEnglish
        {
            get { return isEnglish; }
            set { isEnglish = value; }
        }

        /// <summary>
        /// 词语
        /// </summary>
        public string Word
        {
            get { return word; }
            set { word = value; }
        }

        /// <summary>
        /// 词频，打字出现次数
        /// </summary>
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

       

        public CodeType CodeType { get; set; }
        public IList<List<string>> Codes { get; set; }

        #endregion

        #region 扩展属性和方法
        private static IWordCodeGenerater pyGenerater=new PinyinGenerater();
        private string[] tempPinyin;
        /// <summary>
        /// 词中每个字的拼音(已消除多音字)
        /// </summary>
        public string[] PinYin
        {
            get
            {
                if(CodeType==CodeType.Pinyin&&Codes.Count>0)
                {
                    return Codes[0].ToArray();
                }
                else
                {
                    if (tempPinyin == null)
                    {
                        var py = pyGenerater.GetCodeOfString(this.word);
                        tempPinyin = new string[py.Count];
                        for (int i = 0; i < py.Count; i++)
                        {
                            tempPinyin[i] = py[i];
                        }
                    }
                    return tempPinyin;
                }
            }
            set
            {
                CodeType=CodeType.Pinyin;
                Codes=new List<string>[1];
                Codes[0]=new List<string>(value);
            }
        }
        /// <summary>
        /// 词的拼音字符串，可以单独设置的一个属性，如果没有设置该属性，而获取该属性，则返回PinYin属性和“'”组合的字符串
        /// </summary>
        public string PinYinString
        {
            get
            {
                if (pinYinString == "" && !isEnglish)
                {
                    pinYinString = string.Join("'", PinYin);
                }
                return pinYinString;
            }
            set { pinYinString = value; }
        }

        public string WubiCode
        {
            get
            {
                if (this.CodeType== CodeType.Wubi)
                {
                    return Codes[0][0];
                }
                return null;
            }
        }

        /// <summary>
        /// 添加一种编码类型和词对应的编码，针对一词一码的情况
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        public void AddCode(CodeType type, string str)
        {
            this.CodeType = type;
            this.Codes= new List<List<string>> {new List<string> {str}};
        }

        /// <summary>
        /// 获得拼音字符串
        /// </summary>
        /// <param name="split">每个拼音之间的分隔符</param>
        /// <param name="buildType">组装拼音字符串的方式</param>
        /// <returns></returns>
        public string GetPinYinString(string split, BuildType buildType)
        {
            return CollectionHelper.GetString(PinYin, split, buildType);

        }

        public string ToDisplayString()
        {
            return "汉字：" + word + (string.IsNullOrEmpty(WubiCode) ? "；拼音：" + PinYinString : "五笔：" + WubiCode) + "；词频：" +
                   count;
        }

        public override string ToString()
        {
            return "WordLibrary 汉字：" + word +"Codes:"+
                   CollectionHelper.ListToString(Codes[0]) + "；词频：" + count;
        }

        #endregion
    }
}