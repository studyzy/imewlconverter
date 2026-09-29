using System;
using System.IO;
using System.Text;
using ImeWlConverter.Formats.BaiduBcd;
using ImeWlConverter.Formats.BaiduPinyinBackup;
using ImeWlConverter.Formats.Jidian;
using ImeWlConverter.Formats.RimeUserDb;
using ImeWlConverter.Formats.Gboard;
using ImeWlConverter.Formats.SougouBin;
using ImeWlConverter.Formats.Win10MsSelfStudy;
using ImeWlConverter.Formats.Win10Ms;
using ImeWlConverter.Formats.ZiGuangUwl;
using ImeWlConverter.Abstractions.Contracts;
using Xunit;

namespace ImeWlConverterCoreTest;

/// <summary>
/// 使用真实下载的词库文件验证各二进制/专用格式导入器。
/// 样本均位于 Test/ 目录，最小词条数由 CLI 实测得出。
/// </summary>
public class RealDictionaryImportTest
{
    static RealDictionaryImportTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static IFormatImporter CreateImporter(string formatId) => formatId switch
    {
        "sgpybin" => new SougouBinImporter(),
        "bcd" => new BaiduBcdImporter(),
        "bdpybin" => new BaiduPinyinBackupImporter(),
        "win10mspyss" => new Win10MsPinyinSelfStudyImporter(),
        "win10mswb" => new Win10MsWubiImporter(),
        "rimedb" => new RimeUserDbImporter(),
        "jd" => new JidianImporter(),
        "uwl" => new ZiGuangUwlImporter(),
        "gboard" => new GboardImporter(),
        _ => throw new ArgumentException($"未知格式: {formatId}")
    };

    [Theory]
    [InlineData("sgpybin", "搜狗备份.bin", 30)] // 搜狗输入法"导出词库"备份，30 条
    [InlineData("bcd", "记者必备.bcd", 1000)] // 百度手机词库，1398 条
    [InlineData("bdpybin", "百度拼音备份.bin", 1)] // 百度 PC 输入法备份，1 条
    [InlineData("win10mspyss", "Win10拼音自学习_ChsPinyinUDL.dat", 1)] // Win10 微软拼音自学习，2 条
    [InlineData("win10mswb", "微软五笔UserDefinedPhrase.dat", 1)] // Win10 微软五笔 UDP，2 条
    [InlineData("rimedb", "rime_luna_pinyin_export.txt", 10)] // Rime 用户词典导出，17 条
    [InlineData("jd", "极点五笔_freeime_user.txt", 20)] // 极点五笔用户词文本，27 条
    [InlineData("uwl", "华宇紫光economics.uwl", 1000)] // 华宇紫光词库（经济类），7737 条
    [InlineData("gboard", "GBoard_dictionary.txt", 2)] // Gboard 文本词典导出，2 条
    public void Import_RealDictionary_ProducesEntries(string formatId, string fileName, int minCount)
    {
        var importer = CreateImporter(formatId);
        var path = Path.Combine(
            Path.GetDirectoryName(typeof(RealDictionaryImportTest).Assembly.Location)!,
            "Test", fileName);

        using var stream = File.OpenRead(path);
        var result = importer.ImportAsync(stream).GetAwaiter().GetResult();

        Assert.NotNull(result.Entries);
        Assert.True(result.Entries.Count >= minCount,
            $"{fileName} 仅导入 {result.Entries.Count} 条（预期 >= {minCount}）");
    }
}
