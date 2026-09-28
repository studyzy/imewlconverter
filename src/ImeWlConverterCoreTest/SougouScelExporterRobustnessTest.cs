using System;
using System.Collections.Generic;
using System.IO;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.SougouScel;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// scel 导出器健壮性测试（参见 issue #419：坏词条不应中断整个导出）。
/// </summary>
public class SougouScelExporterRobustnessTest
{
    public static IEnumerable<object[]> AdversarialCodes()
    {
        yield return new object[] { new[] { "" } };                       // 空编码串
        yield return new object[] { new[] { "123" } };                    // 纯数字(去声调后为空)
        yield return new object[] { new[] { "ce", "", "shi" } };          // 混入空编码
        yield return new object[] { new[] { "CE", "SHI" } };              // 大写
        yield return new object[] { new[] { "abcdefghijklmnopqrstuvwxyz" } }; // 超长编码
    }

    [Theory]
    [MemberData(nameof(AdversarialCodes))]
    public void ExportWithAdversarialCodes_DoesNotThrow(string[] codes)
    {
        var exporter = new SougouScelExporter();
        var entries = new List<WordEntry>
        {
            new()
            {
                Word = "测试词",
                Rank = 1,
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(codes)
            }
        };
        using var stream = new MemoryStream();
        var ex = Record.ExceptionAsync(() => exporter.ExportAsync(entries, stream))
            .GetAwaiter().GetResult();
        Assert.Null(ex);
    }

    [Fact]
    public void ExportWithOversizedGroup_SplitsIntoChunks()
    {
        // 同一拼音组合挂 4 万词: 16 位词数上限 32767, 应拆分为多组而非溢出/崩溃
        var exporter = new SougouScelExporter();
        var entries = new List<WordEntry>();
        for (var i = 0; i < 40000; i++)
        {
            entries.Add(new WordEntry
            {
                Word = "词" + i,
                Rank = 1,
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(new[] { "ce", "shi" })
            });
        }

        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();

        Assert.Equal(40000, result.EntryCount);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void ExportWithNullAndEmptySegmentCodes_SkipsGracefully()
    {
        var exporter = new SougouScelExporter();
        var entries = new List<WordEntry>
        {
            new() { Word = "无编码", Rank = 1, CodeType = CodeType.Pinyin, Code = null },
            new()
            {
                Word = "测试", Rank = 1, CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(new[] { "ce", "shi" })
            }
        };
        using var stream = new MemoryStream();
        var result = exporter.ExportAsync(entries, stream).GetAwaiter().GetResult();
        Assert.Equal(1, result.EntryCount);
    }
}
