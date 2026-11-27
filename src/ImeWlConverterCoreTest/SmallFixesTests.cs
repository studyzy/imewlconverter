using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.NoPinyinWordOnly;
using ImeWlConverter.Formats.Rime;
using ImeWlConverter.Formats.UserPhrase;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// 回归：#185 用户自定义短语排位、#212 Rime 无编码词条、UTF-8 BOM 损坏首行。
/// </summary>
public class SmallFixesTests
{
    [Fact]
    public async Task DyExporter_NumbersCandidatePositionsPerCode()
    {
        var entries = new List<WordEntry>
        {
            Entry("爱", "ai", 0),
            Entry("艾", "ai", 0),
            Entry("碍", "ai", 0),
            Entry("中国", "zhongguo", 0),
            Entry("隘", "ai", 0),
        };
        var exporter = new UserDefinePhraseExporter();

        var lines = await ExportLinesAsync(exporter, entries);

        Assert.Equal(5, lines.Count);
        Assert.Equal("ai,1=爱", lines[0]);
        Assert.Equal("ai,2=艾", lines[1]);
        Assert.Equal("ai,3=碍", lines[2]);
        Assert.Equal("zhongguo,1=中国", lines[3]); // 不同编码重新从 1 开始
        Assert.Equal("ai,4=隘", lines[4]);
    }

    [Fact]
    public async Task DyExporter_SecondExportDoesNotLeakPositions()
    {
        var exporter = new UserDefinePhraseExporter();
        await ExportLinesAsync(exporter, [Entry("爱", "ai", 0)]);

        var lines = await ExportLinesAsync(exporter, [Entry("艾", "ai", 0)]);

        Assert.Single(lines);
        Assert.Equal("ai,1=艾", lines[0]);
    }

    [Fact]
    public async Task DyAndWordExporters_DoNotWriteUtf8Bom()
    {
        var entries = new List<WordEntry> { Entry("你好", "nihao", 1) };

        var dyBytes = await ExportBytesAsync(new UserDefinePhraseExporter(), entries);
        var wordBytes = await ExportBytesAsync(new NoPinyinWordOnlyExporter(), entries);

        Assert.False(StartsWithBom(dyBytes), "dy 导出不应带 UTF-8 BOM");
        Assert.False(StartsWithBom(wordBytes), "word 导出不应带 UTF-8 BOM");
    }

    [Fact]
    public async Task RimeImporter_KeepsWordOnlyLines()
    {
        const string content = "中国\tdummy\t1\n自定义词语\n没有编码\n";
        var importer = new RimeImporter();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var result = await importer.ImportAsync(stream);

        Assert.Equal(3, result.Entries.Count);
        Assert.Equal("中国", result.Entries[0].Word);
        Assert.Equal("自定义词语", result.Entries[1].Word);
        Assert.Empty(result.Entries[1].Code!.Segments); // 空编码交给管道自动补拼音
        Assert.Equal("没有编码", result.Entries[2].Word);
    }

    [Fact]
    public async Task RimeImporter_SkipsYamlFrontMatter()
    {
        const string content = "# Rime dictionary\n---\nname: luna_pinyin\nversion: \"1.0\"\n" +
                               "sort: by_weight\n...\n中国\tzhong guo\t100\n自定义词语\n";
        var importer = new RimeImporter();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var result = await importer.ImportAsync(stream);

        Assert.Equal(2, result.Entries.Count);
        Assert.Equal("中国", result.Entries[0].Word);
        Assert.Equal("自定义词语", result.Entries[1].Word);
    }

    private static WordEntry Entry(string word, string code, int rank) => new()
    {
        Word = word,
        Rank = rank,
        CodeType = CodeType.Pinyin,
        Code = WordCode.FromSingle(code.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    };

    private static async Task<byte[]> ExportBytesAsync(
        ImeWlConverter.Abstractions.Contracts.IFormatExporter exporter,
        IReadOnlyList<WordEntry> entries)
    {
        using var stream = new MemoryStream();
        await exporter.ExportAsync(entries, stream);
        return stream.ToArray();
    }

    private static async Task<List<string>> ExportLinesAsync(
        ImeWlConverter.Abstractions.Contracts.IFormatExporter exporter,
        IReadOnlyList<WordEntry> entries)
    {
        var bytes = await ExportBytesAsync(exporter, entries);
        var text = Encoding.UTF8.GetString(bytes).Replace("\r\n", "\n").TrimEnd('\n');
        return [.. text.Split('\n')];
    }

    private static bool StartsWithBom(byte[] bytes) =>
        bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
}
