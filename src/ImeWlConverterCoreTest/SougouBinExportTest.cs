using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.SougouBin;
using Xunit;

namespace ImeWlConverterCoreTest;

public class SougouBinExportTest
{
    [Fact]
    public async Task ExportThenImport_PreservesWordsCodesAndRanks()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "\u6df1\u84dd\u8bcd\u5e93", Code = WordCode.FromSingle(new[] { "shen", "lan", "ci", "ku" }), Rank = 17, CodeType = CodeType.Pinyin },
            new() { Word = "\u8f6f\u4ef6\u6d4b\u8bd5", Code = WordCode.FromSingle(new[] { "ruan", "jian", "ce", "shi" }), Rank = 1, CodeType = CodeType.Pinyin },
            new() { Word = "\u5973\u7eff", Code = WordCode.FromSingle(new[] { "nv", "lv" }), Rank = 0, CodeType = CodeType.Pinyin }
        };

        var (result, data) = await Export(entries);

        Assert.Equal(entries.Count, result.EntryCount);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(0x55504753u, BitConverter.ToUInt32(data, 0));
        Assert.Equal((uint)entries.Count, BitConverter.ToUInt32(data, 0x40));
        Assert.Equal((uint)0x8C, BitConverter.ToUInt32(data, 0x38));
        Assert.Equal((uint)data.Length, BitConverter.ToUInt32(data, 0x10));

        using var stream = new MemoryStream(data);
        var imported = (await new SougouBinImporter().ImportAsync(stream)).Entries;

        Assert.Equal(entries.Count, imported.Count);
        foreach (var entry in entries)
        {
            var actual = Assert.Single(imported, item => item.Word == entry.Word);
            Assert.Equal(entry.Code!.GetPrimaryCode(), actual.Code!.GetPrimaryCode());
            Assert.Equal(entry.Rank, actual.Rank);
        }
    }

    [Fact]
    public async Task Export_SkipsEntriesWithoutSupportedPinyin()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "\u6b63\u5e38", Code = WordCode.FromSingle(new[] { "zheng", "chang" }), Rank = 1 },
            new() { Word = "\u65e0\u6548", Code = WordCode.FromSingle(new[] { "not-pinyin" }), Rank = 1 },
            new() { Word = "\u7f3a\u5c11", Rank = 1 }
        };

        var (result, data) = await Export(entries);

        Assert.Equal(1, result.EntryCount);
        Assert.Equal(2, result.ErrorCount);
        Assert.Equal(1u, BitConverter.ToUInt32(data, 0x40));
    }

    [Fact]
    public async Task Export_WritesSogouMarkerFromDictionarySizeAndZeroRanks()
    {
        var entries = new List<WordEntry>
        {
            new() { Word = "\u4ec4\u963f\u7532", Code = WordCode.FromSingle(new[] { "ze", "a", "jia" }), Rank = 1 },
            new() { Word = "\u4ec4\u963f\u5047", Code = WordCode.FromSingle(new[] { "ze", "a", "jia" }), Rank = 0 }
        };

        var (_, data) = await Export(entries);

        var dictionarySize = BitConverter.ToUInt32(data, 0x4c);
        var wordCount = BitConverter.ToUInt32(data, 0x40);
        var zeroRankCount = BitConverter.ToUInt32(data, 0x54);
        var expected = checked(0x5691F359u + dictionarySize + wordCount - 1 + zeroRankCount);

        Assert.Equal(expected, BitConverter.ToUInt32(data, 0x20));
    }

    private static async Task<(ImeWlConverter.Abstractions.Results.ExportResult Result, byte[] Data)> Export(
        IReadOnlyList<WordEntry> entries)
    {
        var exporter = new SougouBinExporter();
        using var stream = new MemoryStream();
        var result = await exporter.ExportAsync(entries, stream);
        return (result, stream.ToArray());
    }
}
