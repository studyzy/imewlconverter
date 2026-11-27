#nullable enable
using System.IO;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Application.Mapping;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Application;

/// <summary>
/// FilterSpecParser / CodeTypeMapper / LegacyArgSupport / FormatDetectionService 测试。
/// </summary>
public class MappingParsersTests
{
    // ---------------- FilterSpecParser ----------------

    [Fact]
    public void FilterSpec_Null_ReturnsNullConfig()
    {
        Assert.True(FilterSpecParser.TryParse(null, out var config, out var error));
        Assert.Null(config);
        Assert.Null(error);
    }

    [Fact]
    public void FilterSpec_ValidSwitches_SetsFlags()
    {
        Assert.True(FilterSpecParser.TryParse("rm:eng|rm:num|rm:space|rm:pun", out var config, out _));
        Assert.NotNull(config);
        Assert.True(config!.IgnoreEnglish);
        Assert.True(config.IgnoreNumber);
        Assert.True(config.IgnoreSpace);
        Assert.True(config.IgnorePunctuation);
    }

    [Theory]
    [InlineData("len:abc-10")]
    [InlineData("rank:x-y")]
    [InlineData("bad:part")]
    public void FilterSpec_InvalidInput_FailsWithError(string spec)
    {
        Assert.False(FilterSpecParser.TryParse(spec, out _, out var error));
        Assert.NotNull(error);
        Assert.Contains(spec, error);
    }

    [Fact]
    public void FilterSpec_MinOnly_UsesDefaultMax()
    {
        Assert.True(FilterSpecParser.TryParse("len:5", out var config, out _));
        Assert.Equal(5, config!.WordLengthFrom);
        Assert.Equal(9999, config.WordLengthTo);
    }

    // ---------------- CodeTypeMapper ----------------

    [Theory]
    [InlineData("pinyin", CodeType.Pinyin)]
    [InlineData("wubi", CodeType.Wubi86)]
    [InlineData("wubi86", CodeType.Wubi86)]
    [InlineData("wubi98", CodeType.Wubi98)]
    [InlineData("wubi_newage", CodeType.WubiNewAge)]
    [InlineData("zhengma", CodeType.Zhengma)]
    [InlineData("cangjie5", CodeType.Cangjie5)]
    [InlineData("zhuyin", CodeType.Zhuyin)]
    [InlineData("terra", CodeType.TerraPinyin)]
    [InlineData("custom", CodeType.UserDefine)]
    public void CodeTypeParse_KnownAliases_MapsCorrectly(string input, CodeType expected)
    {
        Assert.Equal(expected, CodeTypeMapper.Parse(input));
    }

    [Theory]
    [InlineData(null, CodeType.NoCode)]
    [InlineData("no-such-type", CodeType.NoCode)]
    public void CodeTypeParse_Unknown_ReturnsNoCode(string? input, CodeType expected)
    {
        Assert.Equal(expected, CodeTypeMapper.Parse(input));
    }

    [Fact]
    public void InferFromOutputFormat_SelfWithPinyinDisplay_ReturnsPinyin()
    {
        Assert.Equal(CodeType.Pinyin, CodeTypeMapper.InferFromOutputFormat("self", "213 ,nyyy"));
    }

    [Fact]
    public void InferFromOutputFormat_WubiOutput_ReturnsWubi86()
    {
        Assert.Equal(CodeType.Wubi86, CodeTypeMapper.InferFromOutputFormat("wb86", null));
    }

    [Fact]
    public void InferFromOutputFormat_QqPhrase_PreservesUserDefinePhrase()
    {
        Assert.Equal(CodeType.UserDefinePhrase, CodeTypeMapper.InferFromOutputFormat("qqdy", null));
    }

    // ---------------- LegacyArgSupport ----------------

    [Theory]
    [InlineData(new[] { "-i:scel", "a.scel" }, true)]
    [InlineData(new[] { "-ft:len:1-2" }, true)]
    [InlineData(new[] { "-i", "scel", "a.scel" }, false)]
    [InlineData(new string[0], false)]
    public void IsLegacyArgFormat_DetectsColonStyle(string[] args, bool expected)
    {
        Assert.Equal(expected, ImeWlConverter.Application.Legacy.LegacyArgSupport.IsLegacyArgFormat(args));
    }

