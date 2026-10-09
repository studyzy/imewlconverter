/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Buffers.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.LibIMEPinyin;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// Fcitx5 / libime 二进制拼音词库格式测试。
/// </summary>
public class LibIMEPinyinTest
{
    private readonly LibIMEPinyinExporter _exporter = new();
    private readonly LibIMEPinyinImporter _importer = new();

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

    /// <summary>格式元数据: 二进制、.dict 扩展名。</summary>
    [Fact]
    public void Metadata_IsBinaryDict()
    {
        Assert.Equal("libimebin", _exporter.Metadata.Id);
        Assert.True(_exporter.Metadata.IsBinary);
        Assert.Equal(".dict", _exporter.Metadata.FileExtension);
    }

    /// <summary>文件头必须是 libime 的 magic(大端) + version 2, 其后是 zstd 帧。</summary>
    [Fact]
    public void Export_WritesLibimeHeader()
    {
        using var ms = new MemoryStream();
        _exporter.ExportAsync(new[] { Entry("你好", 1, "ni", "hao") }, ms).GetAwaiter().GetResult();
        var bytes = ms.ToArray();

        Assert.Equal(
            new byte[] { 0x00, 0x0f, 0xc6, 0x13, 0x00, 0x00, 0x00, 0x02 },
            bytes.Take(8).ToArray());
        // zstd 帧魔数 0x28 B5 2F FD
        Assert.Equal(new byte[] { 0x28, 0xb5, 0x2f, 0xfd }, bytes.Skip(8).Take(4).ToArray());
    }

    /// <summary>导出后再导入, 词面 / 拼音 / 词频应完整往返。</summary>
    [Fact]
    public void ExportThenImport_RoundTrips()
    {
        var entries = new[]
        {
            Entry("你好", 5, "ni", "hao"),
            Entry("小企鹅", 3, "xiao", "qi", "e"),
            Entry("绿色", 1, "lv", "se"),
        };

        using var ms = new MemoryStream();
        _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();

        ms.Position = 0;
        var result = _importer.ImportAsync(ms).GetAwaiter().GetResult();
        var imported = result.Entries.ToDictionary(e => e.Word);

        Assert.Equal(3, imported.Count);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(5, imported["你好"].Rank);
        Assert.Equal("ni'hao", imported["你好"].Code!.GetPrimaryCode("'"));
        Assert.Equal(3, imported["小企鹅"].Rank);
        Assert.Equal("xiao'qi'e", imported["小企鹅"].Code!.GetPrimaryCode("'"));
        Assert.Equal("lv'se", imported["绿色"].Code!.GetPrimaryCode("'"));
    }

    /// <summary>缺少拼音的词条应被计入错误而不写出损坏数据。</summary>
    [Fact]
    public void Export_AllEntriesWithoutPinyin_Throws()
    {
        var entries = new[] { new WordEntry { Word = "你好", CodeType = CodeType.Pinyin } };

        using var ms = new MemoryStream();
        Assert.Throws<System.InvalidOperationException>(
            () => _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult());
    }

