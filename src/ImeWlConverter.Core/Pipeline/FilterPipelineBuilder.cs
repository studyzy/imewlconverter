using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// FilterPipeline 构建器：收集模块贡献的过滤器，按 单条过滤 → 变换 → 批量过滤 装配。
/// </summary>
public sealed class FilterPipelineBuilder
{
    private readonly List<IWordFilter> _filters = [];
    private readonly List<IWordTransform> _transforms = [];
    private readonly List<IBatchFilter> _batchFilters = [];

    public void AddFilter(IWordFilter filter) => _filters.Add(filter);

    public void AddTransform(IWordTransform transform) => _transforms.Add(transform);

    public void AddBatchFilter(IBatchFilter batchFilter) => _batchFilters.Add(batchFilter);

    /// <summary>装配 FilterPipeline；未贡献任何过滤器时返回 null（无需过滤）。</summary>
    public FilterPipeline? Build() =>
        _filters.Count == 0 && _transforms.Count == 0 && _batchFilters.Count == 0
            ? null
            : new FilterPipeline(_filters, _transforms, _batchFilters);
}
