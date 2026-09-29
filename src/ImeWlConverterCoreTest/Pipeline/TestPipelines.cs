#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.Pipeline;

namespace ImeWlConverterCoreTest;

/// <summary>
/// 测试用 ConversionPipeline 装配工厂：用默认过滤器模块与可选服务构造管道实例。
/// </summary>
internal static class TestPipelines
{
    internal static ConversionPipeline Create(
        IEnumerable<IFormatImporter> importers,
        IEnumerable<IFormatExporter> exporters,
        ICodeGenerator? codeGenerator = null,
        IChineseConverter? chineseConverter = null,
        IWordRankGenerator? wordRankGenerator = null,
        ImeWlConverter.Abstractions.Contracts.ISelfDefiningCodeSource? selfDefiningCodeSource = null)
    {
        var generators = codeGenerator is null ? [] : new[] { codeGenerator };
        var filterFactory = new FilterPipelineFactory(DefaultFilterModules.Create());
        var transformation = new EntryTransformationService(
            chineseConverter: chineseConverter,
            wordRankGenerator: wordRankGenerator,
            codeGenerationService: new CodeGenerationService(generators),
            selfDefiningCodeSource: selfDefiningCodeSource);
        return new ConversionPipeline(
            importers.ToList(),
            exporters.ToList(),
            filterFactory,
            transformation);
    }
}
