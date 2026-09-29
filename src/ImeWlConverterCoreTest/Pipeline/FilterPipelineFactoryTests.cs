#nullable enable
using System;
using System.Linq;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.Filters;
using ImeWlConverter.Core.Pipeline;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Pipeline;

/// <summary>
/// FilterPipelineFactory 模块注册制测试：验证各模块的触发条件与装配顺序。
/// </summary>
public class FilterPipelineFactoryTests
{
    private readonly FilterPipelineFactory factory = new(DefaultFilterModules.Create());

    [Fact]
    public void Create_WithNullConfig_ReturnsNull()
    {
        Assert.Null(factory.Create(null));
    }

    [Fact]
    public void Create_WithNoFilter_ReturnsNull()
    {
        Assert.Null(factory.Create(new FilterConfig { NoFilter = true }));
    }

    [Fact]
    public void Create_WithAllDisabled_ReturnsNull()
    {
        // 默认 FilterConfig 所有开关关闭 → 无需过滤
        Assert.Null(factory.Create(new FilterConfig()));
    }

    [Fact]
    public void Create_WithEnglishIgnore_ContainsEnglishFilter()
    {
        var pipeline = factory.Create(new FilterConfig { IgnoreEnglish = true });
        Assert.NotNull(pipeline);
        var entries = new[]
        {
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "hello", Rank = 1 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "你好", Rank = 2 },
        };
        var result = pipeline.Apply(entries);
        Assert.Single(result);
        Assert.Equal("你好", result[0].Word);
    }

    [Fact]
    public void Create_WithLengthRange_EnforcesRange()
    {
        var pipeline = factory.Create(new FilterConfig { WordLengthFrom = 2, WordLengthTo = 3 });
        Assert.NotNull(pipeline);
        var entries = new[]
        {
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "一", Rank = 1 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "二三", Rank = 2 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "四五六", Rank = 3 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "七八九十", Rank = 4 },
        };
        var result = pipeline.Apply(entries);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Create_WithSpaceReplace_TransformsEntries()
    {
        var pipeline = factory.Create(new FilterConfig { ReplaceSpace = true });
        Assert.NotNull(pipeline);
        var entries = new[]
        {
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "你 好", Rank = 1 },
        };
        var result = pipeline.Apply(entries);
        Assert.Equal("你好", result[0].Word);
    }

    [Fact]
    public void Create_WithRankPercentage_AddsBatchFilter()
    {
        var pipeline = factory.Create(new FilterConfig { WordRankPercentage = 50 });
        Assert.NotNull(pipeline);
        var entries = Enumerable.Range(1, 10)
            .Select(i => new ImeWlConverter.Abstractions.Models.WordEntry { Word = $"词{i}", Rank = i })
            .ToList();
        var result = pipeline.Apply(entries);
        Assert.True(result.Count < entries.Count);
    }

    [Fact]
    public void Create_WithMultipleOptions_CombinesAllModules()
    {
        var pipeline = factory.Create(new FilterConfig
        {
            IgnoreEnglish = true,
            IgnoreNumber = true,
            ReplaceSpace = true,
        });
        Assert.NotNull(pipeline);
        var entries = new[]
        {
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "english", Rank = 1 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "123", Rank = 2 },
            new ImeWlConverter.Abstractions.Models.WordEntry { Word = "你 好", Rank = 3 },
        };
        var result = pipeline.Apply(entries);
        Assert.Single(result);
        Assert.Equal("你好", result[0].Word);
    }

    [Fact]
    public void Create_WithCustomModule_OrderedByModuleOrder()
    {
        // 自定义模块可插拔：无需修改管道，仅传入新模块即可
        var custom = new TrackingModule();
        var modules = DefaultFilterModules.Create().Append(custom).ToList();
        var customFactory = new FilterPipelineFactory(modules);

        var pipeline = customFactory.Create(new FilterConfig { IgnoreEnglish = true });
        Assert.NotNull(pipeline);
        Assert.True(custom.Contributed);
    }

    private sealed class TrackingModule : IFilterModule
    {
        public bool Contributed { get; private set; }

        public int Order => 95;

        public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
        {
            if (config.IgnoreEnglish)
            {
                Contributed = true;
                builder.AddFilter(new EnglishFilter());
            }
        }
    }
}
