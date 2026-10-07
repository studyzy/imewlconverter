using System.IO;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.QingJian;
using System.Threading.Tasks;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

public class QingJianTest
{
    /// <summary>
    /// 青简 TSV 导入：每行 词[TAB]拼音[TAB]词频，拼音音节间以空格分隔。
    /// </summary>
    [Fact]
    public async Task TestImport()
    {
        var text = "深蓝\tshen lan\t100\n输入法\tshu ru fa\t50\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var result = await new QingJianImporter().ImportAsync(stream);

        Assert.Equal(2, result.Entries.Count);
        var first = result.Entries[0];
        Assert.Equal("深蓝", first.Word);
        Assert.Equal(100, first.Rank);
        Assert.Equal(CodeType.Pinyin, first.CodeType);
        Assert.Equal("shen", first.Code!.Segments[0][0]);
        Assert.Equal("lan", first.Code.Segments[1][0]);

        var second = result.Entries[1];
        Assert.Equal("输入法", second.Word);
        Assert.Equal(50, second.Rank);
        Assert.Equal(3, second.Code!.Segments.Count);
    }

    /// <summary>
    /// 缺少拼音列或拼音为空的行应跳过，不产生词条。
    /// </summary>
    [Fact]
    public async Task TestImportInvalidLines()
    {
        var text = "只有词\n\t\t100\n好\thao\t7\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var result = await new QingJianImporter().ImportAsync(stream);

        var entry = Assert.Single(result.Entries);
        Assert.Equal("好", entry.Word);
        Assert.Equal(7, entry.Rank);
    }

    /// <summary>
    /// 青简官方文档：TSV 与 Rime 词库导入时，lue/nue 自动统一为 lve/nve。
    /// </summary>
    [Fact]
    public async Task TestImportNormalizesLueNue()
    {
        var text = "策略\tce lue\t10\n学习\txue xi\t5\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var result = await new QingJianImporter().ImportAsync(stream);

        Assert.Equal(2, result.Entries.Count);
        Assert.Equal("lve", result.Entries[0].Code!.Segments[1][0]);
        // 非 lue/nue 音节不受影响
        Assert.Equal("xi", result.Entries[1].Code!.Segments[1][0]);
    }

    /// <summary>
    /// 青简 TSV 导出：词[TAB]拼音[TAB]词频，音节以空格分隔，LF 行尾。
    /// </summary>
    [Fact]
    public async Task TestExport()
    {
        var entry = new WordEntry
        {
            Word = "深蓝",
            Code = WordCode.FromSingle(new[] { "shen", "lan" }),
            Rank = 100,
            CodeType = CodeType.Pinyin
        };
        using var stream = new MemoryStream();
        var result = await new QingJianExporter().ExportAsync(new[] { entry }, stream);

        Assert.Equal(1, result.EntryCount);
        var text = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("深蓝\tshen lan\t100\n", text);
    }

    /// <summary>
    /// 青简以 v 表示 ü：导出时 lue/nue 统一为 lve/nve。
    /// </summary>
    [Fact]
    public async Task TestExportNormalizesLueNue()
    {
        var entry = new WordEntry
        {
            Word = "策略",
            Code = WordCode.FromSingle(new[] { "ce", "lue" }),
            Rank = 10,
            CodeType = CodeType.Pinyin
        };
        using var stream = new MemoryStream();
        await new QingJianExporter().ExportAsync(new[] { entry }, stream);

        var text = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("策略\tce lve\t10\n", text);
    }

    /// <summary>
    /// 导出后再导入应保留词条与拼音。
    /// </summary>
    [Fact]
    public async Task TestRoundtrip()
    {
        var entries = new[]
        {
            new WordEntry
            {
                Word = "深蓝词库",
                Code = WordCode.FromSingle(new[] { "shen", "lan", "ci", "ku" }),
                Rank = 5,
                CodeType = CodeType.Pinyin
            },
            new WordEntry
            {
                Word = "策略",
                Code = WordCode.FromSingle(new[] { "ce", "lue" }),
                Rank = 10,
                CodeType = CodeType.Pinyin
            }
        };

        var exporter = new QingJianExporter();
        using var stream = new MemoryStream();
        await exporter.ExportAsync(entries, stream);

        stream.Position = 0;
        var imported = (await new QingJianImporter().ImportAsync(stream)).Entries;

        Assert.Equal("深蓝词库", imported[0].Word);
        Assert.Equal("shen", imported[0].Code!.Segments[0][0]);
        Assert.Equal("ku", imported[0].Code.Segments[3][0]);
        // lue 在导出时统一为 lve
        Assert.Equal("lve", imported[1].Code!.Segments[1][0]);
    }
}
