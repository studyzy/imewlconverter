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

    [Fact]
    public void Apply_CjkExtensionA_KeepsPinyinSegments()
    {
        // Issue #424: CJK 扩展 A 区生僻字（㐖 U+3416）此前被 IsPunctuationOrSymbol
        // 误判为标点，词条自带拼音的首音节被清空（xie'du -> ''du）
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("\u3416\u6BD2", "xie", "du");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("xie", result[0].Code!.Segments[0][0]);
        Assert.Equal("du", result[0].Code!.Segments[1][0]);
    }

    [Fact]
    public void Apply_GenuineSymbolAfterExtensionA_StillCleared()
    {
        // 扩展 A 生僻字保留的同时，真正的标点仍按规则清除
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("\u3416·\u6BD2", "xie", "", "du");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("xie", result[0].Code!.Segments[0][0]);
        Assert.Empty(result[0].Code!.Segments[1]);
        Assert.Equal("du", result[0].Code!.Segments[2][0]);
    }

    [Fact]
    public void Apply_NonBmpCjkChar_KeepsPinyinSegments()
    {
        // BMP 之外的汉字（𫚉 U+2B689，CJK 扩展 B）在 UTF-16 中是代理对。
        // 此前按码元配对，代理对的两半被各自当成「标点」清空，
        // 且后续字与 segment 下标整体错位（hong 被清成 ''，niang 被丢弃）。
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("豹江𫚉娘", "bao", "jiang", "hong", "niang");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal(
            new[] { "bao", "jiang", "hong", "niang" },
            result[0].Code!.Segments.Select(s => s[0]).ToArray());
    }

    [Fact]
    public void Apply_ConsecutiveNonBmpCjkChars_KeepAllPinyinSegments()
    {
        // 𩽾𩾌 均为代理对，修复前整条词条的拼音被清空后按空编码丢弃
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("𩽾𩾌", "an", "kang");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Equal(
            new[] { "an", "kang" },
            result[0].Code!.Segments.Select(s => s[0]).ToArray());
    }

    [Fact]
    public void Apply_GenuineSymbolNextToNonBmpChar_StillCleared()
    {
        // 非 BMP 生僻字保留的同时，真正的标点仍按规则清除且不串位
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("𫚉·人", "hong", "", "ren");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("hong", result[0].Code!.Segments[0][0]);
        Assert.Empty(result[0].Code!.Segments[1]);
        Assert.Equal("ren", result[0].Code!.Segments[2][0]);
    }

    [Fact]
    public void Apply_AsciiDigitAfterNonBmpChar_ClearedWithoutDesync()
    {
        var options = new CodeGenerationOptions { KeepNumberInCode = false };
        var entry = MakeEntry("𫚉3娘", "hong", "3", "niang");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("hong", result[0].Code!.Segments[0][0]);
        Assert.Empty(result[0].Code!.Segments[1]);
        Assert.Equal("niang", result[0].Code!.Segments[2][0]);
    }

    [Fact]
    public void Apply_EnglishAfterNonBmpChar_UnderscoredWithoutDesync()
    {
        var options = new CodeGenerationOptions { PrefixEnglishWithUnderscore = true };
        var entry = MakeEntry("𫚉a娘", "hong", "a", "niang");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("hong", result[0].Code!.Segments[0][0]);
        Assert.Equal("_a", result[0].Code!.Segments[1][0]);
        Assert.Equal("niang", result[0].Code!.Segments[2][0]);
    }

    [Fact]
    public void Apply_SegmentsBeyondWordLength_ArePreserved()
    {
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("我", "wo", "ni");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Equal("wo", result[0].Code!.Segments[0][0]);
        Assert.Equal("ni", result[0].Code!.Segments[1][0]);
    }

    [Fact]
    public void Apply_UnpairedSurrogate_DoesNotThrow()
    {
        // 畸形输入：落单的代理项不应让后处理器抛异常（char.ConvertToUtf32 会抛）
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("\uD86D", "hong");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Single(result);
        Assert.Empty(result[0].Code!.Segments[0]);  // 仍按非中文标点处理
    }

    [Theory]
    [InlineData('\uE000')]   // 私用区起点
    [InlineData('\uE123')]   // 私用区中部（IME 生僻字常用段）
    [InlineData('\uF8FF')]   // 私用区终点
    [InlineData('\u2E80')]   // CJK 部首补充
    [InlineData('\u31C0')]   // CJK 笔画
    public void Apply_PrivateUseAndRadicalChars_KeepPinyinSegments(char rare)
    {
        // IME 词库用私用区承载生僻字，它们同样带拼音，不能被当标点清空
        var options = new CodeGenerationOptions();
        var entry = MakeEntry($"{rare}汗", "po", "han");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Equal("po", result[0].Code!.Segments[0][0]);
        Assert.Equal("han", result[0].Code!.Segments[1][0]);
    }

    [Fact]
    public void Apply_LibimeOmitsPunctuationPinyin_AlignsWithoutDesync()
    {
        // libime 文本「芭芭拉·巴布科克」8 个字、7 个音节（· 无声母），
        // 标点不占用 segment，其余音节必须逐位对齐而不是整体前移
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("芭芭拉·巴布科克", "ba", "ba", "la", "ba", "bu", "ke", "ke");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Equal(
            new[] { "ba", "ba", "la", "ba", "bu", "ke", "ke" },
            result[0].Code!.Segments.Select(s => s[0]).ToArray());
    }

    [Fact]
    public void Apply_GeneratedStylePunctuationSegment_StillHandled()
    {
        // 生成路径为每个码点都产出 segment（标点为空段），此时按 1:1 配对
        var options = new CodeGenerationOptions();
        var entry = MakeEntry("玛·萨拉", "ma", "", "sa", "la");
        var result = CodeGenerationPostProcessor.Apply([entry], options);

        Assert.Equal("ma", result[0].Code!.Segments[0][0]);
        Assert.Empty(result[0].Code!.Segments[1]);
        Assert.Equal("sa", result[0].Code!.Segments[2][0]);
        Assert.Equal("la", result[0].Code!.Segments[3][0]);
    }
}
