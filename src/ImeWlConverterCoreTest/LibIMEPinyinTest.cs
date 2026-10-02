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
}
