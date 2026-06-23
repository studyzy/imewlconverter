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

using Xunit;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Core.Filters;

namespace Studyzy.IMEWLConverter.Test.FilterTest;

/// <summary>
/// EnglishPunctuationFilter 正则分析：
///
/// 修复后正则：[-,~.?:;'"!`\^]|(-{2})|(\.{3})|(\(\))|(\[\])|(\{\})
/// （C# 字符串中 \\ 在正则中为 \，如 \\^ → \^）
///
/// 逐项分析：
///   [-,~.?:;'"!\`\^] — 字符类，匹配单个英文标点
///   (-{2})            — 双连字符 "--"
///   (\.{3})           — 省略号 "..."
///   (\(\))            — 括号对 "()"
///   (\[\])            — 方括号对 "[]"
///   (\{\})            — 花括号对 "{}"
///
/// 原正则 Bug（已修复）：
///   (/.{3})  → 用 / 而非 \ 转义，误匹配 /abc 等正斜杠+3字符
///   (/(/))   → 匹配字面量 "/()"，括号对 "()" 不被匹配
///   (/[/])   → 匹配字面量 "/[]"，方括号对 "[]" 不被匹配
///   ({})     → 只匹配空 {}，含内容的 {test} 不被匹配
/// </summary>
public class EnglishPunctuationFilterTest
{
    #region EnglishPunctuationFilter.ShouldKeep 测试

