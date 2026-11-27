#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.Pipeline;
using ImeWlConverter.Core.WordRank;
using ImeWlConverter.Formats.QQPhrase;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>QQ 输入法自定义短语（编码=排序位置,词语，UTF-16LE）导入导出测试。</summary>
public class QQPhraseTest
{
    private static string SamplePath(string fileName) =>
        Path.Combine(System.AppContext.BaseDirectory, "Test", fileName);

    [Fact]
    public async Task Importer_ReadsPinyinSample()
    {
        var importer = new QQPhraseImporter();
        await using var stream = File.OpenRead(SamplePath("qqpinyin-自定义短语.ini"));

        var result = await importer.ImportAsync(stream);

        var entry = Assert.Single(result.Entries);
        Assert.Equal("深蓝词库转换", entry.Word);
        Assert.Equal("slckzh", entry.Code!.GetPrimaryCode(""));
        Assert.Equal(1, entry.Rank);
        Assert.Equal(CodeType.UserDefinePhrase, entry.CodeType);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public async Task Importer_ReadsWubiSample_AndSkipsSymbolSets()
    {
        var importer = new QQPhraseImporter();
        await using var stream = File.OpenRead(SamplePath("qqwubi-自定义短语.ini"));

        var result = await importer.ImportAsync(stream);

        Assert.True(result.Entries.Count > 100);
        Assert.Equal(0, result.ErrorCount);

        var aStock = result.Entries.First(e => e.Word == "A股");
        Assert.Equal("aaem", aStock.Code!.GetPrimaryCode(""));
        Assert.Equal(2, aStock.Rank);

        // 词语中的逗号属于词条本身，不能被当作字段分隔符
        var time = result.Entries.First(e => e.Word.Contains(", $(AM/PM)"));
        Assert.Equal("time", time.Code!.GetPrimaryCode(""));
        Assert.Equal(3, time.Rank);

        // 「编码=$[...]」是 QQ 内置符号集，不是普通短语
        Assert.DoesNotContain(result.Entries, e => e.Code!.GetPrimaryCode("") == "zzbd");
        Assert.All(result.Entries, e => Assert.False(string.IsNullOrEmpty(e.Word)));
    }

    [Fact]
    public async Task Exporter_NumbersCandidatePositionsPerCode()
    {
        var entries = new List<WordEntry>
        {
            Entry("爱", "ai"),
            Entry("艾", "ai"),
            Entry("中国", "zhongguo"),
            Entry("碍", "ai"),
        };

        var lines = await ExportLinesAsync(entries);

        Assert.Equal(new[] { "ai=1,爱", "ai=2,艾", "zhongguo=1,中国", "ai=3,碍" }, lines);
    }

    [Fact]
    public async Task ExportImportedSample_AllEntriesSurvive()
    {
        var importer = new QQPhraseImporter();
        await using var input = File.OpenRead(SamplePath("qqwubi-自定义短语.ini"));
        var imported = await importer.ImportAsync(input);

        var exporter = new QQPhraseExporter();
        using var output = new MemoryStream();
        var result = await exporter.ExportAsync(imported.Entries, output);

        Assert.Equal(imported.Entries.Count, result.EntryCount);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public async Task Transformation_WithUserDefinePhraseTarget_KeepsMacroEntries()
    {
        var importer = new QQPhraseImporter();
        await using var input = File.OpenRead(SamplePath("qqwubi-自定义短语.ini"));
        var imported = await importer.ImportAsync(input);

        var service = new EntryTransformationService(
            wordRankGenerator: new DefaultWordRankGenerator(),
            codeGenerationService: new CodeGenerationService(System.Array.Empty<ICodeGenerator>()));
        var options = new ConversionOptions
        {
            CodeGeneration = new CodeGenerationOptions
            {
                TargetCodeType = CodeType.UserDefinePhrase
            }
        };

        var result = await service.ApplyAsync(imported.Entries, options, null, null, default);

        Assert.Equal(imported.Entries.Count, result.Count);
    }

    [Fact]
    public async Task Exporter_WritesUtf16LeWithBom()
    {
        var bytes = await ExportBytesAsync([Entry("深蓝词库转换", "slckzh")]);

        Assert.True(bytes.Length > 2);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xFE, bytes[1]);
    }

    [Fact]
    public async Task Exporter_SkipsEntriesWithoutCode()
    {
        var entries = new List<WordEntry> { Entry("无编码", null) };
        var exporter = new QQPhraseExporter();
        using var stream = new MemoryStream();

        var result = await exporter.ExportAsync(entries, stream);

        Assert.Equal(0, result.EntryCount);
    }

    private static WordEntry Entry(string word, string? code) => new()
    {
        Word = word,
        Rank = 0,
        CodeType = CodeType.UserDefinePhrase,
        Code = code is null ? null : WordCode.FromSingle(new[] { code })
    };

    private static async Task<string[]> ExportLinesAsync(IReadOnlyList<WordEntry> entries)
    {
        var bytes = await ExportBytesAsync(entries);
        using var reader = new StreamReader(new MemoryStream(bytes), Encoding.Unicode);
        var text = await reader.ReadToEndAsync();
        return text.Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    private static async Task<byte[]> ExportBytesAsync(IReadOnlyList<WordEntry> entries)
    {
        var exporter = new QQPhraseExporter();
        using var stream = new MemoryStream();
        await exporter.ExportAsync(entries, stream);
        return stream.ToArray();
    }
}
