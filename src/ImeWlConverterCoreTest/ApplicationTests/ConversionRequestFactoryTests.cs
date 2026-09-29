#nullable enable
using System.Collections.Generic;
using System.Linq;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Application.Requests;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Formats;
using ImeWlConverter.Formats.SelfDefining;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Application;

/// <summary>
/// Application 层组装与请求工厂测试。
/// </summary>
public class ConversionRequestFactoryTests
{
    private static (IReadOnlyList<IFormatImporter> Importers, IReadOnlyList<IFormatExporter> Exporters) Resolve()
    {
        using var sp = ImeWlConverterBootstrapper.CreateServiceProvider();
        return (sp.GetServices<IFormatImporter>().ToList(), sp.GetServices<IFormatExporter>().ToList());
    }

    private static ImeWlConverter.Application.Requests.CliConversionOptions Options(
        string input = "scel", string output = "self",
        string? filter = null, string? customFormat = null, string? codeType = null)
    {
        return new CliConversionOptions
        {
            InputFormatId = input,
            OutputFormatId = output,
            OutputPath = "out.txt",
            InputFiles = new[] { "in.scel" },
            FilterSpec = filter,
            CustomFormat = customFormat,
            CodeType = codeType,
        };
    }

    [Fact]
    public void Create_ValidOptions_BuildsRequest()
    {
        var (importers, exporters) = Resolve();
        var result = ConversionRequestFactory.Create(
            Options(filter: "len:2-10|rm:eng"), importers, exporters);

        Assert.True(result.IsSuccess);
        var request = result.Value;
        Assert.Equal("scel", request.InputFormatId);
        Assert.NotNull(request.FilterConfig);
        Assert.Equal(2, request.FilterConfig!.WordLengthFrom);
        Assert.Equal(10, request.FilterConfig.WordLengthTo);
        Assert.True(request.FilterConfig.IgnoreEnglish);
    }

    [Fact]
    public void Create_InvalidFilter_ReturnsFailureWithError()
    {
        var (importers, exporters) = Resolve();
        var result = ConversionRequestFactory.Create(Options(filter: "len:abc-10"), importers, exporters);

        Assert.True(result.IsFailure);
        Assert.Contains("len:abc-10", result.Error);
    }

    [Fact]
    public void Create_InvalidCustomFormat_ReturnsFailureWithError()
    {
        var (importers, exporters) = Resolve();
        var result = ConversionRequestFactory.Create(Options(customFormat: "213"), importers, exporters);

        Assert.True(result.IsFailure);
        Assert.Contains("自定义格式", result.Error);
    }

    [Fact]
    public void Create_CodeFileForcesUserDefine()
    {
        var (importers, exporters) = Resolve();
        var options = Options() with { CodeFile = "codes.txt" };
        var result = ConversionRequestFactory.Create(options, importers, exporters);

        Assert.True(result.IsSuccess);
        Assert.Equal(ImeWlConverter.Abstractions.Enums.CodeType.UserDefine,
            result.Value.Options.CodeGeneration.TargetCodeType);
    }

    [Fact]
    public void Create_ExplicitCodeType_Wins()
    {
        var (importers, exporters) = Resolve();
        var result = ConversionRequestFactory.Create(Options(codeType: "wubi98"), importers, exporters);

        Assert.True(result.IsSuccess);
        Assert.Equal(ImeWlConverter.Abstractions.Enums.CodeType.Wubi98,
            result.Value.Options.CodeGeneration.TargetCodeType);
    }

    [Fact]
    public void Create_SelfFormatWithPinyin_InfersPinyinCodeType()
    {
        var (importers, exporters) = Resolve();
        var result = ConversionRequestFactory.Create(
            Options(customFormat: "213 ,nyyy"), importers, exporters);

        Assert.True(result.IsSuccess);
        Assert.Equal(ImeWlConverter.Abstractions.Enums.CodeType.Pinyin,
            result.Value.Options.CodeGeneration.TargetCodeType);
    }

    [Fact]
    public void Create_SelfFormat_AppliesSpecToImportersAndExporters()
    {
        var (importers, exporters) = Resolve();
        var result = ConversionRequestFactory.Create(
            Options(customFormat: "213 ,nyyy"), importers, exporters);

        Assert.True(result.IsSuccess);
        var importer = importers.OfType<SelfDefiningImporter>().Single();
        Assert.Equal("213", importer.OrderSpec);
        Assert.Equal(' ', importer.PinyinSeparator);
        Assert.Equal(',', importer.FieldSeparator);
        Assert.True(importer.ShowPinyin);
        Assert.True(importer.ShowWord);
        Assert.True(importer.ShowRank);
    }
}
