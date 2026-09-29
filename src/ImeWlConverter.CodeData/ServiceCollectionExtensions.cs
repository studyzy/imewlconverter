using Microsoft.Extensions.DependencyInjection;

namespace ImeWlConverter.CodeData;

/// <summary>
/// CodeData 服务的 DI 注册扩展。由 AddImeWlConverterCore() 内部调用，三端无需单独注册。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>注册全部码表数据服务（只读、线程安全、惰性构建）。</summary>
    public static IServiceCollection AddImeWlConverterCodeData(this IServiceCollection services)
    {
        services.AddSingleton<IResourceProvider, EmbeddedResourceProvider>();
        services.AddSingleton<ICodeTableLibrary, CodeTableLibrary>();
        services.AddSingleton<IPinyinTable, PinyinTable>();
        services.AddSingleton<IZhuyinTable, ZhuyinTable>();
        services.AddSingleton<IChaoyinTable, ChaoyinTable>();
        return services;
    }
}
