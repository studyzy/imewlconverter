using System.Collections.Generic;
using System.Linq;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.CodeGeneration.Generators;
using Xunit;

namespace ImeWlConverterCoreTest.GeneraterTest;

/// <summary>
/// 端到端测试：PinyinCodeGenerator 生成 + CodeGenerationPostProcessor 后处理。
/// 验证 Issue #406 的各个场景。
/// </summary>
public class PinyinCodeGenerationIntegrationTest
{
    private readonly ICodeGenerator pinyinGenerator = new PinyinCodeGenerator();

    private WordEntry GenerateAndProcess(string word, CodeGenerationOptions options)
    {
        var code = pinyinGenerator.GenerateCode(word);
        var entry = new WordEntry
        {
            Word = word,
            Code = code,
            CodeType = CodeType.Pinyin
        };
        var result = CodeGenerationPostProcessor.Apply([entry], options);
        return result[0];
    }

    /// <summary>
    /// Issue #406 场景1：英文前导下划线。
    /// "hello" 的编码应为 "_h_e_l_l_o"（PrefixEnglishWithUnderscore=true）。
    /// </summary>
    [Fact]
    public void EnglishWithUnderscorePrefix()
    {
        var options = new CodeGenerationOptions
        {
            KeepEnglishInCode = true,
            PrefixEnglishWithUnderscore = true
        };
        var result = GenerateAndProcess("hello", options);

        Assert.Equal("_h", result.Code!.Segments[0][0]);
        Assert.Equal("_e", result.Code!.Segments[1][0]);
        Assert.Equal("_l", result.Code!.Segments[2][0]);
        Assert.Equal("_l", result.Code!.Segments[3][0]);
        Assert.Equal("_o", result.Code!.Segments[4][0]);
    }

    /// <summary>
    /// Issue #406 场景1（续）：英文不保留时，编码段为空。
    /// </summary>
    [Fact]
    public void EnglishNotKept_EmptySegments()
    {
        var options = new CodeGenerationOptions { KeepEnglishInCode = false };
        var result = GenerateAndProcess("hello", options);

        // 所有英文字母编码被清空
        for (var i = 0; i < 5; i++)
            Assert.Empty(result.Code!.Segments[i]);
    }

    /// <summary>
    /// Issue #406 场景1（续）：中英混合词条，英文保留 + 下划线前缀。
    /// </summary>
    [Fact]
    public void MixedChineseEnglish_WithUnderscorePrefix()
    {
        var options = new CodeGenerationOptions
        {
            KeepEnglishInCode = true,
            PrefixEnglishWithUnderscore = true
        };
        var result = GenerateAndProcess("你ok", options);

        Assert.Equal("ni", result.Code!.Segments[0][0]);   // 中文正常
        Assert.Equal("_o", result.Code!.Segments[1][0]);    // 英文加前缀
        Assert.Equal("_k", result.Code!.Segments[2][0]);    // 英文加前缀
    }

    /// <summary>
    /// Issue #406 场景2：符号"·"生成空字符编码，后处理器清空。
    /// </summary>
    [Fact]
    public void SymbolDot_GeneratesEmptyCode()
    {
        var options = new CodeGenerationOptions(); // KeepPunctuationInCode 默认 false
        var result = GenerateAndProcess("我·你", options);

        // PinyinHelper 查不到"·"的拼音，返回空字符串
        // 后处理器清空标点 segment
        Assert.Equal("wo", result.Code!.Segments[0][0]);
        Assert.Empty(result.Code!.Segments[1]);     // 标点被清空
        Assert.Equal("ni", result.Code!.Segments[2][0]);
    }

    /// <summary>
    /// Issue #406 场景3：数字默认保留为编码。
    /// </summary>
    [Fact]
    public void NumberDefault_KeptInCode()
    {
        var options = new CodeGenerationOptions(); // KeepNumberInCode 默认 true
        var result = GenerateAndProcess("3楼", options);

        Assert.Equal("3", result.Code!.Segments[0][0]);
        Assert.Equal("lou", result.Code!.Segments[1][0]);
    }

    /// <summary>
    /// Issue #406 场景3：数字不编码时清空数字段。
    /// </summary>
    [Fact]
    public void NumberNotKept_EmptySegment()
    {
        var options = new CodeGenerationOptions { KeepNumberInCode = false };
        var result = GenerateAndProcess("3楼", options);

        Assert.Empty(result.Code!.Segments[0]);        // 数字被清空
        Assert.Equal("lou", result.Code!.Segments[1][0]); // 中文正常
    }

