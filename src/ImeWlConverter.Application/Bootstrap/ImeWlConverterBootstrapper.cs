using ImeWlConverter.Core;
using ImeWlConverter.Formats;
using Microsoft.Extensions.DependencyInjection;

namespace ImeWlConverter.Application.Bootstrap;

/// <summary>
/// 三端共用的 DI 组装入口（消灭 CLI/Win/macOS 各自复制一份组合根代码）。
/// </summary>
public static class ImeWlConverterBootstrapper
{
    /// <summary>构建包含全部格式与核心服务的 ServiceProvider。</summary>
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAllFormats();
        services.AddImeWlConverterCore();
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
