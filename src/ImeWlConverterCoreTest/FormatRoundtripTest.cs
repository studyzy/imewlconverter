using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.BaiduPinyin;
using ImeWlConverter.Formats.BaiduShouji;
using ImeWlConverter.Formats.BaiduShoujiEng;
using ImeWlConverter.Formats.BingPinyin;
using ImeWlConverter.Formats.Chaoyin;
using ImeWlConverter.Formats.ChinesePyim;
using ImeWlConverter.Formats.FitInput;
using ImeWlConverter.Formats.Gboard;
using ImeWlConverter.Formats.GooglePinyin;
using ImeWlConverter.Formats.iFlyIME;
using ImeWlConverter.Formats.Jidian;
using ImeWlConverter.Formats.LibIMEText;
using ImeWlConverter.Formats.Libpinyin;
using ImeWlConverter.Formats.MacPlist;
using ImeWlConverter.Formats.MsPinyin;
using ImeWlConverter.Formats.NoPinyinWordOnly;
using ImeWlConverter.Formats.PinyinJiaJia;
using ImeWlConverter.Formats.QQPinyin;
using ImeWlConverter.Formats.QQPinyinEng;
using ImeWlConverter.Formats.QQShouji;
using ImeWlConverter.Formats.Rime;
using ImeWlConverter.Formats.SelfDefining;
using ImeWlConverter.Formats.ShouXinPinyin;
using ImeWlConverter.Formats.SinaPinyin;
using ImeWlConverter.Formats.SougouPinyin;
using ImeWlConverter.Formats.SougouScel;
using ImeWlConverter.Formats.Win10Ms;
using ImeWlConverter.Formats.Win10MsSelfStudy;
using ImeWlConverter.Formats.Wubi;
using ImeWlConverter.Formats.Xiaoxiao;
using ImeWlConverter.Formats.YahooKeyKey;
using ImeWlConverter.Formats.ZiGuangPinyin;
using Xunit;

namespace ImeWlConverterCoreTest;

/// <summary>
/// Roundtrip tests for all formats that have both Importer and Exporter.
/// Export → Import should preserve word text.
/// </summary>
public class FormatRoundtripTest
{
    static FormatRoundtripTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static readonly IReadOnlyList<WordEntry> PinyinEntries = new List<WordEntry>
    {
        new() { Word = "深蓝词库", Code = WordCode.FromSingle(new[] { "shen", "lan", "ci", "ku" }), Rank = 5, CodeType = CodeType.Pinyin },
        new() { Word = "词库转换", Code = WordCode.FromSingle(new[] { "ci", "ku", "zhuan", "huan" }), Rank = 3, CodeType = CodeType.Pinyin },
    };

    private static readonly IReadOnlyList<WordEntry> EnglishEntries = new List<WordEntry>
    {
        new() { Word = "hello", Code = WordCode.FromSingle(new[] { "h", "e", "l", "l", "o" }), Rank = 1, CodeType = CodeType.Pinyin, IsEnglish = true },
        new() { Word = "world", Code = WordCode.FromSingle(new[] { "w", "o", "r", "l", "d" }), Rank = 2, CodeType = CodeType.Pinyin, IsEnglish = true },
    };

    private static IReadOnlyList<WordEntry> WubiEntries => new List<WordEntry>
    {
        new() { Word = "深蓝", Code = WordCode.FromSingle(new[] { "ipad" }), Rank = 1, CodeType = CodeType.Wubi86 },
    };

    private static IReadOnlyList<WordEntry> ZhengmaEntries => new List<WordEntry>
    {
        new() { Word = "深蓝", Code = WordCode.FromSingle(new[] { "vwfl" }), Rank = 1, CodeType = CodeType.Zhengma },
    };

