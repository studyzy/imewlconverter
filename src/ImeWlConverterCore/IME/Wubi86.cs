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

namespace Studyzy.IMEWLConverter.IME;

/// <summary>
///     搜狗五笔的词库格式为“五笔编码 词语”\r\n
/// </summary>
[ComboBoxShow(ConstantString.WUBI86, ConstantString.WUBI86_C, 210)]
public class Wubi86 : BaseTextImport, IWordLibraryTextImport, IWordLibraryExport
{
    public override CodeType CodeType => CodeType.Wubi;

    #region IWordLibraryImport 成员

    public override WordLibraryList ImportLine(string line)
    {
        // 跳过空行和注释
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
        {
            return new WordLibraryList();
        }

        string word = null;
        string code = null;

        try
        {
            // 尝试多种分隔符：Tab、多个空格、单个空格
            string[] parts = null;

            // 优先尝试Tab分隔
            if (line.Contains('\t'))
            {
                parts = line.Split('\t');
            }
            // 尝试多个连续空格分隔
            else if (line.Contains("  "))
            {
                parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            // 尝试单个空格分隔
            else if (line.Contains(' '))
            {
                parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (parts == null || parts.Length < 2)
            {
                Debug.WriteLine($"行格式错误，无法解析: {line}");
                return new WordLibraryList();
            }

            // 去除首尾空白
            parts[0] = parts[0].Trim();
            parts[1] = parts[1].Trim();

            // 判断格式：如果第一部分全是小写字母且长度<=4，则是"编码 词语"格式
            // 否则是"词语 编码"格式
            if (IsValidWubiCode(parts[0]) && parts[0].Length <= 4)
            {
                // 格式: 编码 词语 (旧格式)
                code = parts[0];
                word = parts[1];
            }
            else if (IsValidWubiCode(parts[1]) && parts[1].Length <= 4)
            {
                // 格式: 词语 编码 (新格式，Issue #372)
                word = parts[0];
                code = parts[1];
            }
            else
            {
                Debug.WriteLine($"无效的五笔编码或词语: {line}");
                return new WordLibraryList();
            }

            // 验证词语和编码不为空
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(code))
            {
                Debug.WriteLine($"词语或编码为空: {line}");
                return new WordLibraryList();
            }

            var wl = new WordLibrary();
            wl.Word = word;
            wl.Rank = DefaultRank;
            wl.SetCode(CodeType.Wubi, code);

            var wll = new WordLibraryList();
            if (wl.PinYin.Length > 0)
            {
                wll.Add(wl);
            }
            else
            {
                Debug.WriteLine($"生成拼音失败，跳过词条: {word} {code}");
            }

            return wll;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"解析词条失败: {line}, 错误: {ex.Message}");
            return new WordLibraryList();
        }
    }

    /// <summary>
    /// 验证是否为有效的五笔编码（仅包含a-z小写字母，长度不超过4）
    /// </summary>
    private bool IsValidWubiCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length > 4)
            return false;

        return code.All(c => c >= 'a' && c <= 'z');
    }

    #endregion

    #region IWordLibraryExport 成员

    //private readonly IWordCodeGenerater wubiGenerater = new Wubi86Generater();

    public string ExportLine(WordLibrary wl)
    {
        var sb = new StringBuilder();

        sb.Append(wl.WubiCode);
        sb.Append(" ");
        sb.Append(wl.Word);

        return sb.ToString();
    }

    public IList<string> Export(WordLibraryList wlList)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < wlList.Count; i++)
            try
            {
                sb.Append(ExportLine(wlList[i]));
                sb.Append("\r\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        return new List<string> { sb.ToString() };
    }

    public override Encoding Encoding => Encoding.Unicode;

    #endregion

    //private readonly IWordCodeGenerater pinyinFactory = new PinyinGenerater();
}
