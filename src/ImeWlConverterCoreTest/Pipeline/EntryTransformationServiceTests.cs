#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.WordRank;
using ImeWlConverter.Core.CodeGeneration.Generators;
using ImeWlConverter.Core.Pipeline;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Pipeline;

/// <summary>
/// EntryTransformationService 五阶段处理测试：
/// Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty 的顺序与短路行为。
/// </summary>
public class EntryTransformationServiceTests
{
    private static EntryTransformationService CreateService(
        ICodeGenerator? generator = null,
        IChineseConverter? chineseConverter = null,
        IWordRankGenerator? wordRank = null)
    {
        return new EntryTransformationService(
            chineseConverter: chineseConverter,
            wordRankGenerator: wordRank,
            codeGenerationService: generator is null ? null : new CodeGenerationService([generator]));
    }

    private static readonly FilterPipeline NoFilters = new();

    [Fact]
    public async Task Apply_WithNoCodeGeneration_ReturnsEntriesUnchanged()
    {
        var service = CreateService();
        var entries = new List<WordEntry>
        {
            new() { Word = "你好", Rank = 1 },
            new() { Word = "世界", Rank = 2 },
        };

        var result = await service.ApplyAsync(
            entries, new ConversionOptions(), NoFilters, progress: null, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Null(e.Code));
    }

    [Fact]
    public async Task Apply_WithGenerator_AssignsCodesAndRemovesEmpty()
    {
        // 生成器：只有"好"能取码，"？"取不到码（空串）
        var generator = new FakeGenerator(new Dictionary<string, IReadOnlyList<IReadOnlyList<string>>>
        {
            ["好"] = [["hao"]],
        });
        var service = CreateService(generator);
        var options = new ConversionOptions
        {
            CodeGeneration = new CodeGenerationOptions { TargetCodeType = CodeType.Pinyin },
        };
        var entries = new List<WordEntry>
        {
            new() { Word = "好", Rank = 1 },
            new() { Word = "？", Rank = 2 },
        };

        var result = await service.ApplyAsync(
            entries, options, NoFilters, progress: null, CancellationToken.None);

        // "？"编码为空串被 RemoveEmpty 移除
        Assert.Single(result);
        Assert.Equal("好", result[0].Word);
        Assert.Equal("hao", result[0].Code!.Segments[0][0]);
    }

    [Fact]
    public async Task Apply_NoCodeGeneration_KeepsEntriesWithoutCode()
    {
        // NoCode 目标：RemoveEmpty 阶段被短路，无码词条保留
        var service = CreateService();
        var options = new ConversionOptions
        {
            CodeGeneration = new CodeGenerationOptions { TargetCodeType = CodeType.NoCode },
        };
        var entries = new List<WordEntry> { new() { Word = "任意", Rank = 1 } };

        var result = await service.ApplyAsync(
            entries, options, NoFilters, progress: null, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task Apply_WithFilter_FiltersBeforeCodeGen()
    {
        var generator = new FakeGenerator(new Dictionary<string, IReadOnlyList<IReadOnlyList<string>>>
        {
            ["好"] = [["hao"]],
            ["世界"] = [["shijie"]],
        });
        var service = CreateService(generator);
        var options = new ConversionOptions
        {
            CodeGeneration = new CodeGenerationOptions { TargetCodeType = CodeType.Pinyin },
        };
        var filters = new FilterPipeline(filters: [new MinLengthFilter(2)]);
        var entries = new List<WordEntry>
        {
            new() { Word = "好", Rank = 1 },     // 被长度过滤
            new() { Word = "世界", Rank = 2 },   // 保留
        };

        var result = await service.ApplyAsync(
            entries, options, filters, progress: null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("世界", result[0].Word);
    }

    [Fact]
    public async Task Apply_WithWordRank_AssignsRanksForMissing()
    {
        var service = CreateService(wordRank: new DefaultWordRankGenerator());
        var options = new ConversionOptions();
        var entries = new List<WordEntry> { new() { Word = "你好", Rank = 0 } };

        var result = await service.ApplyAsync(
            entries, options, NoFilters, progress: null, CancellationToken.None);

        Assert.True(result[0].Rank > 0);
    }

    [Fact]
    public async Task Apply_CancellationRequested_Throws()
    {
        var service = CreateService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ApplyAsync(
            new List<WordEntry> { new() { Word = "你好", Rank = 1 } },
            new ConversionOptions(), NoFilters, progress: null, cts.Token));
    }

    private sealed class FakeGenerator(IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<string>>> map)
        : ICodeGenerator
    {
        public CodeType SupportedType => CodeType.Pinyin;
        public bool Is1Char1Code => false;

        public WordCode GenerateCode(string word) => new()
        {
            Segments = map.TryGetValue(word, out var segments)
                ? segments
                : [word.Select(_ => "").ToArray()],
        };
    }

    private sealed class MinLengthFilter(int minLength) : IWordFilter
    {
        public bool ShouldKeep(WordEntry entry) => entry.Word.Length >= minLength;
    }
}
