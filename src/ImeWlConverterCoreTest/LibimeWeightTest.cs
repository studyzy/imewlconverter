using ImeWlConverter.Formats.LibIMEPinyin;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>libime 对数权重 ↔ 正整数词频的换算（issue #436）。</summary>
public class LibimeWeightTest
{
    [Theory]
    [InlineData(1, -1.0)]
    [InlineData(5, -0.30103)]
    [InlineData(10, 0.0)]
    [InlineData(100, 1.0)]
    [InlineData(350000, 4.544068)]
    public void FromRank_UsesFixedLogScale(int rank, double expected)
        => Assert.Equal(expected, LibimeWeight.FromRank(rank), 4);

    [Fact]
    public void FromRank_ClampsNonPositiveRanksToRankOne()
    {
        Assert.Equal(LibimeWeight.FromRank(1), LibimeWeight.FromRank(0));
        Assert.Equal(LibimeWeight.FromRank(1), LibimeWeight.FromRank(-5));
    }

    [Fact]
    public void FromRank_ClampsUpperBound()
        => Assert.Equal(LibimeWeight.MaxWeight, LibimeWeight.FromRank(int.MaxValue));

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1200)]
    [InlineData(350000)]
    [InlineData(1000000)]
    public void ToRank_InvertsFromRank(int rank)
        => Assert.Equal(rank, LibimeWeight.ToRank(LibimeWeight.FromRank(rank)));

    [Fact]
    public void ToRank_ReturnsZeroForNonFiniteWeights()
    {
        Assert.Equal(0, LibimeWeight.ToRank(float.NaN));
        Assert.Equal(0, LibimeWeight.ToRank(float.PositiveInfinity));
        Assert.Equal(0, LibimeWeight.ToRank(float.NegativeInfinity));
    }

    [Fact]
    public void ToRank_ClampsNegativeWeightToOne()
        => Assert.Equal(1, LibimeWeight.ToRank(-3f));

    [Fact]
    public void Format_IsRoundTrippableAndInvariant()
    {
        var weight = LibimeWeight.FromRank(5);
        var text = LibimeWeight.Format(weight);

        Assert.StartsWith("-0.30103", text);
        Assert.True(LibimeWeight.TryParse(text, out var parsed));
        Assert.Equal(weight, parsed);
    }

    [Theory]
    [InlineData("0", 0f)]
    [InlineData("1.25", 1.25f)]
    [InlineData("-3", -3f)]
    [InlineData("0.69897", 0.69897f)]
    public void TryParse_AcceptsSignedFloats(string text, float expected)
    {
        Assert.True(LibimeWeight.TryParse(text, out var weight));
        Assert.Equal((double)expected, weight, 5);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void TryParse_RejectsInvalidAndNonFinite(string text)
        => Assert.False(LibimeWeight.TryParse(text, out _));
}
