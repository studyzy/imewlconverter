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

using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.GboardBinary;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// Gboard user_dict_3_3 格式测试。
/// </summary>
public class GboardBinaryTest
{
    private readonly GboardBinaryExporter _exporter = new();
    private readonly GboardBinaryImporter _importer = new();

    private static WordEntry Entry(string word, params string[] pinyin)
    {
        return new WordEntry
        {
            Word = word,
            Code = WordCode.FromSingle(pinyin),
            CodeType = CodeType.Pinyin
        };
    }

    /// <summary>格式元数据: 扩展名与建议文件名(设备上的固定文件名)。</summary>
    [Fact]
    public void Metadata_UsesDeviceFileName()
    {
        Assert.Equal("gboardbin", _exporter.Metadata.Id);
        Assert.Equal(".dict", _exporter.Metadata.FileExtension);
        Assert.Equal("user_dict_3_3", _exporter.Metadata.DefaultFileName);
    }

    /// <summary>导出后再导入, 词面应完整保留。</summary>
    [Fact]
    public void ExportThenImport_RoundTrips()
    {
        var entries = new[]
        {
            Entry("你好", "ni", "hao"),
            Entry("词库转换", "ci", "ku", "zhuan", "huan")
        };

        using var ms = new MemoryStream();
        _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        var bytes = ms.ToArray();

        Assert.True(bytes.Length > 0);

        ms.Position = 0;
        var result = _importer.ImportAsync(ms).GetAwaiter().GetResult();
        var words = result.Entries.Select(e => e.Word).OrderBy(w => w, System.StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "你好", "词库转换" }, words);
    }

    /// <summary>生成的文件必须满足 Gboard 的硬性格式约束。</summary>
    [Fact]
    public void Export_ProducesValidHeader()
    {
        using var ms = new MemoryStream();
        _exporter
            .ExportAsync(new[] { Entry("你好", "ni", "hao") }, ms)
            .GetAwaiter().GetResult();
        var bytes = ms.ToArray();

        // 魔数 96 A4 CB A7(必须按字节写, 不能按 u32 小端写)
        Assert.Equal(0x96, bytes[0]);
        Assert.Equal(0xA4, bytes[1]);
        Assert.Equal(0xCB, bytes[2]);
        Assert.Equal(0xA7, bytes[3]);

        // 文件总长度必须 8 字节对齐, 否则 Gboard 会拒绝并清空词典
        Assert.Equal(0, bytes.Length % 8);
    }

    /// <summary>全部条目都没有拼音时应明确报错, 而不是写出损坏的词典。</summary>
    [Fact]
    public void Export_AllEntriesWithoutPinyin_Throws()
    {
        var entries = new[] { new WordEntry { Word = "你好", CodeType = CodeType.Pinyin } };

        using var ms = new MemoryStream();
        Assert.Throws<System.InvalidOperationException>(
            () => _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult());
    }

    /// <summary>
    /// KEY 的 F1 是「用户选中次数」，必须保持在 Gboard 的量级（官方 1~140）。
    /// 早期版本按名次摊成 1..n 的大跨度，使导入词的 F1 远高于 Gboard 之后学到的
    /// 词，候选顺序被永久冻死（实测：亟需 F1=7775 压过反复选中的 继续 F1=33）。
    /// 这里断言 F1 就是词库给的词频，而不是按名次重排的值。
    /// </summary>
    [Fact]
    public void Export_KeepsFrequenciesInGboardScale()
    {
        var entries = new[]
        {
            Entry("啊", "a") with { Rank = 1 },
            Entry("爱", "ai") with { Rank = 5 },
            Entry("安", "an") with { Rank = 100 },
        };

        using var ms = new MemoryStream();
        _exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        var f1 = ReadKeyF1(ms.ToArray());

        Assert.Equal(new[] { 1, 5, 100 }, f1.OrderBy(x => x).ToArray());
    }

    /// <summary>
    /// 取出所有 KEY 条目的 F1。
    /// FPT2 块 = "FixedPhraseTable\0" + 3B 对齐 + [u32 count] + count×14B，
    /// 条目 = [keyNode:u32][valNode:u32][flag&lt;&lt;24 | F1:u32][D:u16]。
    /// </summary>
    private static List<int> ReadKeyF1(byte[] d)
    {
        var tag = System.Text.Encoding.ASCII.GetBytes("FixedPhraseTable");
        var i1 = IndexOf(d, tag, 0);
        var i2 = IndexOf(d, tag, i1 + 1);
        Assert.True(i2 > 0, "找不到 FPT2 块");

        var count = System.BitConverter.ToUInt32(d, i2 + 20);
        var b = i2 + 28;
        var result = new List<int>();
        for (var e = 0; e < count; e++)
        {
            var flagsF1 = System.BitConverter.ToUInt32(d, b + e * 14 + 8);
            var flag = flagsF1 >> 24;
            if ((flag & 0x40) != 0 && (flag & 0x08) == 0)
                result.Add((int)(flagsF1 & 0xFFFFFF));
        }
        return result;
    }

    private static int IndexOf(byte[] haystack, byte[] needle, int from)
    {
        for (var i = from; i <= haystack.Length - needle.Length; i++)
        {
            var ok = true;
            for (var j = 0; j < needle.Length; j++)
                if (haystack[i + j] != needle[j]) { ok = false; break; }
            if (ok) return i;
        }
        return -1;
    }
}
