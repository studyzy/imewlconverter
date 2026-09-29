using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Formats.SelfDefining;

namespace ImeWlConverter.Application.Requests;

/// <summary>
/// ConversionRequest 组装工厂：CLI 参数 → 管道请求的单一事实来源。
/// 规则：-c 提供时强制 UserDefine；未显式指定编码类型时从输出格式推断。
/// </summary>
public static class ConversionRequestFactory
{
    public static Result<ConversionRequest> Create(
        CliConversionOptions options,
        IReadOnlyList<IFormatImporter> importers,
        IReadOnlyList<IFormatExporter> exporters)
    {
        if (!Mapping.FilterSpecParser.TryParse(options.FilterSpec, out var filterConfig, out var filterError))
            return Result<ConversionRequest>.Failure(filterError!);

        if (!Mapping.SelfDefiningSpecParser.TryParse(options.CustomFormat, out var selfSpec, out var specError))
            return Result<ConversionRequest>.Failure(specError!);

        // self 格式配置应用到导入/导出器实例
        if (selfSpec is not null)
        {
            foreach (var imp in importers.OfType<SelfDefiningImporter>())
                Apply(imp, selfSpec);
            foreach (var exp in exporters.OfType<SelfDefiningExporter>())
                Apply(exp, selfSpec);
        }

        var targetCodeType = Mapping.CodeTypeMapper.Parse(options.CodeType);

        // 提供码表文件时强制自定义编码
        if (!string.IsNullOrEmpty(options.CodeFile))
            targetCodeType = CodeType.UserDefine;

        // 未显式指定编码类型时从输出格式推断
        if (targetCodeType == CodeType.NoCode)
            targetCodeType = Mapping.CodeTypeMapper.InferFromOutputFormat(options.OutputFormatId, options.CustomFormat);

        return Result<ConversionRequest>.Success(new ConversionRequest
        {
            InputFormatId = options.InputFormatId,
            OutputFormatId = options.OutputFormatId,
            InputPaths = options.InputFiles,
            OutputPath = options.OutputPath,
            FilterConfig = filterConfig,
            Options = new ConversionOptions
            {
                CodeGeneration = new CodeGenerationOptions
                {
                    TargetCodeType = targetCodeType,
                    CodeFilePath = options.CodeFile,
                    MultiCodeFormat = options.MultiCode
                },
                Export = new ExportOptions
                {
                    DictionaryId = options.DictId,
                    DictionaryName = options.DictName,
                    DictionaryCategory = options.DictCategory,
                    DictionaryDescription = options.DictDescription
                }
            }
        });
    }

    private static void Apply(SelfDefiningImporter importer, Mapping.SelfDefiningSpec spec)
    {
        importer.OrderSpec = spec.OrderSpec;
        importer.PinyinSeparator = spec.PinyinSeparator;
        importer.FieldSeparator = spec.FieldSeparator;
        importer.ShowPinyin = spec.ShowPinyin;
        importer.ShowWord = spec.ShowWord;
        importer.ShowRank = spec.ShowRank;
    }

    private static void Apply(SelfDefiningExporter exporter, Mapping.SelfDefiningSpec spec)
    {
        exporter.OrderSpec = spec.OrderSpec;
        exporter.PinyinSeparator = spec.PinyinSeparator;
        exporter.FieldSeparator = spec.FieldSeparator;
        exporter.ShowPinyin = spec.ShowPinyin;
        exporter.ShowWord = spec.ShowWord;
        exporter.ShowRank = spec.ShowRank;
    }
}
