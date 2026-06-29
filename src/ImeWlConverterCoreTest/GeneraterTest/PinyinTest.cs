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

using System.Linq;
using Xunit;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.CodeGeneration.Generators;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest;

public class PinyinTest
{
    private readonly ICodeGenerator generator;

    public PinyinTest()
    {
        generator = new PinyinCodeGenerator();
    }

    [Fact]
    public void TestGetOneWordPinyin()
    {
    }

    [Theory]
    [InlineData("曾毅", "zeng yi")]
    [InlineData("音乐", "yin yue")]
    [InlineData("快乐", "kuai le")]
    [InlineData("银行", "yin hang")]
    [InlineData("行走", "xing zou")]
    [InlineData("〇〇七", "ling ling qi")]
    public void TestGetLongWordsPinyin(string str, string py)
    {
        var result = generator.GenerateCode(str);
        var primaryCode = result.GetPrimaryCode(" ");
        Assert.Equal(py, primaryCode);
    }

    [Fact]
    public void TestPostProcessorKeepsIdeographicZero()
    {
        // Regression for #406: 〇 (U+3007) must be treated as CJK, not punctuation.
        // Previously IsCJK only covered U+4E00-U+9FFF, so 〇 was misclassified as
        // punctuation, its pinyin segment was cleared, and GetPrimaryCode threw
        // ArgumentOutOfRangeException (accessing s[0] on empty segment), causing
        // the whole entry to be silently dropped by the exporter.
        var code = generator.GenerateCode("〇〇七");
        var entries = new[] { new WordEntry { Word = "〇〇七", Code = code, CodeType = CodeType.Pinyin } }.ToList();
        var result = CodeGenerationPostProcessor.Apply(entries, new CodeGenerationOptions { TargetCodeType = CodeType.Pinyin });
        Assert.NotNull(result[0].Code);
        Assert.Equal("ling ling qi", result[0].Code!.GetPrimaryCode(" "));
    }
}