    private static List<(IFormatImporter importer, IFormatExporter exporter, IReadOnlyList<WordEntry> entries, bool checkCode)> BuildFormatPairs()
    {
        return new List<(IFormatImporter, IFormatExporter, IReadOnlyList<WordEntry>, bool)>
        {
            // Text-based pinyin formats
            (new BaiduPinyinImporter(), new BaiduPinyinExporter(), PinyinEntries, true),
            (new BaiduShoujiImporter(), new BaiduShoujiExporter(), PinyinEntries, false),
            (new BaiduShoujiEngImporter(), new BaiduShoujiEngExporter(), EnglishEntries, true),
            (new BingPinyinImporter(), new BingPinyinExporter(), PinyinEntries, true),
            (new ChaoyinImporter(), new ChaoyinExporter(), PinyinEntries, true),
            (new ChinesePyimImporter(), new ChinesePyimExporter(), PinyinEntries, true),
            (new FitInputImporter(), new FitInputExporter(), PinyinEntries, true),
            (new GboardImporter(), new GboardExporter(), PinyinEntries, true),
            (new GooglePinyinImporter(), new GooglePinyinExporter(), PinyinEntries, true),
            (new LibpinyinImporter(), new LibpinyinExporter(), PinyinEntries, true),
            (new MacPlistImporter(), new MacPlistExporter(), PinyinEntries, true),
            (new MsPinyinImporter(), new MsPinyinExporter(), PinyinEntries, true),
            (new PinyinJiaJiaImporter(), new PinyinJiaJiaExporter(), PinyinEntries, true),
            (new QQPinyinImporter(), new QQPinyinExporter(), PinyinEntries, true),
            (new QQPinyinEngImporter(), new QQPinyinEngExporter(), PinyinEntries, true),
            (new QQShoujiImporter(), new QQShoujiExporter(), PinyinEntries, false),
            (new RimeImporter(), new RimeExporter(), PinyinEntries, true),
            (new ShouXinPinyinImporter(), new ShouXinPinyinExporter(), PinyinEntries, true),
            (new SinaPinyinImporter(), new SinaPinyinExporter(), PinyinEntries, true),
            (new SougouPinyinImporter(), new SougouPinyinExporter(), PinyinEntries, true),
            (new YahooKeyKeyImporter(), new YahooKeyKeyExporter(), PinyinEntries, true),
            (new ZiGuangPinyinImporter(), new ZiGuangPinyinExporter(), PinyinEntries, true),
            (new iFlyIMEImporter(), new iFlyIMEExporter(), PinyinEntries, true),

            // Self-defining format
            (new SelfDefiningImporter { OrderSpec = "213", PinyinSeparator = '\'', FieldSeparator = ' ', ShowPinyin = true, ShowWord = true, ShowRank = true },
             new SelfDefiningExporter { OrderSpec = "213", PinyinSeparator = '\'', FieldSeparator = ' ', ShowPinyin = true, ShowWord = true, ShowRank = true },
             PinyinEntries, true),

            // Binary formats - Win10
            (new Win10MsPinyinImporter(), new Win10MsPinyinExporter(), PinyinEntries, true),
            (new Win10MsWubiImporter(), new Win10MsWubiExporter(), PinyinEntries, true),
            (new Win10MsPinyinSelfStudyImporter(), new Win10MsPinyinSelfStudyExporter(), PinyinEntries, true),

            // SougouScel (binary)
            (new SougouScelImporter(), new SougouScelExporter(), PinyinEntries, true),

            // Wubi formats
            (new Wubi86Importer(), new Wubi86Exporter(), WubiEntries, true),
            (new Wubi98Importer(), new Wubi98Exporter(), WubiEntries, true),
            (new WubiNewAgeImporter(), new WubiNewAgeExporter(), WubiEntries, true),
            (new QQWubiImporter(), new QQWubiExporter(), WubiEntries, true),
            (new XiaoyaWubiImporter(), new XiaoyaWubiExporter(), WubiEntries, true),

            // Jidian
            (new JidianImporter(), new JidianExporter(), WubiEntries, true),
            (new JidianZhengmaImporter(), new JidianZhengmaExporter(), ZhengmaEntries, true),

            // Xiaoxiao
            (new XiaoxiaoImporter(), new XiaoxiaoExporter(), PinyinEntries, true),
        };
    }

    [Theory]
    [MemberData(nameof(RoundtripFormatData))]
    public void ExportThenImport_ShouldPreserveWords(
        IFormatImporter importer, IFormatExporter exporter,
        IReadOnlyList<WordEntry> entries, bool checkCode, string formatName)
    {
        // Export
        using var exportStream = new MemoryStream();
        var exportResult = exporter.ExportAsync(entries, exportStream).GetAwaiter().GetResult();
        Assert.True(exportResult.EntryCount > 0, $"Export produced 0 entries for {formatName}");
        Assert.True(exportStream.Length > 0, $"Export produced empty stream for {formatName}");

        // Import back
        exportStream.Position = 0;
        var importResult = importer.ImportAsync(exportStream).GetAwaiter().GetResult();

        Assert.True(importResult.Entries.Count > 0, $"Import produced 0 entries for {formatName}");

        // Verify all original words are present
        var importedWords = importResult.Entries.Select(e => e.Word).ToHashSet();
        foreach (var entry in entries)
        {
            Assert.Contains(entry.Word, importedWords);
        }

        // Optionally verify code is non-empty
        if (checkCode)
        {
            foreach (var original in entries)
            {
                var imported = importResult.Entries.FirstOrDefault(e => e.Word == original.Word);
                Assert.NotNull(imported);
                if (imported!.Code != null && original.Code != null)
                {
                    var impCode = imported.Code.GetPrimaryCode("'").ToLowerInvariant();
                    Assert.False(string.IsNullOrEmpty(impCode), $"Code empty for '{original.Word}' in {formatName}");
                }
            }
        }
    }

    public static IEnumerable<object[]> RoundtripFormatData()
    {
        var pairs = BuildFormatPairs();
        for (var i = 0; i < pairs.Count; i++)
        {
            var (importer, exporter, entries, checkCode) = pairs[i];
            var name = importer.Metadata.DisplayName;
            yield return new object[] { importer, exporter, entries, checkCode, name };
        }
    }
}
