using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.CodeData;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.CodeGeneration.Generators;
using ImeWlConverter.Core.Language;
using ImeWlConverter.Core.Pipeline;
using ImeWlConverter.Core.WordRank;
using Microsoft.Extensions.DependencyInjection;

namespace ImeWlConverter.Core;

/// <summary>
/// Extension methods for registering ImeWlConverter.Core services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core conversion services: pipeline, code generators, filters, converters.
    /// </summary>
    public static IServiceCollection AddImeWlConverterCore(this IServiceCollection services)
    {
        // 码表数据（只读、线程安全、惰性构建）
        services.AddImeWlConverterCodeData();

        // Pipeline
        services.AddSingleton<ConversionPipeline>();
        services.AddSingleton<IConversionPipeline>(sp => sp.GetRequiredService<ConversionPipeline>());
        services.AddSingleton<CodeGenerationService>();

        // Chinese converter
        services.AddSingleton<IChineseConverter, ChineseConverter>();

        // Word rank generator
        services.AddSingleton<IWordRankGenerator, DefaultWordRankGenerator>();

        // Code generators
        // 依赖链：Zhuyin/Chaoyin → TerraPinyin → Pinyin，因此具体类型需单独注册供注入
        services.AddSingleton<PinyinCodeGenerator>();
        services.AddSingleton<TerraPinyinCodeGenerator>();
        services.AddSingleton<ICodeGenerator>(sp => sp.GetRequiredService<PinyinCodeGenerator>());
        services.AddSingleton<ICodeGenerator>(sp => sp.GetRequiredService<TerraPinyinCodeGenerator>());
        services.AddSingleton<ICodeGenerator, Wubi86CodeGenerator>();
        services.AddSingleton<ICodeGenerator, Wubi98CodeGenerator>();
        services.AddSingleton<ICodeGenerator, WubiNewAgeCodeGenerator>();
        services.AddSingleton<ICodeGenerator, ZhengmaCodeGenerator>();
        services.AddSingleton<ICodeGenerator, Cangjie5CodeGenerator>();
        services.AddSingleton<ICodeGenerator, ZhuyinCodeGenerator>();
        services.AddSingleton<ICodeGenerator, ChaoyinCodeGenerator>();
        services.AddSingleton<ICodeGenerator, QingsongErbiCodeGenerator>();
        services.AddSingleton<ICodeGenerator, ChaoqiangErbiCodeGenerator>();
        services.AddSingleton<ICodeGenerator, XiandaiErbiCodeGenerator>();
        services.AddSingleton<ICodeGenerator, YinxingErbiCodeGenerator>();

        return services;
    }
}