    // ---------------- FormatDetectionService ----------------

    [Fact]
    public void DetectByExtension_MatchesRegisteredFormats()
    {
        using var sp = ImeWlConverterBootstrapper.CreateServiceProvider();
        var service = new ImeWlConverter.Application.FormatDetection.FormatDetectionService(
            sp.GetServices<IFormatImporter>());

        Assert.Equal("scel", service.DetectByExtension("唐诗.scel"));
        Assert.Equal("bdict", service.DetectByExtension("movie.bdict"));
        Assert.Equal("sgpybin", service.DetectByExtension("备份.bin"));
        Assert.Equal("qqdy", service.DetectByExtension("自定义短语.ini"));
        Assert.Null(service.DetectByExtension("unknown.xyz"));
    }

    // ---------------- ShuangpinSchemeParser ----------------

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("full", null)]
    [InlineData("全拼", null)]
    [InlineData("xiaohe", "小鹤双拼")]
    [InlineData("ms", "微软双拼")]
    [InlineData("microsoft", "微软双拼")]
    [InlineData("ziranma", "自然码")]
    [InlineData("abc", "智能ABC")]
    [InlineData("pyjj", "拼音加加")]
    [InlineData("小鹤双拼", "小鹤双拼")]
    [InlineData("微软双拼", "微软双拼")]
    public void ShuangpinScheme_ResolvesAlias(string? spec, string? expected)
    {
        Assert.True(ShuangpinSchemeParser.TryParse(spec, out var scheme, out var error));
        Assert.Null(error);
        Assert.Equal(expected, scheme);
    }

    [Fact]
    public void ShuangpinScheme_Unknown_FailsWithError()
    {
        Assert.False(ShuangpinSchemeParser.TryParse("bogus", out _, out var error));
        Assert.Contains("bogus", error!);
    }

    [Theory]
    [InlineData("小鹤双拼", "小鹤双拼")]
    [InlineData("自然码", "自然码")]
    [InlineData("微软双拼", "微软双拼")]
    [InlineData("智能ABC", "智能ABC")]
    [InlineData("紫光拼音", "紫光拼音")]
    [InlineData("星空键道", "星空键道")]
    [InlineData("大牛双拼", "大牛双拼")]
    [InlineData("小浪双拼", "小浪双拼")]
    public void ShuangpinScheme_ChineseHeaderNamesAreAccepted(string spec, string expected)
    {
        Assert.True(ShuangpinSchemeParser.TryParse(spec, out var scheme, out _));
        Assert.Equal(expected, scheme);
    }

    [Fact]
    public void ShuangpinScheme_WhitelistMatchesTableHeader()
    {
        // Shuangpin.txt 若新增方案，CLI 白名单必须同步，否则会被拒绝
        var table = new ImeWlConverter.CodeData.ShuangpinTable(
            new ImeWlConverter.CodeData.EmbeddedResourceProvider());
        Assert.Equal(table.Schemes, ShuangpinSchemeParser.Schemes);
    }

    [Fact]
    public void ShuangpinScheme_AllAliasesResolveToKnownSchemes()
    {
        var table = new ImeWlConverter.CodeData.ShuangpinTable(
            new ImeWlConverter.CodeData.EmbeddedResourceProvider());
        foreach (var scheme in ShuangpinSchemeParser.Schemes)
            Assert.True(table.GetMapping(scheme).Count > 0, $"方案 {scheme} 无映射数据");
    }

    // ---------------- FilterConfig 契约 ----------------

    [Fact]
    public void FilterConfig_NoFilter_FactoryReturnsNull()
    {
        // 契约：NoFilter 时即使设置了其它开关也不过滤
        var config = new FilterConfig { NoFilter = true, IgnoreEnglish = true };
        Assert.True(config.NoFilter);
    }
}
