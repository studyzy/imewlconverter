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
using System.Linq;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME;

/// <summary>
///     RIME是一个输入法框架，支持多种输入法编码，词库规则是：
///     词语+Tab+编码（拼音空格隔开）+Tab+词频
/// </summary>
[ComboBoxShow(ConstantString.RIME, ConstantString.RIME_C, 150)]
public class Rime : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport, IMultiCodeType
{
    private string lineSplitString;

    private OperationSystem os;

    public Rime()
    {
        CodeType = CodeType.Pinyin;
        OS = OperationSystem.Windows;
    }

    public OperationSystem OS
    {
        get => os;
        set
        {
            os = value;
            lineSplitString = GetLineSplit(os);
        }
    }

    #region IWordLibraryImport 成员

    //private IWordCodeGenerater pyGenerater=new PinyinGenerater();
    public override WordLibraryList ImportLine(string line)
    {
        var lineArray = line.Split('\t');

        var word = lineArray[0];
        var code = lineArray[1];
        var wl = new WordLibrary();
        wl.Word = word;
        if (lineArray.Length >= 3) wl.Rank = Convert.ToInt32(lineArray[2]);
        if (CodeType == CodeType.Pinyin)
            wl.PinYin = code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        else
            //wl.PinYin = CollectionHelper.ToArray(pyGenerater.GetCodeOfString(wl.Word));
            wl.SetCode(CodeType, code);

        var wll = new WordLibraryList();
        wll.Add(wl);
        return wll;
    }

    #endregion

    private string GetLineSplit(OperationSystem os)
    {
        switch (os)
        {
            case OperationSystem.Windows:
                return "\r\n";

            case OperationSystem.MacOS:
                return "\r";

            case OperationSystem.Linux:
                return "\n";
        }

        return "\r\n";
    }

    #region IWordLibraryExport 成员

    private IWordCodeGenerater codeGenerater;

    //private RimeConfigForm form;

    public string ExportLine(WordLibrary wl)
    {
        var sb = new StringBuilder();
        if (
            CodeType == wl.CodeType
            && CodeType != CodeType.Pinyin
            && CodeType != CodeType.TerraPinyin
        )
            return wl.Word + "\t" + wl.Codes[0][0] + "\t" + wl.Rank;

        if (codeGenerater == null) codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
        try
        {
            codeGenerater.GetCodeOfWordLibrary(wl);
        }
        catch (Exception ex)
        {
            // 生成编码失败时，记录警告并抛出异常，以便上层调用者知晓
            Debug.WriteLine($"为词条 '{wl.Word}' 生成编码失败: {ex.Message}");
            throw new Exception($"无法为词条 '{wl.Word}' 生成拼音编码: {ex.Message}", ex);
        }

        // 检查是否成功生成了编码
        if (wl.Codes == null || wl.Codes.Count == 0 || string.IsNullOrEmpty(wl.SingleCode))
        {
            throw new Exception($"为词条 '{wl.Word}' 生成的拼音编码为空");
        }

        if (codeGenerater.Is1CharMutiCode)
        {
            var codes = codeGenerater.GetCodeOfString(wl.Word).ToCodeString(" ");
            var i = 0;
            foreach (var code in codes)
            {
                sb.Append(wl.Word);
                sb.Append("\t");
                sb.Append(code);
                sb.Append("\t");
                sb.Append(wl.Rank);
                i++;
                if (i != codes.Count)
                    sb.Append(lineSplitString);
            }
        }
        else
        {
            sb.Append(wl.Word);
            sb.Append("\t");
            if (CodeType == CodeType.Pinyin || CodeType == CodeType.TerraPinyin)
                sb.Append(wl.GetPinYinString(" ", BuildType.None));
            else if (CodeType == wl.CodeType)
                sb.Append(wl.Codes[0][0]);
            else
                sb.Append(wl.Codes.ToCodeString(" ")[0]);
            sb.Append("\t");
            sb.Append(wl.Rank);
        }

        return sb.ToString();
    }

    public IList<string> Export(WordLibraryList wlList)
    {
        codeGenerater = CodeTypeHelper.GetGenerater(CodeType);

        // 使用字典进行去重和词频合并
        var uniqueWords = new Dictionary<string, WordLibrary>();
        int duplicateCount = 0;
        int inputCount = wlList.Count;

        for (var i = 0; i < wlList.Count; i++)
        {
            var wl = wlList[i];

            // 先生成编码（如果需要）
            if (CodeType != wl.CodeType || CodeType == CodeType.Pinyin || CodeType == CodeType.TerraPinyin)
            {
                try
                {
                    if (codeGenerater == null) codeGenerater = CodeTypeHelper.GetGenerater(CodeType);
                    codeGenerater.GetCodeOfWordLibrary(wl);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"为词条 '{wl.Word}' 生成编码失败: {ex.Message}");
                    continue; // 跳过无法生成编码的词条
                }
            }

            // 获取词条的编码字符串
            string code;
            if (CodeType == CodeType.Pinyin || CodeType == CodeType.TerraPinyin)
            {
                code = wl.GetPinYinString(" ", BuildType.None);
            }
            else if (CodeType == wl.CodeType)
            {
                code = wl.Codes[0][0];
            }
            else
            {
                var codes = wl.Codes.ToCodeString(" ");
                code = codes.Count > 0 ? codes[0] : "";
            }

            // 创建唯一键: 词语+编码
            string key = $"{wl.Word}\t{code}";

            if (uniqueWords.ContainsKey(key))
            {
                // 发现重复，合并词频（取最大值）
                var existing = uniqueWords[key];
                existing.Rank = Math.Max(existing.Rank, wl.Rank);
                duplicateCount++;
                Debug.WriteLine($"合并重复词条: {wl.Word} {code} (词频: {existing.Rank})");
            }
            else
            {
                uniqueWords[key] = wl;
            }
        }

        // 输出统计信息
        if (duplicateCount > 0)
        {
            Debug.WriteLine($"检测到{duplicateCount}个重复词条，已自动合并");
            Debug.WriteLine($"输入词条: {inputCount}, 去重后: {uniqueWords.Count}");
        }

        // 生成输出
        var sb = new StringBuilder();

        // 按词频排序后输出
        var sortedWords = uniqueWords.Values
            .OrderByDescending(w => w.Rank)
            .ThenBy(w => w.Word);

        foreach (var wl in sortedWords)
        {
            var line = ExportLine(wl);
            if (!string.IsNullOrEmpty(line))
            {
                sb.Append(line);
                sb.Append(lineSplitString);
            }
        }

        return new List<string> { sb.ToString() };
    }

    public override Encoding Encoding => new UTF8Encoding(false);

    #endregion
}