    /// <summary>v1(未压缩)旧格式也应能导入。</summary>
    [Fact]
    public void Import_LegacyUncompressedVersion_Works()
    {
        var key = new List<byte>();
        Assert.True(LibimePinyinTable.TryEncode("ni", out var initial, out var final));
        key.Add(initial);
        key.Add(final);
        key.Add((byte)'!');
        key.AddRange(Encoding.UTF8.GetBytes("你好"));

        var trie = new CedarFloatTrie();
        trie.Set(key.ToArray(), 7f);

        using var ms = new MemoryStream();
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(header, 0x000fc613);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], 0x1);
        ms.Write(header);
        trie.Save(ms);

        ms.Position = 0;
        var result = _importer.ImportAsync(ms).GetAwaiter().GetResult();

        var entry = Assert.Single(result.Entries);
        Assert.Equal("你好", entry.Word);
        Assert.Equal(7, entry.Rank);
        Assert.Equal("ni", entry.Code!.GetPrimaryCode("'"));
    }

    /// <summary>
    /// 音节编码必须与 libime <c>PinyinEncoder</c> 一致
    /// (枚举数值取自 libime pinyinencoder.h / testpinyinencoder.cpp)。
    /// </summary>
    [Theory]
    [InlineData("ni", 71, 78)]      // N=71, I=78
    [InlineData("hao", 75, 69)]     // H=75, AO=69
    [InlineData("xiao", 78, 81)]    // X=78, IAO=81
    [InlineData("zhong", 79, 76)]   // ZH=79, ONG=76
    [InlineData("e", 88, 70)]       // Zero=88, E=70
    [InlineData("lv", 72, 96)]      // L=72, V=96
    [InlineData("lve", 72, 97)]     // L=72, VE=97
    [InlineData("lue", 72, 97)]     // VE_UE 模糊别名 → lve
    [InlineData("nue", 71, 97)]     // N=71, VE=97
    public void EncodeSyllable_MatchesLibime(string syllable, int initial, int final)
    {
        Assert.True(LibimePinyinTable.TryEncode(syllable, out var i, out var f));
        Assert.Equal((byte)initial, i);
        Assert.Equal((byte)final, f);
        Assert.Equal(syllable == "lue" ? "lve" : syllable == "nue" ? "nve" : syllable,
            LibimePinyinTable.DecodeSyllable(i, f));
    }

    /// <summary>多音节拼音字节串解码为 ' 分隔的拼音。</summary>
    [Fact]
    public void DecodeFullPinyin_JoinsWithApostrophe()
    {
        var data = new byte[] { 71, 78, 75, 69 }; // ni hao
        Assert.Equal("ni'hao", LibimePinyinTable.DecodeFullPinyin(data));
    }

    /// <summary>不是 libime 词库时应明确失败。</summary>
    [Fact]
    public void Import_InvalidMagic_Throws()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 0, 0, 0, 2, 0, 0, 0, 0 };
        using var ms = new MemoryStream(bytes);
        Assert.Throws<System.IO.InvalidDataException>(
            () => _importer.ImportAsync(ms).GetAwaiter().GetResult());
    }

    /// <summary>
    /// libime 压缩链路（Boost.Iostreams <c>symmetric_filter</c> + <c>ZSTDCompressor</c>）
    /// 在关闭时需要多轮 flush 时，会在帧尾多写出一个空 zstd 帧。
    /// 上游 <c>libime_pinyindict</c> / Fcitx5 的 *.dict 都带这个尾巴，必须逐字节复刻，
    /// 否则与上游文件哈希不一致。13 字节 = 帧头 + 空数据 + 校验和(xxhash64("") 低 32 位)。
    /// </summary>
    private static readonly byte[] LibimeTrailingEmptyFrame =
    {
        0x28, 0xb5, 0x2f, 0xfd, 0x24, 0x00, 0x01, 0x00, 0x00, 0x99, 0xe9, 0xd8, 0x51,
    };

    /// <summary>
    /// 上游 libime 写出的 1352 字节小词库（无空帧，SHA256 5F8B247EB2D47322…）：
    /// 读入后原样写出必须逐字节一致，从而锁定文件头、zstd 参数与 trie 序列化。
    /// </summary>
    [Fact]
    public void Export_ReproducesUpstreamLibimeBytes()
    {
        var expected = Convert.FromBase64String(UpstreamSmallDictBase64);

        using var input = new MemoryStream(expected);
        var trie = LibimeDictFormat.Read(input);

        using var ms = new MemoryStream();
        LibimeDictFormat.Write(trie, ms);
        var actual = ms.ToArray();

        Assert.Equal(expected.Length, actual.Length);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// 词库主体压缩后超过 libime 一个过滤缓冲（4096 字节）时，尾部必须带上那个空 zstd 帧。
    /// </summary>
    [Fact]
    public void Export_LargeDict_EndsWithLibimeTrailingEmptyFrame()
    {
        var entries = BuildLargeEntries();

        using var ms = new MemoryStream();
        _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        var bytes = ms.ToArray();

        // 压缩主体必须大于 4096 字节，空帧才会出现（这也正是 libime 出现它的条件）。
        Assert.True(
            bytes.Length > 8 + 4096 + LibimeTrailingEmptyFrame.Length,
            $"词库太小，覆盖不到空帧场景：{bytes.Length} 字节。");
        Assert.Equal(
            LibimeTrailingEmptyFrame,
            bytes.Skip(bytes.Length - LibimeTrailingEmptyFrame.Length).ToArray());

        // 带空帧的文件仍要能被自己完整读回（导入器不得把空帧当成错误）。
        ms.Position = 0;
        var result = _importer.ImportAsync(ms).GetAwaiter().GetResult();
        Assert.Equal(entries.Count, result.Entries.Count);
        Assert.Equal(0, result.ErrorCount);
    }

    /// <summary>小词库（压缩后不足一个过滤缓冲）不带空帧，与上游一致。</summary>
    [Fact]
    public void Export_SmallDict_HasNoTrailingEmptyFrame()
    {
        using var ms = new MemoryStream();
        _exporter.ExportAsync(new[] { Entry("你好", 5, "ni", "hao") }, ms).GetAwaiter().GetResult();

        Assert.True(ms.ToArray().Length < 8 + 4096);
        Assert.False(ms.ToArray().AsSpan().EndsWith(LibimeTrailingEmptyFrame));
    }

    /// <summary>构造压缩后远超 4096 字节的词库（词面用固定种子随机汉字，保证可复现）。</summary>
    private static List<WordEntry> BuildLargeEntries()
    {
        // 只用 libime PinyinEncoder 中确定可编码的音节。
        string[] syllables = { "ni", "hao", "xiao", "zhong", "e", "lv", "lve", "nue" };

        var random = new Random(20240607);
        var entries = new List<WordEntry>(1200);
        for (var i = 0; i < 1200; i++)
        {
            var length = 4 + random.Next(6);
            var word = new string(Enumerable.Range(0, length)
                .Select(_ => (char)(0x4e00 + random.Next(0x4000))).ToArray());

            entries.Add(Entry(
                word,
                i + 1,
                syllables[random.Next(syllables.Length)],
                syllables[random.Next(syllables.Length)]));
        }

        return entries;
    }

    private const string UpstreamSmallDictBase64 =
        "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";
}
