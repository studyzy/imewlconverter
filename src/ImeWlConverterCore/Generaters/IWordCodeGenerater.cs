using System.Collections.Generic;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Generaters
{
    /// <summary>
    ///     根据汉字输出其Code的接口,被IME下的类调用
    /// </summary>
    public interface IWordCodeGenerater
    {
        /// <summary>
        ///     该编码方式是否存在一字多码的情况，如果存在就调用GetCodeOfString，如果不存在或者忽略其他编码，就调用GetDefaultCodeOfString
        /// </summary>
        bool Is1CharMutiCode { get; }

        /// <summary>
        ///     在词语的编码中，是否是每个单字一个编码，比如拼音就是每个字一个编码，而五笔则不是。
        /// </summary>
        bool Is1Char1Code { get; }

        ///// <summary>
        /////     在生成编码时，是否可基于原有编码进行升级，比如不带声调的拼音升级为带声调的拼音
        ///// </summary>
        //bool IsBaseOnOldCode { get; }

        ///// <summary>
        /////     获得一个字的默认编码,如果找不到编码，则返回null
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //string GetDefaultCodeOfChar(char str);

        ///// <summary>
        /////     获得一个词的编码,如果MutiCode==True，那么返回的是一个词的多种编码方式，如果为False，那么返回的是每个字的编码
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        Code GetCodeOfString(string str);

        /// <summary>
        ///     获得一个词条的编码，可能会利用到词条的原编码
        /// </summary>
        /// <param name="wl"></param>
        /// <returns></returns>
       void GetCodeOfWordLibrary(WordLibrary wl);

        /// <summary>
        ///     获得一个字的所有编码。比如多音字，一字多码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IList<string> GetAllCodesOfChar(char str);
    }
}