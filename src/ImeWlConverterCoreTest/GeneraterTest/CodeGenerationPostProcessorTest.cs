using System.Collections.Generic;
using System.Linq;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using Xunit;

namespace ImeWlConverterCoreTest.GeneraterTest;

public class CodeGenerationPostProcessorTest
{
    private static WordEntry MakeEntry(string word, params string[] codes)
    {
        var segments = codes.Select(c => (IReadOnlyList<string>)new[] { c }).ToList();
        return new WordEntry
        {
            Word = word,
            Code = new WordCode { Segments = segments },
            CodeType = CodeType.Pinyin
        };
    }

    [Fact]
    public void Apply_DefaultOptions_NoChange()
    {
        var options = new CodeGenerationOptions(); // 默认 KeepEnglish=true, KeepNumber=true, KeepPunctuation=false
        var entry = MakeEntry("我你", "wo", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("wo", result[0].Code!.Segments[0][0]);
        Assert.Equal("ni", result[0].Code!.Segments[1][0]);
    }

    [Fact]
    public void Apply_KeepNumberFalse_ClearsNumberSegments()
    {
        var options = new CodeGenerationOptions { KeepNumberInCode = false };
        var entry = MakeEntry("我3你", "wo", "3", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Empty(result[0].Code!.Segments[1]);
    }

    [Fact]
    public void Apply_KeepEnglishFalse_ClearsEnglishSegments()
    {
        var options = new CodeGenerationOptions { KeepEnglishInCode = false };
        var entry = MakeEntry("我a你", "wo", "a", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Empty(result[0].Code!.Segments[1]);
    }

    [Fact]
    public void Apply_KeepPunctuationFalse_ClearsPunctuationSegments()
    {
        var options = new CodeGenerationOptions(); // KeepPunctuationInCode 默认 false
        var entry = MakeEntry("我·你", "wo", "", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Empty(result[0].Code!.Segments[1]);
    }

    [Fact]
    public void Apply_KeepPunctuationTrue_KeepsPunctuationSegments()
    {
        var options = new CodeGenerationOptions { KeepPunctuationInCode = true };
        var entry = MakeEntry("我·你", "wo", "", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        // KeepPunctuationInCode=true 但 "·" 的编码本身就是空字符串，后处理器不清空它
        Assert.Equal("", result[0].Code!.Segments[1][0]);
    }

    [Fact]
    public void Apply_PrefixEnglishWithUnderscore_AddsUnderscore()
    {
        var options = new CodeGenerationOptions { PrefixEnglishWithUnderscore = true, KeepEnglishInCode = true };
        var entry = MakeEntry("我a你", "wo", "a", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("_a", result[0].Code!.Segments[1][0]);
        Assert.Equal("wo", result[0].Code!.Segments[0][0]);
    }

    [Fact]
    public void Apply_TranslateNumbersToChinese_ReplacesDigits()
    {
        var options = new CodeGenerationOptions { TranslateNumbersToChinese = true, KeepNumberInCode = true };
        var entry = MakeEntry("3楼", "3", "lou");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("三", result[0].Code!.Segments[0][0]);
        Assert.Equal("lou", result[0].Code!.Segments[1][0]);
    }

    [Fact]
    public void Apply_ConvertFullWidth_ConvertsToHalfWidth()
    {
        var options = new CodeGenerationOptions { ConvertFullWidth = true };
        // 全角编码内容 "ａ" → 半角 "a"
        var entry = MakeEntry("你", "ａ");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("a", result[0].Code!.Segments[0][0]);
    }

    [Fact]
    public void Apply_MixedEnglishAndNumber_AllOptionsApplied()
    {
        var options = new CodeGenerationOptions
        {
            KeepEnglishInCode = true,
            KeepNumberInCode = false,
            PrefixEnglishWithUnderscore = true,
            KeepPunctuationInCode = false
        };
        var entry = MakeEntry("a1·b", "a", "1", "", "b");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        var segments = result[0].Code!.Segments;
        Assert.Equal("_a", segments[0][0]);   // 英文 + 前缀
        Assert.Empty(segments[1]);             // 数字被清除
        Assert.Empty(segments[2]);             // 标点被清除
        Assert.Equal("_b", segments[3][0]);    // 英文 + 前缀
    }

    [Fact]
    public void Apply_EmptyCode_ReturnsUnchanged()
    {
        var options = new CodeGenerationOptions { KeepNumberInCode = false };
        var entry = new WordEntry
        {
            Word = "abc",
            Code = null,
            CodeType = CodeType.Pinyin
        };
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Null(result[0].Code);
    }
}
