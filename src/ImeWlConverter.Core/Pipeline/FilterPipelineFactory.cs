using ImeWlConverter.Abstractions.Options;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// FilterPipeline 工厂：按 Order 遍历注册的 IFilterModule，由模块决定贡献哪些过滤器。
/// 替代 ConversionPipeline 中硬编码 12 种过滤器的 BuildFilterPipeline（开闭原则）。
/// </summary>
public sealed class FilterPipelineFactory
{
    private readonly IReadOnlyList<IFilterModule> _modules;

    public FilterPipelineFactory(IEnumerable<IFilterModule> modules)
    {
        _modules = modules.OrderBy(m => m.Order).ToList();
    }

    /// <summary>根据过滤配置构建 FilterPipeline；配置为空或 NoFilter 时返回 null。</summary>
    public FilterPipeline? Create(FilterConfig? config)
    {
        if (config is null || config.NoFilter) return null;

        var builder = new FilterPipelineBuilder();
        foreach (var module in _modules)
            module.Contribute(config, builder);

        return builder.Build();
    }
}
