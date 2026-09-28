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

        // candidate position (1 byte)：同拼音第几条候选，1 起
        var position = stream.ReadByte();
        Assert.Equal(1, position);

        // unknown byte (0x06)
        stream.ReadByte();

        // unknown 4 bytes + 2000 纪元时间戳 4 bytes
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
    /// Issue #401: 头部 0x20 为 Unix 时间戳（uint32），0x24-0x3F 必须为 0，
    /// 0x18 为文件大小，0x14 为数据区起始 0x40+4N。微软拼音会校验这些字段。
    /// </summary>
    [Fact]
    public void Issue401_Header_TimestampAndZeroPadding()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "测试", Code = WordCode.FromSingle(new[] { "ce", "shi" }), Rank = 1, CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();
        var bytes = stream.ToArray();

        // 0x14: 数据区起始 = 0x40 + 4 * count
        var dataStart = BitConverter.ToUInt32(bytes, 0x14);
        Assert.Equal(0x40L + 4, (long)dataStart);

        // 0x18: 文件大小
        var fileSize = BitConverter.ToUInt32(bytes, 0x18);
        Assert.Equal((uint)bytes.Length, fileSize);

        // 0x20: Unix 时间戳（合理范围：2020-2050 年）
        var unixTs = BitConverter.ToUInt32(bytes, 0x20);
        Assert.InRange((long)unixTs, 1577836800L, 2524608000L);

        // 0x24-0x3F 必须全为 0
        for (var i = 0x24; i < 0x40; i++)
        {
            Assert.Equal(0, bytes[i]);
        }

        // 记录 +0x08：4 字节 0；+0x0C：2000 纪元秒（当前 Unix 秒 - 946684800）
        stream.Position = 0x40 + 4; // 跳过偏移表第 1 项，定位到第 1 条记录
        using var reader = new BinaryReader(stream, Encoding.Unicode, leaveOpen: true);
        stream.Position += 8; // magic(4) + hanziOffset(2) + position(1) + 0x06(1)
        var zeroField = reader.ReadUInt32();
        Assert.Equal(0u, zeroField);
        var epoch2000Seconds = reader.ReadUInt32();
        Assert.InRange(epoch2000Seconds, 1577836800u - 946684800u, 2524608000u - 946684800u);
    }

    /// <summary>
    /// Issue #401: 同拼音多条候选，记录 +0x06 应为候选序号 1,2,3...（而非词频）。
    /// </summary>
    [Fact]
    public void Issue401_DuplicatePinyin_AssignsCandidatePositions()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "啊", Code = WordCode.FromSingle(new[] { "a" }), Rank = 100, CodeType = CodeType.Pinyin },
            new() { Word = "阿", Code = WordCode.FromSingle(new[] { "a" }), Rank = 50, CodeType = CodeType.Pinyin },
            new() { Word = "斤", Code = WordCode.FromSingle(new[] { "jin" }), Rank = 10, CodeType = CodeType.Pinyin },
            new() { Word = "今", Code = WordCode.FromSingle(new[] { "jin" }), Rank = 20, CodeType = CodeType.Pinyin },
            new() { Word = "金", Code = WordCode.FromSingle(new[] { "jin" }), Rank = 30, CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        using var reader = new BinaryReader(stream, Encoding.Unicode, leaveOpen: true);
        var positions = new List<byte>();
        for (var i = 0; i < entries.Count; i++)
        {
            stream.Position = 0x40 + 4 * i;             // 偏移表第 i 项
            var recordOffset = reader.ReadInt32();
            stream.Position = 0x40 + 4 * entries.Count + recordOffset;
            stream.Position += 6;                        // magic(4) + hanziOffset(2)
            positions.Add(reader.ReadByte());
        }

        Assert.Equal(new byte[] { 1, 2, 1, 2, 3 }, positions);
    }

    /// <summary>
    /// Issue #401: 微软自学习词库 2 万条上限，超出部分截断并计入未导出数。
    /// </summary>
    [Fact]
    public void Issue401_EntriesOverLimit_TruncatedTo20000()
    {
        var entries = new List<WordEntry>();
        for (var i = 0; i < MsChxUdpExporterBase.MaxEntries + 5; i++)
        {
            entries.Add(new WordEntry
            {
                Word = $"词{i:D6}",
                Code = WordCode.FromSingle(new[] { CodeForIndex(i) }),
                CodeType = CodeType.Pinyin
            });
        }

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(MsChxUdpExporterBase.MaxEntries, result.EntryCount);
        Assert.Equal(5, result.ErrorCount);

        // 头部词条数也应为 2 万
        var phraseCount = BitConverter.ToUInt32(stream.ToArray(), 0x1C);
        Assert.Equal((uint)MsChxUdpExporterBase.MaxEntries, phraseCount);
    }

    /// <summary>生成唯一的合法拼音编码（26 进制小写字母）。</summary>
    private static string CodeForIndex(int i)
    {
        var sb = new StringBuilder();
        do
        {
            sb.Append((char)('a' + (i % 26)));
            i /= 26;
        }
        while (i > 0);
        return sb.ToString();
    }

    /// <summary>
    /// Issue #401: 空编码、非法字符编码（数字/大写）、重复词条应被跳过并计入 ErrorCount，
    /// 避免生成微软拼音无法导入的畸形记录。
    /// </summary>
    [Fact]
    public void Issue401_InvalidAndDuplicateEntries_Skipped()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "无编码", Code = null, CodeType = CodeType.Pinyin },                       // 空编码
            new() { Word = "带数字", Code = WordCode.FromSingle(new[] { "ce", "shi", "1" }), CodeType = CodeType.Pinyin }, // 非法字符
            new() { Word = "测试", Code = WordCode.FromSingle(new[] { "ce", "shi" }), CodeType = CodeType.Pinyin },
            new() { Word = "测试", Code = WordCode.FromSingle(new[] { "ce", "shi" }), CodeType = CodeType.Pinyin }, // 重复
            new() { Word = "正常", Code = WordCode.FromSingle(new[] { "zheng", "chang" }), CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinExporter();
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(2, result.EntryCount);
        Assert.Equal(3, result.ErrorCount);

        stream.Position = 0;
        var importer = new Win10MsPinyinImporter();
        var importResult = importer.ImportAsync(stream).GetAwaiter().GetResult();
        Assert.Equal(2, importResult.Entries.Count);
    }

    /// <summary>
    /// Issue #401: 导入器应拒绝非 mschxudp 文件与损坏的头部结构。
    /// </summary>
    [Fact]
    public void Issue401_Importer_RejectsInvalidFiles()
    {
        var importer = new Win10MsPinyinImporter();

        // 非魔数文件
        using var garbage = new MemoryStream(Encoding.ASCII.GetBytes("not-a-dat-file--padding-padding"));
        Assert.Throws<InvalidDataException>(() =>
            importer.ImportAsync(garbage).GetAwaiter().GetResult());

        // 魔数正确但头部词条数与文件大小矛盾（损坏）
        var corrupt = new byte[0x40];
        Encoding.ASCII.GetBytes("mschxudp").CopyTo(corrupt, 0);
        BitConverter.GetBytes((uint)0x40).CopyTo(corrupt, 0x10);   // 偏移表起始
        BitConverter.GetBytes((uint)0x40).CopyTo(corrupt, 0x14);   // 记录区起始
        BitConverter.GetBytes((uint)0x40).CopyTo(corrupt, 0x18);   // 文件大小
        BitConverter.GetBytes((uint)100).CopyTo(corrupt, 0x1C);    // 词条数=100，超出文件范围
        using var corruptStream = new MemoryStream(corrupt);
        Assert.Throws<InvalidDataException>(() =>
            importer.ImportAsync(corruptStream).GetAwaiter().GetResult());
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
