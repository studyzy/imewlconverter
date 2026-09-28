using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Win10MsSelfStudy;
using Xunit;

namespace ImeWlConverterCoreTest;

public class Win10MsPinyinSelfStudyTest
{
    [Fact]
    public void TestExport1()
    {
        var entries = new List<WordEntry>
        {
            new()
            {
                Word = "曾毅曾诚",
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(new[] { "zeng", "yi", "zeng", "cheng" })
            }
        };

        var exporter = new Win10MsPinyinSelfStudyExporter();
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(1, result.EntryCount);
        Assert.True(stream.Length > 0);

        // Verify header magic
        stream.Position = 0;
        var header = new byte[4];
        stream.Read(header, 0, 4);
        Assert.Equal(0x55, header[0]);
        Assert.Equal(0xAA, header[1]);
        Assert.Equal(0x88, header[2]);
        Assert.Equal(0x81, header[3]);

        // Verify word count
        stream.Position = 12;
        var countBytes = new byte[8];
        stream.Read(countBytes, 0, 8);
        var count = BitConverter.ToInt64(countBytes, 0);
        Assert.Equal(1, count);

        // Verify word at entry 0, offset 0x2400 + 12
        stream.Position = 0x2400 + 12;
        var wordBytes = new byte[8]; // 4 chars * 2 bytes
        stream.Read(wordBytes, 0, 8);
        var word = Encoding.Unicode.GetString(wordBytes);
        Assert.Equal("曾毅曾诚", word);

        // Verify file is padded to 1KB boundary
        Assert.Equal(0, stream.Length % 1024);
    }

    [Fact]
    public void TestExportRoundTrip()
    {
        var entries = new List<WordEntry>
        {
            new()
            {
                Word = "深蓝词库",
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(new[] { "shen", "lan", "ci", "ku" })
            }
        };

        var exporter = new Win10MsPinyinSelfStudyExporter();
        using var stream = new MemoryStream();
        exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        // Import back
        stream.Position = 0;
        var importer = new Win10MsPinyinSelfStudyImporter();
        var importResult = importer.ImportAsync(stream).GetAwaiter().GetResult();

        Assert.Single(importResult.Entries);
        Assert.Equal("深蓝词库", importResult.Entries[0].Word);
    }

    [Fact]
    public void SelfStudyExport_SkipsInvalidCodeEntries()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "无编码词", Code = null, CodeType = CodeType.Pinyin },                                    // 空编码
            new() { Word = "带数字词", Code = WordCode.FromSingle(new[] { "dai", "shu", "zi", "1" }), CodeType = CodeType.Pinyin }, // 非法音节
            new() { Word = "音节数不符", Code = WordCode.FromSingle(new[] { "yin", "jie" }), CodeType = CodeType.Pinyin },          // 音节数 != 字数
            new() { Word = "正常词汇", Code = WordCode.FromSingle(new[] { "zheng", "chang", "ci", "hui" }), CodeType = CodeType.Pinyin },
        };

        var exporter = new Win10MsPinyinSelfStudyExporter();
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(1, result.EntryCount);
        Assert.Equal(3, result.ErrorCount);

        // 只有合法词条写入了文件
        stream.Position = 12;
        var countBytes = new byte[8];
        stream.Read(countBytes, 0, 8);
        Assert.Equal(1, BitConverter.ToInt64(countBytes, 0));
    }

    [Fact]
    public void SelfStudyImport_RejectsInvalidMagic()
    {
        // 大小满足 >= 0x2400，但头部魔数错误，且带有看似合法的词条数
        var garbage = new byte[0x2400 + 120];
        BitConverter.GetBytes(5).CopyTo(garbage, 12);
        var importer = new Win10MsPinyinSelfStudyImporter();
        using var stream = new MemoryStream(garbage);

        var ex = Assert.Throws<InvalidDataException>(() =>
            importer.ImportAsync(stream).GetAwaiter().GetResult());
        Assert.Contains("文件头", ex.Message);
    }

    [Fact]
    public void SelfStudyImport_SkipsOverlongWordRecords()
    {
        // 先导出一个合法词条，取出其 60 字节记录作为"好记录"
        var exporter = new Win10MsPinyinSelfStudyExporter();
        var goodEntries = new List<WordEntry>
        {
            new() { Word = "测试", Code = WordCode.FromSingle(new[] { "ce", "shi" }), CodeType = CodeType.Pinyin },
        };
        using var goodStream = new MemoryStream();
        exporter.ExportAsync(goodEntries, goodStream).GetAwaiter().GetResult();
        var goodRecord = new byte[60];
        goodStream.Position = 0x2400;
        goodStream.Read(goodRecord, 0, 60);

        // 手工构造：坏记录（wordLen=13，超过记录容量）+ 好记录
        var badRecord = new byte[60];
        badRecord[10] = 13;
        for (var j = 0; j < 13; j++)
        {
            Encoding.Unicode.GetBytes("测").CopyTo(badRecord, 12 + j * 2);
        }

        var bytes = goodStream.ToArray().AsSpan(0, 0x2400).ToArray(); // 头部 + 填充
        BitConverter.GetBytes(2).CopyTo(bytes, 12); // 词条数 = 2
        var data = new List<byte>(bytes);
        data.AddRange(badRecord);
        data.AddRange(goodRecord);

        var importer = new Win10MsPinyinSelfStudyImporter();
        using var stream = new MemoryStream(data.ToArray());
        var importResult = importer.ImportAsync(stream).GetAwaiter().GetResult();

        // 坏记录被跳过，只解析出好记录
        Assert.Single(importResult.Entries);
        Assert.Equal("测试", importResult.Entries[0].Word);
    }
}