    /// <summary>
    /// Issue #406 场景3（续）：数字转中文数字。
    /// </summary>
    [Fact]
    public void NumberTranslateToChinese()
    {
        var options = new CodeGenerationOptions
        {
            KeepNumberInCode = true,
            TranslateNumbersToChinese = true
        };
        var result = GenerateAndProcess("3楼", options);

        Assert.Equal("三", result.Code!.Segments[0][0]);
        Assert.Equal("lou", result.Code!.Segments[1][0]);
    }

    /// <summary>
    /// Issue #406 场景4：多音字"银行"应读 yin hang（WordPinyin.txt 词组注音）。
    /// </summary>
    [Fact]
    public void MultiPinyinWord_Bank()
    {
        var options = new CodeGenerationOptions();
        var result = GenerateAndProcess("银行", options);

        var code = result.Code!.GetPrimaryCode(" ");
        Assert.Equal("yin hang", code);
    }

    /// <summary>
    /// Issue #406 场景4：多音字"行走"应读 xing zou（WordPinyin.txt 词组注音）。
    /// </summary>
    [Fact]
    public void MultiPinyinWord_Walk()
    {
        var options = new CodeGenerationOptions();
        var result = GenerateAndProcess("行走", options);

        var code = result.Code!.GetPrimaryCode(" ");
        Assert.Equal("xing zou", code);
    }

    /// <summary>
    /// Issue #406 场景4：混合中英词条"剑器行hang"，"行"后接英文 hang。
    /// 验证"行"取默认拼音 xing，hang 作为英文编码。
    /// </summary>
    [Fact]
    public void MixedChineseEnglish_MultiPinyinFallback()
    {
        var options = new CodeGenerationOptions
        {
            KeepEnglishInCode = true,
            PrefixEnglishWithUnderscore = true
        };
        var result = GenerateAndProcess("剑器行hang", options);

        Assert.Equal("jian", result.Code!.Segments[0][0]);
        Assert.Equal("qi", result.Code!.Segments[1][0]);
        Assert.Equal("xing", result.Code!.Segments[2][0]); // "行"默认拼音
        Assert.Equal("_h", result.Code!.Segments[3][0]);    // 英文加前缀
        Assert.Equal("_a", result.Code!.Segments[4][0]);
        Assert.Equal("_n", result.Code!.Segments[5][0]);
        Assert.Equal("_g", result.Code!.Segments[6][0]);
    }

    /// <summary>
    /// 完整场景：中英数标混合词条，后处理各选项生效。
    /// </summary>
    [Fact]
    public void FullMixedEntry_AllOptionsApplied()
    {
        var options = new CodeGenerationOptions
        {
            KeepEnglishInCode = true,
            KeepNumberInCode = false,
            KeepPunctuationInCode = false,
            PrefixEnglishWithUnderscore = true
        };
        var result = GenerateAndProcess("C3·楼", options);

        Assert.Equal("_c", result.Code!.Segments[0][0]);  // 英文 + 前缀
        Assert.Empty(result.Code!.Segments[1]);             // 数字被清空
        Assert.Empty(result.Code!.Segments[2]);             // 标点被清空
        Assert.Equal("lou", result.Code!.Segments[3][0]);   // 中文正常
    }

    /// <summary>
    /// 纯中文词条，默认选项不影响结果。
    /// </summary>
    [Fact]
    public void PureChinese_DefaultOptions_Unchanged()
    {
        var options = new CodeGenerationOptions();
        var result = GenerateAndProcess("深蓝词库", options);

        var code = result.Code!.GetPrimaryCode(" ");
        Assert.Equal("shen lan ci ku", code);
    }

    /// <summary>
    /// 数字 0-9 全部转中文验证。
    /// </summary>
    [Theory]
    [InlineData('0', "零")]
    [InlineData('1', "一")]
    [InlineData('5', "五")]
    [InlineData('9', "九")]
    public void TranslateNumber_EachDigit(char digit, string expected)
    {
        var options = new CodeGenerationOptions
        {
            KeepNumberInCode = true,
            TranslateNumbersToChinese = true
        };
        var result = GenerateAndProcess(digit.ToString(), options);

        Assert.Equal(expected, result.Code!.Segments[0][0]);
    }
}
