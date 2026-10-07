using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Formats.QingJian;
using System.Threading.Tasks;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

public class QingJianQjTest
{
    /// <summary>
    /// 导出 .qj 后逐字节验证容器结构：头、分节表、对齐、记录数与排序规则。
    /// </summary>
    [Fact]
    public async Task TestExportContainerLayout()
    {
        var entries = new[]
        {
            MakeEntry("深蓝", "shen lan", 5),
            MakeEntry("蓝蓝", "shen lan", 10),
            MakeEntry("你好", "ni hao", 1),
        };

        var data = await Export(entries);

        Assert.Equal("QINGJIAN"u8.ToArray(), data[..8]);
        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(8, 2)));   // version
        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(10, 2)));  // kind = Dictionary
        Assert.Equal(5u, BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(12, 4))); // 5 个分节

        var sections = ParseSections(data);
        Assert.Equal(5, sections.Count);

        // 分节顺序固定：META 第一，正文 8 字节对齐
        Assert.Equal("META", sections[0].Tag);
        foreach (var section in sections)
        {
            Assert.Equal(0, section.Offset % 8);
            Assert.True(section.Offset + section.Length <= data.Length);
        }

        // META 是 TOML 文本
        var meta = Encoding.UTF8.GetString(data[sections[0].Offset..(sections[0].Offset + sections[0].Length)]);
        Assert.Contains("name = \"深蓝词库转换\"", meta);
        Assert.Contains("entries = 3", meta);

        // INDX = 16B × 键数（2 个键：ni hao、shen lan），按键字节序升序
        var indexSection = sections.Single(s => s.Tag == "INDX");
        Assert.Equal(0, indexSection.Length % 16);
        Assert.Equal(2, indexSection.Length / 16);

        // SLOT = 12B × 词条数
        var slotsSection = sections.Single(s => s.Tag == "SLOT");
        Assert.Equal(36, slotsSection.Length);

        // INDX 首键是 "ni hao"（字节序 n < s）
        var indxOffset = indexSection.Offset;
        var keyStart = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(indxOffset, 4));
        var keyLen = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(indxOffset + 12, 2));
        var keysSection = sections.Single(s => s.Tag == "KEYS");
        var firstKey = Encoding.UTF8.GetString(data, keysSection.Offset + (int)keyStart, keyLen);
        Assert.Equal("ni hao", firstKey);
    }

    /// <summary>
    /// 同一键下的词目按词频降序排列。
    /// </summary>
    [Fact]
    public async Task TestExportSortsSlotsByFrequencyDescending()
    {
        var entries = new[]
        {
            MakeEntry("深蓝", "shen lan", 5),
            MakeEntry("蓝蓝", "shen lan", 10),
        };

        var imported = await Roundtrip(entries);

        Assert.Equal(2, imported.Count);
        // 词频高的在前
        Assert.Equal("蓝蓝", imported[0].Word);
        Assert.Equal(10, imported[0].Rank);
        Assert.Equal("深蓝", imported[1].Word);
        Assert.Equal(5, imported[1].Rank);
    }

    /// <summary>
    /// 导出时 lue/nue 统一为青简规范形式 lve/nve。
    /// </summary>
    [Fact]
    public async Task TestExportNormalizesLueNue()
    {
        var entries = new[] { MakeEntry("策略", "ce lue", 10) };
        var imported = await Roundtrip(entries);

        var entry = Assert.Single(imported);
        Assert.Equal("策略", entry.Word);
        Assert.Equal("lve", entry.Code!.Segments[1][0]);
    }

    /// <summary>
    /// 导出后导入应完整保留词、拼音、词频（同键多词、多音节）。
    /// </summary>
    [Fact]
    public async Task TestRoundtrip()
    {
        var entries = new[]
        {
            MakeEntry("深蓝词库", "shen lan ci ku", 5),
            MakeEntry("你好", "ni hao", 100),
            MakeEntry("你好吗", "ni hao ma", 50),
            MakeEntry("策略", "ce lue", 10),
        };

        var imported = await Roundtrip(entries);

        Assert.Equal(4, imported.Count);
        var byWord = imported.ToDictionary(e => e.Word);
        Assert.Equal(5, byWord["深蓝词库"].Rank);
        Assert.Equal("ku", byWord["深蓝词库"].Code!.Segments[3][0]);
        Assert.Equal(100, byWord["你好"].Rank);
        Assert.Equal(50, byWord["你好吗"].Rank);
        Assert.Equal("lve", byWord["策略"].Code!.Segments[1][0]);
    }

    /// <summary>
    /// 缺拼音的词条被跳过并计入 ErrorCount；全部无效则抛出异常。
    /// </summary>
    [Fact]
    public async Task TestExportSkipsEntriesWithoutPinyin()
    {
        var entries = new[]
        {
            MakeEntry("深蓝", "shen lan", 5),
            new WordEntry { Word = "无码", Rank = 1, CodeType = CodeType.Pinyin },
        };

        using var stream = new MemoryStream();
        var result = await new QingJianQjExporter().ExportAsync(entries, stream);

        Assert.Equal(1, result.EntryCount);
        Assert.Equal(1, result.ErrorCount);
    }

    /// <summary>
    /// META.name 默认按源词库文件名主干命名。
    /// </summary>
    [Fact]
    public async Task TestExportUsesSourceFileName()
    {
        var entries = new[] { MakeEntry("深蓝", "shen lan", 5) };
        using var stream = new MemoryStream();
        await new QingJianQjExporter().ExportAsync(
            entries, stream,
            new ExportOptions { SourceFileName = "搜狗词库备份_2026_02_02.bin" });

        var meta = ReadMeta(stream.ToArray());
        Assert.Contains("name = \"搜狗词库备份_2026_02_02\"", meta);
    }

    /// <summary>
    /// 显式指定的词库名称（--dict-name）优先于源文件名。
    /// </summary>
    [Fact]
    public async Task TestExportExplicitDictionaryNameWins()
    {
        var entries = new[] { MakeEntry("深蓝", "shen lan", 5) };
        using var stream = new MemoryStream();
        await new QingJianQjExporter().ExportAsync(
            entries, stream,
            new ExportOptions { DictionaryName = "自定义名", SourceFileName = "src.tsv" });

        var meta = ReadMeta(stream.ToArray());
        Assert.Contains("name = \"自定义名\"", meta);
    }

    /// <summary>
    /// 无源文件名时回退到默认名称。
    /// </summary>
    [Fact]
    public async Task TestExportNameFallsBackToDefault()
    {
        var entries = new[] { MakeEntry("深蓝", "shen lan", 5) };
        using var stream = new MemoryStream();
        await new QingJianQjExporter().ExportAsync(entries, stream);

        var meta = ReadMeta(stream.ToArray());
        Assert.Contains("name = \"深蓝词库转换\"", meta);
    }

    /// <summary>
    /// 非法文件（错误魔数）应抛出 InvalidDataException。
    /// </summary>
    [Fact]
    public async Task TestImportRejectsBadMagic()
    {
        var data = new byte[64];
        "NOTQJ..."u8.CopyTo(data);
        using var stream = new MemoryStream(data);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => new QingJianQjImporter().ImportAsync(stream));
    }

    /// <summary>
    /// 交叉验证：与青简官方导出的真实 .qj（用户词库样本，若存在）结构对齐。
    /// 该测试在本机有青简词库目录时运行，否则跳过。
    /// </summary>
    [Fact]
    public async Task TestImportRealQingjianFile()
    {
        var dictsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library", "Application Support", "Qingjian", "dicts");
        if (!Directory.Exists(dictsDir))
            return; // 未安装青简，跳过

        var sample = Directory.GetFiles(dictsDir, "*.qj").FirstOrDefault();
        if (sample is null)
            return;

        using var stream = File.OpenRead(sample);
        var result = await new QingJianQjImporter().ImportAsync(stream);

        Assert.NotEmpty(result.Entries);
        Assert.All(result.Entries, e =>
        {
            Assert.False(string.IsNullOrEmpty(e.Word));
            Assert.NotNull(e.Code);
            Assert.True(e.Code!.Segments.Count > 0);
        });
    }

    private static string ReadMeta(byte[] data)
    {
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(12, 4));
        var metaOffset = (int)BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(32 + 8, 8));
        var metaLength = (int)BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(32 + 16, 8));
        Assert.Equal("META", Encoding.ASCII.GetString(data, 32, 4));
        Assert.True(count >= 1);
        return Encoding.UTF8.GetString(data, metaOffset, metaLength);
    }

    private static WordEntry MakeEntry(string word, string pinyin, int rank) => new()
    {
        Word = word,
        Code = WordCode.FromSingle(pinyin.Split(' ')),
        Rank = rank,
        CodeType = CodeType.Pinyin
    };

    private static async Task<byte[]> Export(WordEntry[] entries)
    {
        using var stream = new MemoryStream();
        var result = await new QingJianQjExporter().ExportAsync(entries, stream);
        Assert.Equal(entries.Length, result.EntryCount);
        return stream.ToArray();
    }

    private static async Task<List<WordEntry>> Roundtrip(WordEntry[] entries)
    {
        using var stream = new MemoryStream(await Export(entries));
        return (await new QingJianQjImporter().ImportAsync(stream)).Entries.ToList();
    }

    private static List<(string Tag, int Offset, int Length)> ParseSections(byte[] data)
    {
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(12, 4));
        var sections = new List<(string, int, int)>((int)count);
        for (var i = 0; i < count; i++)
        {
            var offset = 32 + i * 24;
            var tag = Encoding.ASCII.GetString(data, offset, 4);
            var bodyOffset = (int)BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(offset + 8, 8));
            var length = (int)BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(offset + 16, 8));
            sections.Add((tag, bodyOffset, length));
        }
        return sections;
    }
}
