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
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME;

public abstract class BaseTextImport : BaseImport
{
    public abstract Encoding Encoding { get; }
    public abstract WordLibraryList ImportLine(string line);

    public WordLibraryList Import(string path)
    {
        // 对于大文件使用流式处理避免内存溢出
        if (FileOperationHelper.ShouldUseStreaming(path))
        {
            return ImportStreaming(path);
        }
        else
        {
            var str = FileOperationHelper.ReadFile(path);
            return ImportText(str);
        }
    }

    /// <summary>
    ///     流式读取大文件,逐行处理避免内存溢出
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>词库列表</returns>
    protected virtual WordLibraryList ImportStreaming(string path)
    {
        var wlList = new WordLibraryList();
        var encoding = Encoding ?? FileOperationHelper.GetEncodingType(path);

        using (var sr = FileOperationHelper.GetStreamReader(path, encoding))
        {
            if (sr == null) return wlList;

            // 先统计行数用于进度显示
            var lineCount = 0;
            while (sr.ReadLine() != null) lineCount++;
            sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            sr.DiscardBufferedData();

            CountWord = lineCount;
            var currentLine = 0;

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                CurrentStatus = currentLine++;
                try
                {
                    if (IsContent(line))
                    {
                        var words = ImportLine(line);
                        if (words != null && words.Count > 0)
                        {
                            wlList.AddWordLibraryList(words);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SendImportLineErrorNotice($"无效的词条，解析失败：{line}. 错误: {ex.Message}");
                }

                // 定期触发GC以释放内存
                if (currentLine % 10000 == 0)
                {
                    GC.Collect(0, GCCollectionMode.Optimized);
                }
            }
        }

        return wlList;
    }

    protected virtual bool IsContent(string line)
    {
        return true;
    }

    public WordLibraryList ImportText(string str)
    {
        var wlList = new WordLibraryList();
        var lines = str.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        CountWord = lines.Length;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            CurrentStatus = i;
            try
            {
                if (IsContent(line)) wlList.AddWordLibraryList(ImportLine(line));
            }
            catch
            {
                SendImportLineErrorNotice("无效的词条，解析失败：" + line);
            }
        }

        return wlList;
    }
}
