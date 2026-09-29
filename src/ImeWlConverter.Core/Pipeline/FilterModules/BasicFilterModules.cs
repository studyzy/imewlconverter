using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.Filters;

namespace ImeWlConverter.Core.Pipeline.FilterModules;

/// <summary>过滤英文词条（IgnoreEnglish）。</summary>
public sealed class EnglishFilterModule : IFilterModule
{
    public int Order => 10;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.IgnoreEnglish) builder.AddFilter(new EnglishFilter());
    }
}

/// <summary>过滤首字母非 CJK 的词条（IgnoreFirstCJK）。</summary>
public sealed class FirstCjkFilterModule : IFilterModule
{
    public int Order => 20;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.IgnoreFirstCJK) builder.AddFilter(new FirstCJKFilter());
    }
}

/// <summary>按词长过滤（WordLengthFrom/WordLengthTo）。</summary>
public sealed class LengthFilterModule : IFilterModule
{
    public int Order => 30;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.WordLengthFrom > 1 || config.WordLengthTo < 9999)
            builder.AddFilter(new LengthFilter
            {
                MinLength = config.WordLengthFrom,
                MaxLength = config.WordLengthTo
            });
    }
}

/// <summary>按词频序号过滤（WordRankFrom/WordRankTo）。</summary>
public sealed class RankFilterModule : IFilterModule
{
    public int Order => 40;

    public void Contribute(FilterConfig config, FilterPipelineBuilder builder)
    {
        if (config.WordRankFrom > 1 || config.WordRankTo < 999999)
            builder.AddFilter(new RankFilter
            {
                MinRank = config.WordRankFrom,
                MaxRank = config.WordRankTo
            });
    }
}
