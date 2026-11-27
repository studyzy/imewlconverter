using System;
using System.Collections.Generic;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Core.Filters;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>双拼变换：按音节替换、未知音节保留、非拼音编码不动。</summary>
public class ShuangpinReplacerTests
{
    private static readonly Dictionary<string, string> XiaoHe = new(StringComparer.Ordinal)
    {
        ["ni"] = "ni",
        ["hao"] = "hc",
        ["zhong"] = "vs",
        ["guo"] = "go"
    };

    [Fact]
    public void Transform_PinyinEntry_ReplacesEachSyllable()
    {
        var entry = new WordEntry
        {
            Word = "中国",
            Rank = 1,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(new[] { "zhong", "guo" })
        };

        var result = new ShuangpinReplacer(XiaoHe).Transform(entry);

        Assert.NotNull(result);
        Assert.Equal("vsgo", result!.Code!.GetPrimaryCode(""));
    }

    [Fact]
    public void Transform_UnknownSyllable_KeepsOriginal()
    {
        var entry = new WordEntry
        {
            Word = "未知",
            Rank = 1,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(new[] { "zzzz", "hao" })
        };

        var result = new ShuangpinReplacer(XiaoHe).Transform(entry);

        Assert.Equal("zzzzhc", result!.Code!.GetPrimaryCode(""));
    }

    [Fact]
    public void Transform_NonPinyinEntry_ReturnsUnchanged()
    {
        var entry = new WordEntry
        {
            Word = "你好",
            Rank = 1,
            CodeType = CodeType.UserDefine,
            Code = WordCode.FromSingle(new[] { "nihao" })
        };

        var result = new ShuangpinReplacer(XiaoHe).Transform(entry);

        Assert.Same(entry, result);
        Assert.Equal("nihao", result!.Code!.GetPrimaryCode(""));
    }

    [Fact]
    public void Transform_NullCode_ReturnsUnchanged()
    {
        var entry = new WordEntry { Word = "你好", Rank = 1, CodeType = CodeType.Pinyin };
        Assert.Same(entry, new ShuangpinReplacer(XiaoHe).Transform(entry));
    }
}
