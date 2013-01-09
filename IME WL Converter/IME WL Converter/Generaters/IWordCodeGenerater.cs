using System.Collections.Generic;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    /// 根据汉字输出其Code的接口,其假设是一个汉字只有一个Code对应
    /// </summary>
    public interface IWordCodeGenerater
    {
        /// <summary>
        /// 该编码方式是否存在一字多码的情况，如果存在就调用GetCodeOfString，如果不存在或者忽略其他编码，就调用GetDefaultCodeOfString
        /// </summary>
        bool Is1CharMutiCode { get; }

        /// <summary>
        /// 在词语的编码中，是否是每个单字一个编码，比如拼音就是每个字一个编码，而无比则不是。
        /// </summary>
        bool Is1Char1Code { get; }

        /// <summary>
        /// 获得一个字的默认编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string GetDefaultCodeOfChar(char str);

        /// <summary>
        /// 获得一个词的编码,如果MutiCode==True，那么返回的是一个词的多种编码方式，如果为False，那么返回的是每个字的编码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="charCodeSplit">一个词中的各个字的编码之间的分隔符，默认不分割 </param>
        /// <returns></returns>
        IList<string> GetCodeOfString(string str, string charCodeSplit = "");

        /// <summary>
        /// 获得一个字的所有编码。比如多音字，一字多码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IList<string> GetCodeOfChar(char str);
    }
}