using ImeWlConverter.Core.Pipeline.FilterModules;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// 内置过滤器模块清单（按 Order 排序由 FilterPipelineFactory 处理）。
/// 新增过滤器：实现 IFilterModule 并加入此清单 + DI 注册即可。
/// </summary>
public static class DefaultFilterModules
{
    public static IEnumerable<IFilterModule> Create()
    {
        yield return new EnglishFilterModule();
        yield return new FirstCjkFilterModule();
        yield return new LengthFilterModule();
        yield return new RankFilterModule();
        yield return new SpaceFilterModule();
        yield return new PunctuationFilterModule();
        yield return new NumberFilterModule();
        yield return new AlphabetCodeFilterModule();
        yield return new RankPercentageFilterModule();
    }
}
