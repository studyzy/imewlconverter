using ImeWlConverter.Core;
using ImeWlConverter.Formats;
using Microsoft.Extensions.DependencyInjection;

namespace ImeWlConverter.Application.Bootstrap;

/// <summary>
/// 三端共用的 DI 组装入口（消灭 CLI/Win/macOS 各自复制一份组合根代码）。
/// </summary>
public static class ImeWlConverterBootstrapper
{
    /// <summary>
    /// 构建包含全部格式与核心服务的 ServiceProvider。
    /// <paramref name="configure"/> 在核心服务注册之后执行，可用于覆盖个别服务
    /// （如 CLI -r 指定的固定词频生成器，后注册者胜出）。
    /// </summary>
    public static ServiceProvider CreateServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddAllFormats();
        services.AddImeWlConverterCore();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    /// <summary>在既有容器上注册全部格式与核心服务。</summary>
    public static IServiceCollection AddImeWlConverter(this IServiceCollection services)
    {
        services.AddAllFormats();
        services.AddImeWlConverterCore();
        return services;
    }
}
