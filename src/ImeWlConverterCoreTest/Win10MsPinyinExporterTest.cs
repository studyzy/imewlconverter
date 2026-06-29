using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Win10Ms;
using Xunit;

namespace ImeWlConverterCoreTest;

/// <summary>
/// Issue #403: 最新版自定义词库转win10自定义词不可用。
/// 验证 Win10MsPinyinExporter 生成的二进制文件格式正确，可被 Win10MsPinyinImporter 正确解析。
/// </summary>
public class Win10MsPinyinExporterTest
{
    static Win10MsPinyinExporterTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Issue #403: 导出再导入，词条应完整保留。
    /// 模拟真实场景：纯汉字词语 -> 拼音编码 -> Win10MsPinyin 导出 -> 导入验证
    /// </summary>
    [Fact]
    public void Issue403_ExportThenImport_PreservesWords()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "深蓝词库", Code = WordCode.FromSingle(new[] { "shen", "lan", "ci", "ku" }), Rank = 5, CodeType = CodeType.Pinyin },
            new() { Word = "词库转换", Code = WordCode.FromSingle(new[] { "ci", "ku", "zhuan", "huan" }), Rank = 3, CodeType = CodeType.Pinyin },
            new() { Word = "测试", Code = WordCode.FromSingle(new[] { "ce", "shi" }), Rank = 1, CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        var importer = new Win10MsPinyinImporter();

        // Export
        using var exportStream = new MemoryStream();
        var exportResult = exporter.ExportAsync(entries, exportStream).GetAwaiter().GetResult();
        Assert.True(exportResult.EntryCount > 0, "Export should produce entries");
        Assert.True(exportStream.Length > 0, "Export should produce non-empty stream");
        Assert.Equal(0, exportResult.ErrorCount);

        // Import back
        exportStream.Position = 0;
        var importResult = importer.ImportAsync(exportStream).GetAwaiter().GetResult();
        Assert.Equal(entries.Count, importResult.Entries.Count);

        // Verify all words preserved
        var importedWords = importResult.Entries.Select(e => e.Word).ToHashSet();
        foreach (var entry in entries)
        {
            Assert.Contains(entry.Word, importedWords);
        }

        // Verify code is preserved
        foreach (var entry in importResult.Entries)
        {
            Assert.NotNull(entry.Code);
            Assert.NotEmpty(entry.Code.Segments);
        }
    }

    /// <summary>
    /// Issue #403: 验证二进制文件头结构正确。
    /// Win10 MsPinyin 文件头应为 "mschxudp" 开头的 40 字节。
    /// </summary>
    [Fact]
    public void Issue403_FileHeader_IsValid()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "测试", Code = WordCode.FromSingle(new[] { "ce", "shi" }), Rank = 1, CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();
        stream.Position = 0;

        var headerBytes = new byte[8];
        stream.Read(headerBytes, 0, 8);
        var header = Encoding.ASCII.GetString(headerBytes);

        Assert.Equal("mschxudp", header);
    }

    /// <summary>
    /// Issue #403: 验证条目中的拼音和词语编码正确。
    /// 读取二进制，手动解析条目，检查拼音和词语的 Unicode 编码。
    /// 文件结构: Header(64bytes) + Offsets(4*count bytes) + Phrases
    /// </summary>
    [Fact]
    public void Issue403_EntryContent_EncodingCorrect()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "深蓝词库", Code = WordCode.FromSingle(new[] { "shen", "lan", "ci", "ku" }), Rank = 5, CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();
        stream.Position = 0;

        using var reader = new BinaryReader(stream, Encoding.Unicode, leaveOpen: true);

        // Skip header (64 bytes = 0x40)
        stream.Position = 0x40;

        // Read offsets: phrase_offset_start points here, offsets are relative to phrase_start
        // phrase_start = 0x40 + 4 * count = 0x44 for 1 entry
        var firstOffset = reader.ReadInt32();
        Assert.Equal(0, firstOffset); // First entry offset relative to phrase_start is 0

        // Now seek to phrase_start (0x44)
        stream.Position = 0x44;

        // Read entry: magic (4 bytes)
        var magic = reader.ReadInt32();
        Assert.Equal(0x00100010, magic);

        // hanzi offset (2 bytes)
        var hanziOffset = reader.ReadInt16();
        Assert.True(hanziOffset > 0);

        // rank (1 byte)
        var rank = stream.ReadByte();
        Assert.Equal(5, rank);

        // unknown byte (0x06)
        stream.ReadByte();

        // unknown 8 bytes
        reader.ReadInt64();

        // Pinyin bytes: hanziOffset - 18
        var pyBytesLen = hanziOffset - 18;
        var pyBytes = reader.ReadBytes(pyBytesLen);
        var pyStr = Encoding.Unicode.GetString(pyBytes);
        Assert.Contains("shen", pyStr);
        Assert.Contains("lan", pyStr);
        Assert.Contains("ci", pyStr);
        Assert.Contains("ku", pyStr);

        // Read separator (00 00)
        reader.ReadInt16();

        // Read word bytes
        var remainingBytes = stream.Length - stream.Position - 2; // -2 for trailing separator
        var wordBytes = reader.ReadBytes((int)remainingBytes);
        var word = Encoding.Unicode.GetString(wordBytes);
        Assert.Equal("深蓝词库", word);
    }

    /// <summary>
    /// Issue #403: 边界条件 - 空词条列表导出不会崩溃。
    /// </summary>
    [Fact]
    public void Issue403_EmptyEntryList_DoesNotCrash()
    {
        var entries = new List<WordEntry>();
        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(0, result.EntryCount);
        Assert.True(stream.Length > 0, "Even empty list should produce valid header");
    }

    /// <summary>
    /// Issue #403: 验证超过长度限制的词条被正确过滤（不导致文件损坏）。
    /// Word > 64 或 Pinyin > 32 的词条会被跳过。
    /// </summary>
    [Fact]
    public void Issue403_LongEntries_FilteredCorrectly()
    {
        var longWord = new string('测', 65); // 65 chars, over 64 limit
        var entries = new List<WordEntry>
        {
            new() { Word = longWord, Code = WordCode.FromSingle(new[] { "ce" }), Rank = 1, CodeType = CodeType.Pinyin },
            new() { Word = "正常", Code = WordCode.FromSingle(new[] { "zheng", "chang" }), Rank = 1, CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(1, result.EntryCount); // 只有"正常"被导出
        Assert.Equal(1, result.ErrorCount); // 长词条被跳过

        // Import back and verify
        stream.Position = 0;
        var importer = new Win10MsPinyinImporter();
        var importResult = importer.ImportAsync(stream).GetAwaiter().GetResult();

        Assert.Single(importResult.Entries);
        Assert.Equal("正常", importResult.Entries[0].Word);
    }
}
