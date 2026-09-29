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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using ImeWlConverter.Core.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest;

public class FileOperationTest
{
    public FileOperationTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Theory]
    [InlineData("Test/u8nobomzy.txt", "UTF-8")]
    [InlineData("Test/luna_pinyin_export.txt", "UTF-8")]
    [InlineData("Test/gbzy.txt", "GB18030")]
    [InlineData("Test/QQPinyin.txt", "Unicode")]
    public void TestGetFileEncoding(string path, string encoding)
    {
        path = GetFullPath(path);
        var e = FileOperationHelper.GetEncodingType(path);
        Assert.Equal(Encoding.GetEncoding(encoding).EncodingName, e.EncodingName);
        var txt = FileOperationHelper.ReadFile(path);
    }

    [Fact]
    public void TestGetFileEncoding_Utf8NoBom_ShortChinese_ReturnsUtf8()
    {
        // Bug 回归：短中文 UTF-8（无 BOM）文件曾被 CharsetDetector 误判为 GBK 家族，
        // 读出"浠?"式乱码（词库合并窗口实证）。严格 UTF-8 结构校验修复后必须返回 UTF-8
        var content = "a wo 我\r\nb ni 你\r\n";
        var path = Path.Combine(Path.GetTempPath(), "imewl-enc-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(content));   // 无 BOM
        try
        {
            var e = FileOperationHelper.GetEncodingType(path);
            Assert.Equal(Encoding.UTF8.EncodingName, e.EncodingName);
            Assert.Equal(content, FileOperationHelper.ReadFile(path, e));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TestGetFileEncoding_GbkChinese_RoundTrips()
    {
        // 金丝雀：严格 UTF-8 校验不得把真正的 GBK 中文误判为 UTF-8（否则反方向乱码）
        var content = "深蓝词库转换是一款跨平台的输入法词库格式转换工具，支持五十多种输入法格式之间的相互转换。\r\n词库编码检测测试样例。\r\n";
        var path = Path.Combine(Path.GetTempPath(), "imewl-enc-" + Guid.NewGuid().ToString("N") + ".txt");
        var gbk = Encoding.GetEncoding(936);
        File.WriteAllBytes(path, gbk.GetBytes(content));
        try
        {
            var e = FileOperationHelper.GetEncodingType(path);
            var text = FileOperationHelper.ReadFile(path, e);
            Assert.Equal(content, text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TestGetFileEncoding_Utf16LeBom_ShortChinese_ReturnsUnicode()
    {
        var content = "a wo 我\r\nb ni 你\r\n";
        var path = Path.Combine(Path.GetTempPath(), "imewl-enc-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllBytes(path, Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(content)).ToArray());
        try
        {
            var e = FileOperationHelper.GetEncodingType(path);
            Assert.Equal(Encoding.Unicode.EncodingName, e.EncodingName);
            Assert.Equal(content, FileOperationHelper.ReadFile(path, e));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TestGetFileEncoding_AsciiOnly_RoundTrips()
    {
        var content = "line-of-text\r\nline2\r\n";
        var path = Path.Combine(Path.GetTempPath(), "imewl-enc-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, content, new UTF8Encoding(false));
        try
        {
            var e = FileOperationHelper.GetEncodingType(path);
            var text = FileOperationHelper.ReadFile(path, e);
            Assert.Equal(content, text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TestCodePagesEncodingProviderRequired()
    {
        // After registration, GB2312 encoding should be available
        Assert.Equal("Chinese Simplified (GB2312)", Encoding.GetEncoding("GB2312").EncodingName);
    }

    [Fact]
    public void TestWriteFile()
    {
        var path = GetFullPath("WriteTest.txt");
        var content = "Hello Word!";
        Assert.True(FileOperationHelper.WriteFile(path, Encoding.UTF8, content));
        Assert.True(File.Exists(path));
        File.Delete(path);
    }

    protected static string GetFullPath(string fileName)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, fileName);
    }
}
