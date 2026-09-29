using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.Filters;

namespace ImeWlConverter.Core.Pipeline.FilterModules;

/// <summary>空格过滤/移除（IgnoreSpace + ReplaceSpace）。</summary>
public sealed class SpaceFilterModule : IFilterModule
{
    public int Order => 50;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.IgnoreSpace) builder.AddFilter(new SpaceFilter());
        if (config.ReplaceSpace) builder.AddTransform(new SpaceRemoveTransform());
    }
}

/// <summary>标点过滤/移除（IgnorePunctuation + ReplacePunctuation）。</summary>
public sealed class PunctuationFilterModule : IFilterModule
{
    public int Order => 60;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.IgnorePunctuation)
        {
            builder.AddFilter(new ChinesePunctuationFilter());
            builder.AddFilter(new EnglishPunctuationFilter());
        }

        if (config.ReplacePunctuation)
        {
            builder.AddTransform(new EnglishPunctuationRemoveTransform());
            builder.AddTransform(new ChinesePunctuationRemoveTransform());
        }
    }
}

/// <summary>数字过滤/移除（IgnoreNumber + ReplaceNumber）。</summary>
public sealed class NumberFilterModule : IFilterModule
{
    public int Order => 70;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.IgnoreNumber) builder.AddFilter(new NumberFilter());
        if (config.ReplaceNumber) builder.AddTransform(new NumberRemoveTransform());
    }
}

/// <summary>过滤无字母编码的词条（IgnoreNoAlphabetCode）。</summary>
public sealed class AlphabetCodeFilterModule : IFilterModule
{
    public int Order => 80;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.IgnoreNoAlphabetCode) builder.AddFilter(new NoAlphabetCodeFilter());
    }
}

/// <summary>按词频百分比批量过滤（WordRankPercentage）。</summary>
public sealed class RankPercentageFilterModule : IFilterModule
{
    public int Order => 90;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.WordRankPercentage < 100)
            builder.AddBatchFilter(new RankPercentageFilter
            {
                Percentage = config.WordRankPercentage
            });
    }
}
