#nullable enable
using System.IO;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Application.Cli;
using ImeWlConverter.Application.Mapping;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Core.WordRank;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Application;

/// <summary>
/// Issue #425 回归测试：CLI -r/--rank-generator 词频选项。
/// 旧版 -r:数字 指定固定词频（ForceUse=true 强制覆盖），新 CLI 必须等价支持。
/// </summary>
public class RankGeneratorOptionTests
{
    // ---------------- RankSpecParser ----------------

    [Theory]
    [InlineData("100", 100)]
    [InlineData("1", 1)]
    [InlineData(" 50 ", 50)]
    public void RankSpec_PositiveNumber_ParsesToFixedRank(string spec, int expected)
    {
        Assert.True(RankSpecParser.TryParse(spec, out var mode, out var rank, out var error));
        Assert.Equal(RankGeneratorMode.FixedRank, mode);
        Assert.Equal(expected, rank);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("llm")]
    [InlineData("LLM")]
    [InlineData(" Llm ")]
    public void RankSpec_LlmKeyword_ParsesToLlmMode(string spec)
    {
        Assert.True(RankSpecParser.TryParse(spec, out var mode, out _, out var error));
        Assert.Equal(RankGeneratorMode.Llm, mode);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("baidu")]
    [InlineData("google")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("")]
    [InlineData("   ")]
    public void RankSpec_InvalidValue_FailsWithError(string spec)
    {
        Assert.False(RankSpecParser.TryParse(spec, out _, out _, out var error));
        Assert.NotNull(error);
    }

    // ---------------- CliOptions 挂载 ----------------

    [Fact]
    public void RootCommand_AcceptsShortR_Option()
    {
        var parseResult = CliCommandFactory.Build().Parse("-r 100");
        Assert.Empty(parseResult.Errors);
        Assert.Equal("100", parseResult.GetValueForOption(CliOptions.RankGenerator));
    }

    [Fact]
    public void RootCommand_AcceptsLongRankGenerator_Option()
    {
        var parseResult = CliCommandFactory.Build().Parse("--rank-generator 100");
        Assert.Empty(parseResult.Errors);
        Assert.Equal("100", parseResult.GetValueForOption(CliOptions.RankGenerator));
    }

    [Fact]
    public void RootCommand_AcceptsLlmRankSpec_WithLlmOptions()
    {
        var parseResult = CliCommandFactory.Build().Parse(
            "-r llm --llm-endpoint https://api.example.com/v1 --llm-key sk-test --llm-model gpt-4o-mini");
        Assert.Empty(parseResult.Errors);
        Assert.Equal("llm", parseResult.GetValueForOption(CliOptions.RankGenerator));
        Assert.Equal("https://api.example.com/v1", parseResult.GetValueForOption(CliOptions.LlmEndpoint));
        Assert.Equal("sk-test", parseResult.GetValueForOption(CliOptions.LlmKey));
        Assert.Equal("gpt-4o-mini", parseResult.GetValueForOption(CliOptions.LlmModel));
    }

    // ---------------- Bootstrapper 覆盖注册 ----------------

    [Fact]
    public void Bootstrapper_ConfigureCallback_OverridesWordRankGenerator()
    {
        using var sp = ImeWlConverterBootstrapper.CreateServiceProvider(services =>
            services.AddSingleton<IWordRankGenerator>(
                new DefaultWordRankGenerator { DefaultRank = 100, ForceOverride = true }));

        var generator = sp.GetRequiredService<IWordRankGenerator>();
        var concrete = Assert.IsType<DefaultWordRankGenerator>(generator);
        Assert.Equal(100, concrete.DefaultRank);
        Assert.True(concrete.ForceOverride);
    }

    // ---------------- 固定词频生效（ForceOverride 语义，等价旧 -r:100） ----------------

    [Fact]
    public async Task DefaultWordRankGenerator_ForceOverride_AppliesRankToAllEntries()
    {
        var generator = new DefaultWordRankGenerator { DefaultRank = 100, ForceOverride = true };
        var entries = new[]
        {
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "你好", Rank = 0 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "世界", Rank = 7 },
        };

        var result = await generator.GenerateRanksAsync(entries);

        Assert.All(result, e => Assert.Equal(100, e.Rank));
    }
}
