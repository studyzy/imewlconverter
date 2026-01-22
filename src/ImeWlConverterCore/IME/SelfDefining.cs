/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME;

[ComboBoxShow(ConstantString.SELF_DEFINING, ConstantString.SELF_DEFINING_C, 2000)]
public class SelfDefining
    : BaseTextImport,
        IWordLibraryTextImport,
        IWordLibraryExport,
        IStreamPrepare
{
    private IWordCodeGenerater codeGenerater;

    public ParsePattern UserDefiningPattern { get; set; }

    public override CodeType CodeType => UserDefiningPattern.CodeType;

    #region 根据字符串生成WL

    /// <summary>
    ///     根据Pattern设置的格式，对输入的一行该格式的字符串转换成WordLibrary
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public WordLibrary BuildWordLibrary(string line)
    {
        var wl = new WordLibrary();
        wl.CodeType = UserDefiningPattern.CodeType;
        var strlist = line.Split(
            new[] { UserDefiningPattern.SplitString },
            StringSplitOptions.RemoveEmptyEntries
        );
        
        // 边界检查: 确保至少有一个元素
        if (strlist.Length == 0)
        {
            Debug.WriteLine($"警告: 无效的行格式，分割后无元素: {line}");
            return null;
        }
        
        var newSort = new List<int>(UserDefiningPattern.Sort);
        newSort.Sort();
        string code = "",
            word = "";
        var rank = 0;

        var index1 = UserDefiningPattern.Sort.FindIndex(i => i == newSort[0]); //最小的一个
        if (index1 == 0 && UserDefiningPattern.ContainCode) //第一个是编码
            code = strlist[0];
        if (index1 == 1) //第一个是汉字
            word = strlist[0];
        if (index1 == 2 && UserDefiningPattern.ContainRank) //第一个是词频
            rank = Convert.ToInt32(strlist[0]);
        if (strlist.Length > 1)
        {
            var index2 = UserDefiningPattern.Sort.FindIndex(i => i == newSort[1]); //中间的一个
            if (index2 == 0 && UserDefiningPattern.ContainCode) //一个是Code
                code = strlist[1];
            if (index2 == 1) word = strlist[1];
            if (index2 == 2 && UserDefiningPattern.ContainRank) rank = Convert.ToInt32(strlist[1]);
        }

        if (strlist.Length > 2)
        {
            var index2 = UserDefiningPattern.Sort.FindIndex(i => i == newSort[2]); //最大的一个
            if (index2 == 0 && UserDefiningPattern.ContainCode) //第一个是拼音
                code = strlist[2];
            if (index2 == 1) word = strlist[2];
            if (index2 == 2 && UserDefiningPattern.ContainRank) rank = Convert.ToInt32(strlist[2]);
        }

        wl.Word = word;
        wl.Rank = rank;
        if (code != "")
        {
            if (UserDefiningPattern.IsPinyinFormat)
            {
                var codes = code.Split(
                    new[] { UserDefiningPattern.CodeSplitString },
                    StringSplitOptions.RemoveEmptyEntries
                );
                wl.SetCode(
                    UserDefiningPattern.CodeType,
                    new List<string>(codes),
                    UserDefiningPattern.IsPinyinFormat
                );
            }
            else
            {
                wl.SetCode(UserDefiningPattern.CodeType, code);
            }
        }

        return wl;
    }

    #endregion

    #region IWordLibraryExport Members

    private string lineFormat = "";

    public void Prepare()
    {
        codeGenerater = CodeTypeHelper.GetGenerater(UserDefiningPattern.CodeType);
        if (UserDefiningPattern.CodeType == CodeType.UserDefine)
        {
            if (string.IsNullOrEmpty(UserDefiningPattern.MappingTablePath))
                throw new Exception("未指定字符编码映射文件，无法对词库进行自定义编码的生成");
            var dict = UserCodingHelper.GetCodingDict(
                UserDefiningPattern.MappingTablePath,
                UserDefiningPattern.TextEncoding
            );
            var g = codeGenerater as SelfDefiningCodeGenerater;
            //如果 g 为 null，抛出更明确的错误
            if (g == null)
            {
                throw new InvalidOperationException("内部错误：无法为自定义编码类型获取正确的编码生成器");
            }
            //确保字典被正确赋值
            g.MappingDictionary = dict;
            g.Is1Char1Code = UserDefiningPattern.IsPinyinFormat;
            g.MutiWordCodeFormat = UserDefiningPattern.MutiWordCodeFormat;
        }

        BuildLineFormat();
    }

    /// <summary>
    ///     导出词库为自定义格式。
    ///     如果没有指定自定义编码文件，而且词库是包含拼音编码的，那么就按拼音编码作为每个字的码。
    ///     如果导出指定了自定义编码文件，那么就忽略词库的已有编码，使用自定义编码文件重新生成编码。
    ///     如果词库没有包含拼音编码，而且导出也没有指定编码文件，那就抛错吧~~~~
    /// </summary>
    /// <param name="wlList"></param>
    /// <returns></returns>
    public IList<string> Export(WordLibraryList wlList)
    {
        Prepare();
        var sb = new StringBuilder();
        foreach (var wordLibrary in wlList)
            try
            {
                sb.Append(ExportLine(wordLibrary));
                sb.Append(UserDefiningPattern.LineSplitString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        return new List<string> { sb.ToString() };
    }

    public string ExportLine(WordLibrary wl)
    {
        if (lineFormat == "") BuildLineFormat();
        var lines = new List<string>();
        //需要判断源WL与导出的字符串的CodeType是否一致，如果一致，那么可以采用其编码
        //调用CodeGenerater生成新的编码，并用新编码生成行
        //IList<string> codes = null;
        //修改：如果当前设置为自定义编码，则强制重新生成编码
        if (codeGenerater != null && UserDefiningPattern.CodeType == CodeType.UserDefine)
        {
            codeGenerater.GetCodeOfWordLibrary(wl);
        }
        var word = wl.Word;
        var rank = wl.Rank;
        foreach (
            var code in wl.Codes.ToCodeString(
                UserDefiningPattern.CodeSplitString,
                UserDefiningPattern.CodeSplitType
            )
        )
        {
            var line = string.Format(lineFormat, code, word, rank);
            lines.Add(line);
        }

        return string.Join(UserDefiningPattern.LineSplitString, lines.ToArray());
    }

    private void BuildLineFormat()
    {
        var dictionary = new Dictionary<int, string>();
        if (UserDefiningPattern.ContainCode) dictionary.Add(UserDefiningPattern.Sort[0], "{0}");
        if (UserDefiningPattern.ContainRank) dictionary.Add(UserDefiningPattern.Sort[2], "{2}");
        dictionary.Add(UserDefiningPattern.Sort[1], "{1}");
        var newSort = new List<int>(UserDefiningPattern.Sort);
        newSort.Sort();

        lineFormat = "";
        foreach (var x in newSort)
            if (dictionary.ContainsKey(x))
                lineFormat += dictionary[x] + UserDefiningPattern.SplitString;

        lineFormat = lineFormat.Substring(
            0,
            lineFormat.Length - UserDefiningPattern.SplitString.Length
        );
    }

    #endregion

    #region IWordLibraryTextImport Members

    public override Encoding Encoding => UserDefiningPattern.TextEncoding;

    public override WordLibraryList ImportLine(string line)
    {
        var wlList = new WordLibraryList();
        var wl = BuildWordLibrary(line);
        // 只添加非空的词库条目
        if (wl != null)
        {
            wlList.Add(wl);
        }
        return wlList;
    }

    #endregion
}
