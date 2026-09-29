using ImeWlConverter.Abstractions.Options;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// 过滤器模块：每个过滤器家族一个模块，按 Order 顺序向 builder 贡献过滤器/变换/批量过滤器。
/// 新增过滤器 = 新增一个 IFilterModule 实现类 + 一行 DI 注册，无需修改管道。
/// </summary>
public interface IFilterModule
{
    /// <summary>贡献顺序（小的先加入 FilterPipeline）。</summary>
    int Order { get; }

    /// <summary>根据配置贡献过滤器；配置未启用本模块功能时不做任何事。</summary>
    void Contribute(FilterConfig config, FilterPipelineBuilder builder);
}