    /// <summary>
    /// 基本英文标点：字符类中的符号应被正确过滤
    /// </summary>
    [Theory]
    [InlineData("-", false)]
    [InlineData(",", false)]
    [InlineData("~", false)]
    [InlineData(".", false)]
    [InlineData("?", false)]
    [InlineData(":", false)]
    [InlineData(";", false)]
    [InlineData("'", false)]
    [InlineData("!", false)]
    [InlineData("^", false)]
    [InlineData("--", false)]
    [InlineData("abc!", false)]
    [InlineData("1.23", false)]
    [InlineData("a,b", false)]
    [InlineData("hello?", false)]
    [InlineData("深蓝", true)]
    [InlineData("abc", true)]
    [InlineData("ABC", true)]
    [InlineData("123", true)]
    [InlineData("abc123", true)]
    public void ShouldKeep_BasicPunctuation(string word, bool expected)
    {
        var entry = new WordEntry { Word = word };
        var filter = new EnglishPunctuationFilter();
        Assert.Equal(expected, filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 括号对 "()" 应被过滤
    /// </summary>
    [Fact]
    public void ShouldKeep_Parentheses_Filtered()
    {
        var entry = new WordEntry { Word = "()" };
        var filter = new EnglishPunctuationFilter();
        Assert.False(filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 方括号对 "[]" 应被过滤
    /// </summary>
    [Fact]
    public void ShouldKeep_SquareBrackets_Filtered()
    {
        var entry = new WordEntry { Word = "[]" };
        var filter = new EnglishPunctuationFilter();
        Assert.False(filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 花括号对 "{}" 应被过滤
    /// </summary>
    [Fact]
    public void ShouldKeep_CurlyBraces_Filtered()
    {
        var entry = new WordEntry { Word = "{}" };
        var filter = new EnglishPunctuationFilter();
        Assert.False(filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 含括号对的词条应被过滤
    /// </summary>
    [Theory]
    [InlineData("(test)", false)]
    [InlineData("a(b)c", false)]
    public void ShouldKeep_WordWithParentheses(string word, bool expected)
    {
        var entry = new WordEntry { Word = word };
        var filter = new EnglishPunctuationFilter();
        Assert.Equal(expected, filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 含方括号对的词条应被过滤
    /// </summary>
    [Theory]
    [InlineData("[test]", false)]
    [InlineData("a[b]c", false)]
    public void ShouldKeep_WordWithSquareBrackets(string word, bool expected)
    {
        var entry = new WordEntry { Word = word };
        var filter = new EnglishPunctuationFilter();
        Assert.Equal(expected, filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 含花括号对的词条应被过滤
    /// </summary>
    [Theory]
    [InlineData("{test}", false)]
    [InlineData("a{b}c", false)]
    public void ShouldKeep_WordWithCurlyBraces(string word, bool expected)
    {
        var entry = new WordEntry { Word = word };
        var filter = new EnglishPunctuationFilter();
        Assert.Equal(expected, filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 正斜杠后跟3字符不应被误匹配（修复前 /abc 会被误判为标点）
    /// </summary>
    [Fact]
    public void ShouldKeep_SlashFollowedByThreeChars_NotFiltered()
    {
        var entry = new WordEntry { Word = "/abc" };
        var filter = new EnglishPunctuationFilter();
        Assert.True(filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 含正斜杠的词条不应被过滤
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/a")]
    [InlineData("/ab")]
    [InlineData("a/b")]
    public void ShouldKeep_SlashNotFiltered(string word)
    {
        var entry = new WordEntry { Word = word };
        var filter = new EnglishPunctuationFilter();
        Assert.True(filter.ShouldKeep(entry));
    }

    /// <summary>
    /// 省略号 "..." 应被过滤（由 \.{3} 匹配）
    /// </summary>
    [Fact]
    public void ShouldKeep_Ellipsis_Filtered()
    {
        var entry = new WordEntry { Word = "..." };
        var filter = new EnglishPunctuationFilter();
        Assert.False(filter.ShouldKeep(entry));
    }

    #endregion

    #region EnglishPunctuationRemoveTransform.Transform 测试

    [Theory]
    [InlineData("abc!", "abc")]
    [InlineData("1.23", "123")]
    [InlineData("a,b,c", "abc")]
    [InlineData("hello?world", "helloworld")]
    public void Transform_RemoveBasicPunctuation(string input, string expectedWord)
    {
        var entry = new WordEntry { Word = input };
        var transform = new EnglishPunctuationRemoveTransform();
        var result = transform.Transform(entry);
        Assert.NotNull(result);
        Assert.Equal(expectedWord, result!.Word);
    }

    /// <summary>
    /// 括号对 () [] {} 应被移除
    /// </summary>
    [Theory]
    [InlineData("(test)", "test")]
    [InlineData("[test]", "test")]
    [InlineData("{test}", "test")]
    [InlineData("a(b)c", "abc")]
    [InlineData("a[b]c", "abc")]
    [InlineData("a{b}c", "abc")]
    public void Transform_RemoveBracketPairs(string input, string expectedWord)
    {
        var entry = new WordEntry { Word = input };
        var transform = new EnglishPunctuationRemoveTransform();
        var result = transform.Transform(entry);
        Assert.NotNull(result);
        Assert.Equal(expectedWord, result!.Word);
    }

    /// <summary>
    /// 省略号 ... 应被整体移除
    /// </summary>
    [Theory]
    [InlineData("hello...world", "helloworld")]
    [InlineData("a...b...c", "abc")]
    public void Transform_RemoveEllipsis(string input, string expectedWord)
    {
        var entry = new WordEntry { Word = input };
        var transform = new EnglishPunctuationRemoveTransform();
        var result = transform.Transform(entry);
        Assert.NotNull(result);
        Assert.Equal(expectedWord, result!.Word);
    }

    /// <summary>
    /// 正斜杠后跟3字符不应被移除（修复前会被误移除）
    /// </summary>
    [Fact]
    public void Transform_SlashFollowedByThreeChars_NotRemoved()
    {
        var entry = new WordEntry { Word = "/abc" };
        var transform = new EnglishPunctuationRemoveTransform();
        var result = transform.Transform(entry);
        Assert.NotNull(result);
        Assert.Equal("/abc", result!.Word);
    }

    /// <summary>
    /// 全标点词条移除后为空应返回 null
    /// </summary>
    [Theory]
    [InlineData("!")]
    [InlineData("...")]
    [InlineData("!?")]
    [InlineData("()")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void Transform_AllPunctuation_ReturnsNull(string input)
    {
        var entry = new WordEntry { Word = input };
        var transform = new EnglishPunctuationRemoveTransform();
        var result = transform.Transform(entry);
        Assert.Null(result);
    }

    #endregion
}
