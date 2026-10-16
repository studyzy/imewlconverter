using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.MetasequoiaPinyin;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// 水杉输入法全拼词库测试。行格式与水杉批量导入一致：<c>词语\t全拼[\t权重]</c>。
/// </summary>
public class MetasequoiaPinyinTest
{
    private readonly MetasequoiaPinyinExporter _exporter = new();
    private readonly MetasequoiaPinyinImporter _importer = new();

    private static WordEntry Entry(string word, int rank, params string[] pinyin)
    {
        return new WordEntry
        {
            Word = word,
            Rank = rank,
            Code = WordCode.FromSingle(pinyin),
            CodeType = CodeType.Pinyin,
        };
    }

    private byte[] ExportBytes(params WordEntry[] entries)
    {
        using var ms = new MemoryStream();
        _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        return ms.ToArray();
    }

    private string Export(params WordEntry[] entries)
    {
        return new UTF8Encoding(false).GetString(ExportBytes(entries)).TrimStart('\uFEFF');
    }

    private IReadOnlyList<WordEntry> Import(string text)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return _importer.ImportAsync(ms).GetAwaiter().GetResult().Entries;
    }

    /// <summary>与水杉自身导出一致：UTF-8 带 BOM、Tab 分列、' 分音节、LF 换行。</summary>
    [Fact]
    public void Export_WritesTabSeparatedLineWithBom()
    {
        var bytes = ExportBytes(Entry("你好", 123, "ni", "hao"));
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
        Assert.Equal("你好\tni'hao\t123\n", Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3));
    }

    /// <summary>无词频（0，或管道补的 1）时写默认权重 100；真实词频原样保留。</summary>
    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 100)]
    [InlineData(2, 2)]
    [InlineData(50000, 50000)]
    public void Export_WeightDefaultsTo100WithoutRank(int rank, int expectedWeight)
    {
        Assert.Equal($"方案\tfang'an\t{expectedWeight}\n", Export(Entry("方案", rank, "fang", "an")));
    }

    /// <summary>ü 韵母统一写成 v（lue/nue → lve/nve）。</summary>
    [Fact]
    public void Export_NormalizesUmlaut()
    {
        Assert.Equal("绿色\tlv'se\t10\n", Export(Entry("绿色", 10, "lü", "se")));
        Assert.Equal("略\tlve\t10\n", Export(Entry("略", 10, "lue")));
        Assert.Equal("虐\tnve\t10\n", Export(Entry("虐", 10, "nüe")));
    }

    /// <summary>水杉会拒收的词条（码点数≠音节数、空音节、带声调、词面含 Tab）应跳过。</summary>
    [Fact]
    public void Export_SkipsEntriesMetasequoiaRejects()
    {
        Assert.Equal("", Export(
            Entry("芭芭拉·巴布科克", 1, "ba", "ba", "la", "ba", "bu", "ke", "ke"),
            Entry("你好", 1, "ni", ""),
            Entry("你好", 1, "ni3", "hao3"),
            Entry("a\tb", 1, "a", "b"),
            new WordEntry { Word = "无码", Rank = 1 }));
    }

    /// <summary>非 BMP 汉字按码点计数，不能因代理对被误判为两个字。</summary>
    [Fact]
    public void Export_CountsNonBmpCharAsOne()
    {
        Assert.Equal("𰻝𰻝面\tbiang'biang'mian\t100\n", Export(Entry("𰻝𰻝面", 1, "biang", "biang", "mian")));
    }

    /// <summary>可读取水杉导出的文件（带 BOM、可省略权重、跳过 # 注释），并与导出往返一致。</summary>
    [Fact]
    public void Import_ReadsMetasequoiaExportAndRoundTrips()
    {
        var entries = Import("\uFEFF# 注释\n你好\tni'hao\t10000\n方案\tfang'an\n\n");
        Assert.Equal(2, entries.Count);
        Assert.Equal("你好", entries[0].Word);
        Assert.Equal("ni'hao", entries[0].Code!.GetPrimaryCode("'"));
        Assert.Equal(10000, entries[0].Rank);
        Assert.Equal("fang'an", entries[1].Code!.GetPrimaryCode("'"));
        Assert.Equal(0, entries[1].Rank);

        Assert.Equal("你好\tni'hao\t10000\n方案\tfang'an\t100\n", Export(entries.ToArray()));
    }

    /// <summary>与水杉一致：非 2/3 列的行忽略；Rime userdb 的 c=… 权重列按无权重处理。</summary>
    [Theory]
    [InlineData("你好")]
    [InlineData("你好\tni'hao\t1\textra")]
    [InlineData("你好 ni'hao 1")]
    public void Import_MalformedLine_IsSkipped(string line)
    {
        Assert.Empty(Import(line));
    }

    [Fact]
    public void Import_NonNumericWeight_DefaultsToZero()
    {
        var entry = Assert.Single(Import("你好\tni'hao\tc=3 d=0.12 t=12345\n"));
        Assert.Equal(0, entry.Rank);
    }
}
