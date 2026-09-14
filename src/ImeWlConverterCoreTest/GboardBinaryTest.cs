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
}
