using System;
using System.Linq;
using System.Threading.Tasks;
using ImeWlConverter.CodeData;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>双拼映射表：按方案名索引，复现旧版按枚举序号取列会错位的回归。</summary>
public class ShuangpinTableTests
{
    private static ShuangpinTable CreateTable() => new(new EmbeddedResourceProvider());

    [Fact]
    public void Schemes_MatchesShuangpinTxtHeader()
    {
        var table = CreateTable();
        Assert.Equal(9, table.Schemes.Count);
        Assert.Contains("小鹤双拼", table.Schemes);
        Assert.Contains("自然码", table.Schemes);
        Assert.Contains("微软双拼", table.Schemes);
        Assert.DoesNotContain("全拼", table.Schemes);
    }

    [Theory]
    [InlineData("小鹤双拼", "hao", "hc")]
    [InlineData("小鹤双拼", "zhong", "vs")]
    [InlineData("自然码", "hao", "hk")]
    [InlineData("微软双拼", "hao", "hk")]
    [InlineData("微软双拼", "ying", "y;")] // 0984607 修复的 ing 韵母映射
    [InlineData("智能ABC", "zhong", "as")]
    [InlineData("拼音加加", "zhong", "vy")]
    public void GetMapping_KnownSyllable(string scheme, string syllable, string expected)
    {
        Assert.Equal(expected, CreateTable().GetMapping(scheme)[syllable]);
    }

    [Fact]
    public void GetMapping_UnknownScheme_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateTable().GetMapping("不存在的方案"));
        Assert.Contains("不存在的方案", ex.Message);
    }

    [Fact]
    public void GetMapping_IsBuiltOnceAndThreadSafe()
    {
        var table = CreateTable();
        var a = table.GetMapping("小鹤双拼");
        var b = table.GetMapping("小鹤双拼");
        Assert.Same(a, b);

        var results = new string[16];
        Parallel.For(0, results.Length, i => results[i] = table.GetMapping("微软双拼")["hao"]);
        Assert.All(results, s => Assert.Equal("hk", s));
    }

    [Fact]
    public void AllSchemes_HaveSameSyllableSet()
    {
        var table = CreateTable();
        var counts = table.Schemes.Select(s => table.GetMapping(s).Count).Distinct().ToArray();
        Assert.Single(counts);
        Assert.True(counts[0] > 300, $"映射条目过少：{counts[0]}");
    }
}
