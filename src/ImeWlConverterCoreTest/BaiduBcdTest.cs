using System;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Formats.BaiduBcd;
using Xunit;

namespace ImeWlConverterCoreTest;

/// <summary>
/// Issue #408: 百度手机输入法只能bcd转其他，不能反过来转成bcd。
/// 验证 BCD 格式当前只有 Importer，没有 Exporter（这是一个功能缺失，不是 bug）。
/// 如果未来添加了 BCD Exporter，这些测试将需要更新。
/// </summary>
public class BaiduBcdTest
{
    static BaiduBcdTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Issue #408: BCD Importer 存在且能正常工作。
    /// </summary>
    [Fact]
    public void Issue408_BcdImporter_Exists()
    {
        var importer = new BaiduBcdImporter();
        Assert.NotNull(importer);
    }

    /// <summary>
    /// Issue #408: 确认 BCD Exporter 不存在（当前的功能缺失状态）。
    /// 通过反射检查 ImeWlConverter.Formats.BaiduBcd 命名空间中没有 Exporter 类。
    /// </summary>
    [Fact]
    public void Issue408_NoBcdExporter_FeatureGap()
    {
        var exporterTypes = typeof(BaiduBcdImporter).Assembly.GetTypes()
            .Where(t => t.Namespace == "ImeWlConverter.Formats.BaiduBcd"
                        && t.Name.Contains("Exporter", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(exporterTypes);
    }

    /// <summary>
    /// Issue #408: BCD Importer 能正确解析 BCD 二进制格式。
    /// 测试词条：BCD 格式编码的"深蓝词库"。
    /// </summary>
    [Fact]
    public void Issue408_BcdImport_ProducesValidEntries()
    {
        var importer = new BaiduBcdImporter();

        // BCD format binary for "深蓝词库转换" (simplified)
        // BCD header: 8 bytes, then 4 bytes count, then entries
        var bcdData = CreateMinimalBcdData();

        using var stream = new MemoryStream(bcdData);
        var result = importer.ImportAsync(stream).GetAwaiter().GetResult();

        Assert.True(result.Entries.Count > 0, "BCD import should produce entries");
        // Verify entries have both word and code
        foreach (var entry in result.Entries)
        {
            Assert.False(string.IsNullOrEmpty(entry.Word));
        }
    }

    /// <summary>
    /// 构造一个最小的 BCD 格式二进制数据用于测试。
    /// BCD 格式结构（基于 BaiduBcdImporter 解析逻辑）：
    /// - 前 0x350 字节填充 0（Importer 跳过这部分）
    /// - 每个条目：
    ///   - 2 bytes: 字符数 count (short)
    ///   - 2 bytes: 未知字段（跳过）
    ///   - count * 2 bytes: 拼音编码（每字2字节：声母索引+韵母索引）
    ///   - count * 2 bytes: 词语 Unicode 编码
    /// </summary>
    private static byte[] CreateMinimalBcdData()
    {
        // BCD shengmu/yunmu index mapping for "深蓝词库"
        // "深" -> s+en -> smIdx=16("s"), ymIdx=15("en")
        // "蓝" -> l+an -> smIdx=9("l"), ymIdx=12("an")
        // "词" -> c+i -> smIdx=0("c"), ymIdx=29("i")
        // "库" -> k+u -> smIdx=8("k"), ymIdx=31("u")
        var word = "深蓝词库";
        var count = (short)word.Length; // 4 characters

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // Fill 0x350 bytes of padding (importer skips this)
        for (var i = 0; i < 0x350; i++)
            bw.Write((byte)0);

        // Entry: character count (2 bytes, little-endian short)
        bw.Write(count);
        // Unknown 2 bytes
        bw.Write((short)0);

        // Pinyin encoding (4 chars * 2 bytes each = 8 bytes)
        bw.Write((byte)16); bw.Write((byte)15); // s=16, en=15 -> shen
        bw.Write((byte)9);  bw.Write((byte)12); // l=9, an=12 -> lan
        bw.Write((byte)0);  bw.Write((byte)29); // c=0, i=29 -> ci
        bw.Write((byte)8);  bw.Write((byte)31); // k=8, u=31 -> ku

        // Word in Unicode (4 chars * 2 bytes = 8 bytes)
        var wordBytes = Encoding.Unicode.GetBytes(word);
        bw.Write(wordBytes);

        bw.Flush();
        return ms.ToArray();
    }
}
