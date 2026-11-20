using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Formats.BaiduBcd;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ImeWlConverterCoreTest;

/// <summary>
/// Issue #408 / #432: 百度手机输入法 bcd 词库的导入与导出。
/// 导出器只写常规词条区（0x350 起），文件头按手机 bcd 的固定布局生成。
/// </summary>
public class BaiduBcdTest
{
    static BaiduBcdTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static WordEntry Entry(string word, params string[] pinyin) => new()
    {
        Word = word,
        Rank = 1,
        CodeType = CodeType.Pinyin,
        Code = WordCode.FromSingle(pinyin)
    };

    [Fact]
    public void ExporterMetadata_IsBinaryBcd()
    {
        var exporter = new BaiduBcdExporter();

        Assert.Equal("bcd", exporter.Metadata.Id);
        Assert.True(exporter.Metadata.IsBinary);
        Assert.Equal(".bcd", exporter.Metadata.FileExtension);
        Assert.True(exporter.Metadata.SupportsExport);
    }

    [Fact]
    public void Export_WritesBcdHeaderAndCounts()
    {
        var exporter = new BaiduBcdExporter();
        var entries = new[]
        {
            Entry("你好", "ni", "hao"),
            Entry("绿色", "lv", "se")
        };
        using var ms = new MemoryStream();

        var result = exporter.ExportAsync(entries, ms, new ExportOptions
        {
            DictionaryName = "测试词库",
            DictionaryDescription = "测试描述"
        }).GetAwaiter().GetResult();

        var bytes = ms.ToArray();
        Assert.Equal(2, result.EntryCount);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal("biptbdsw", Encoding.ASCII.GetString(bytes, 0, 8));
        Assert.Equal(1u, BitConverter.ToUInt32(bytes, 0x08));
        Assert.Equal(2u, BitConverter.ToUInt32(bytes, 0x70));
        Assert.Equal(2u, BitConverter.ToUInt32(bytes, 0x250));
        Assert.Equal("测试词库", Encoding.Unicode.GetString(bytes, 0x90, 8));
        Assert.Equal(4u, BitConverter.ToUInt32(bytes, 0x80));
        // 首条词条: 拼音长 2、词频 300、拼音索引、UTF-16 词
        Assert.Equal(2, BitConverter.ToUInt16(bytes, 0x350));
        Assert.Equal(300, BitConverter.ToUInt16(bytes, 0x352));
        Assert.Equal("你好", Encoding.Unicode.GetString(bytes, 0x358, 4));
    }

    [Fact]
    public void ExportThenImport_RoundTripsPinyin()
    {
        var exporter = new BaiduBcdExporter();
        var importer = new BaiduBcdImporter();
        var entries = new[]
        {
            Entry("你好", "ni", "hao"),
            Entry("俾斯麦装甲", "bi", "si", "mai", "zhuang", "jia"),
            Entry("陆行鸟", "lu", "xing", "niao"),
            Entry("绿色", "lv", "se"),
            Entry("女孩", "nv", "hai"),
            Entry("月亮", "yue", "liang"),
            Entry("西安", "xi", "an"),
            Entry("女儿", "nv", "er"),
            Entry("OK", "O", "K")
        };
        using var ms = new MemoryStream();
        exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        ms.Position = 0;

        var imported = importer.ImportAsync(ms).GetAwaiter().GetResult().Entries;

        Assert.Equal(entries.Select(e => e.Word), imported.Select(e => e.Word));
        Assert.Equal(
            entries.Select(e => string.Join(",", e.Code!.Segments.Select(s => s[0]))),
            imported.Select(e => string.Join(",", e.Code!.Segments.Select(s => s[0]))));
    }

    [Fact]
    public void Export_NormalizesPinyinVariants()
    {
        var exporter = new BaiduBcdExporter();
        var importer = new BaiduBcdImporter();
        var entries = new[]
        {
            Entry("中国", "ZHONG1", "guo2"),
            Entry("略微", "lüe", "wei4"),
            Entry("虐待", "nüe", "dai4"),
            Entry("绿色", "lu:", "se4")
        };
        using var ms = new MemoryStream();
        exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        ms.Position = 0;

        var imported = importer.ImportAsync(ms).GetAwaiter().GetResult().Entries;

        Assert.Equal(["中国", "略微", "虐待", "绿色"], imported.Select(e => e.Word));
        Assert.Equal(
            ["zhong,guo", "lue,wei", "nue,dai", "lv,se"],
            imported.Select(e => string.Join(",", e.Code!.Segments.Select(s => s[0]))));
    }

    [Fact]
    public void Export_RealBcd_EntrySectionMatchesOfficialSample()
    {
        var path = Path.Combine(
            Path.GetDirectoryName(typeof(BaiduBcdTest).Assembly.Location)!,
            "Test", "记者必备.bcd");
        var original = File.ReadAllBytes(path);

        using var input = new MemoryStream(original);
        var entries = new BaiduBcdImporter().ImportAsync(input).GetAwaiter().GetResult().Entries;
        using var output = new MemoryStream();
        var result = new BaiduBcdExporter().ExportAsync(entries, output).GetAwaiter().GetResult();
        var generated = output.ToArray();

        Assert.Equal(entries.Count, result.EntryCount);
        Assert.Equal(0, result.ErrorCount);
        // 头部元数据不同，但 0x350 起的词条区应与官方样本逐字节一致
        Assert.Equal(original.Length, generated.Length);
        Assert.Equal(original.AsSpan(0x350).ToArray(), generated.AsSpan(0x350).ToArray());
    }

    [Fact]
    public void Export_SkipsEntriesThatCannotBeEncoded()
    {
        var exporter = new BaiduBcdExporter();
        var entries = new[]
        {
            Entry("你好", "ni", "hao"),
            Entry("测试", "abc", "shi"),          // 非法拼音音节
            new WordEntry { Word = "无编码", Rank = 1 }, // 没有 Code
            Entry("𠀀字", "a", "zi")               // 代理对，常规区无法 1:1 编码
        };
        using var ms = new MemoryStream();

        var result = exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();

        Assert.Equal(1, result.EntryCount);
        Assert.Equal(3, result.ErrorCount);
    }

    [Fact]
    public async Task Pipeline_SgpyToBcd_StreamOutput_ProducesBinary()
    {
        var src = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(
                src,
                Encoding.GetEncoding("GBK").GetBytes("'ni'hao 你好\r\n'lv'se 绿色\r\n"));

            using var sp = ImeWlConverterBootstrapper.CreateServiceProvider();
            var pipeline = sp.GetRequiredService<IConversionPipeline>();
            using var output = new MemoryStream();

            var result = await pipeline.ExecuteAsync(new ConversionRequest
            {
                InputFormatId = "sgpy",
                OutputFormatId = "bcd",
                InputPaths = [src],
                OutputStream = output
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.ExportedCount);
            Assert.Null(result.Value.ExportContent);
            Assert.NotNull(result.Value.ExportData);
            Assert.Equal(
                "biptbdsw",
                Encoding.ASCII.GetString(result.Value.ExportData!, 0, 8));

            using var reread = new MemoryStream(result.Value.ExportData!);
            var imported = new BaiduBcdImporter().ImportAsync(reread)
                .GetAwaiter().GetResult().Entries;
            Assert.Equal(["你好", "绿色"], imported.Select(e => e.Word));
        }
        finally
        {
            File.Delete(src);
        }
    }
}
