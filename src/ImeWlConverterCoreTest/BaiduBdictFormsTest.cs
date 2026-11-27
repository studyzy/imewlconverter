using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.BaiduBdict;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// 百度 bdict 三种词条格式回归：#216（含英文词条 + 最后一个词条被丢弃）、#225（颜文字词库 0 条）。
/// 用内存构造的二进制样本覆盖格式一(0xFF)、格式二(纯英文)、格式三 A/B(编码与词不等长)。
/// </summary>
public class BaiduBdictFormsTest
{
    private const int DataStart = 0x350;

    private static readonly string[] Shengmu =
    {
        "c", "d", "b", "f", "g", "h", "ch", "j", "k", "l",
        "m", "n", "", "p", "q", "r", "s", "t", "sh", "zh",
        "w", "x", "y", "z"
    };

    private static readonly string[] Yunmu =
    {
        "uang", "iang", "iong", "ang", "eng", "ian", "iao", "ing", "ong", "uai",
        "uan", "ai", "an", "ao", "ei", "en", "er", "ua", "ie", "in",
        "iu", "ou", "ia", "ue", "ui", "un", "uo", "a", "e", "i",
        "o", "u", "v"
    };

    private static List<WordEntry> Import(byte[] data)
    {
        using var stream = new MemoryStream(data);
        return new BaiduBdictImporter().ImportAsync(stream).GetAwaiter().GetResult().Entries.ToList();
    }

    [Fact]
    public void Form1_AsciiMarker_KeepsCodesAndWords()
    {
        var regular = Combine(
            F1("A股", [Ascii('A'), Syl("gu")]),
            F1("NBA", [Ascii('N'), Ascii('B'), Ascii('A')]),
            F1("3D电影", [Ascii('3'), Ascii('D'), Syl("dian"), Syl("ying")]));

        var entries = Import(Build(regular, [], []));

        Assert.Equal(3, entries.Count);
        Assert.Equal("A股", entries[0].Word);
        Assert.Equal(new[] { "A", "gu" }, Segments(entries[0]));
        Assert.Equal("NBA", entries[1].Word);
        Assert.Equal(new[] { "N", "B", "A" }, Segments(entries[1]));
        Assert.Equal("3D电影", entries[2].Word);
        Assert.Equal(new[] { "3", "D", "dian", "ying" }, Segments(entries[2]));
    }

    [Fact]
    public void Form2_PureEnglishWords_AreImported()
    {
        var eng = Combine(F2("NBA"), F2("CCTV"), F2("iPhone"), F2("3D"));

        var entries = Import(Build([], eng, []));

        Assert.Equal(4, entries.Count);
        Assert.Equal(new[] { "NBA", "CCTV", "iPhone", "3D" }, entries.Select(e => e.Word).ToArray());
        Assert.All(entries, e => Assert.Equal(e.Word, e.Code!.GetPrimaryCode("")));
    }

    [Fact]
    public void Form3A_KaomojiSection_IsImported()
    {
        var mixed = Combine(
            F3A("kaomoji", "(≧▽≦)"),
            F3A("yanwenzi", "囧rz"));

        var entries = Import(Build([], [], mixed));

        Assert.Equal(2, entries.Count);
        Assert.Equal("(≧▽≦)", entries[0].Word);
        Assert.Equal(new[] { "kaomoji" }, Segments(entries[0]));
        Assert.Equal("囧rz", entries[1].Word);
    }

    [Fact]
    public void Form3B_LegacyMixedSection_IsImported()
    {
        var mixed = F3B("liao'bu'qi'de'hu'li'ba'ba", "赤裸战士");

        var entries = Import(Build([], [], mixed));

        Assert.Single(entries);
        Assert.Equal("赤裸战士", entries[0].Word);
        Assert.Equal("liaobuqidehulibaba", entries[0].Code!.GetPrimaryCode(""));
    }

    [Fact]
    public void TrailingMixedEntry_IsNotDropped()
    {
        var regular = Combine(
            F1("北京", [Syl("bei"), Syl("jing")]),
            F1("上海", [Syl("shang"), Syl("hai")]),
            F1("广州", [Syl("guang"), Syl("zhou")]));
        var mixed = F3A("lastword", "最后一个词");

        var entries = Import(Build(regular, [], mixed));

        Assert.Equal(4, entries.Count);
        Assert.Equal("最后一个词", entries[3].Word);
    }

    [Fact]
    public void Form1_NonZeroFrequency_UsesTwoByteLengths()
    {
        var regular = Combine(
            F1("北京", [Syl("bei"), Syl("jing")], freq: 100),
            F1("上海", [Syl("shang"), Syl("hai")], freq: 200));

        var entries = Import(Build(regular, [], []));

        Assert.Equal(2, entries.Count);
        Assert.Equal("北京", entries[0].Word);
        Assert.Equal("上海", entries[1].Word);
    }

    [Fact]
    public void RealSample_MovieIncludesTrailingMixedEntry()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Test", "movie.bdict");
        var entries = Import(File.ReadAllBytes(path));

        Assert.True(entries.Count >= 65477,
            $"movie.bdict 仅导入 {entries.Count} 条，末尾混合词条疑似仍被丢弃");
        Assert.Contains(entries, e => e.Word == "赤裸战士");
    }

    private static string[] Segments(WordEntry entry) =>
        entry.Code!.Segments.Select(s => s.Count > 0 ? s[0] : "").ToArray();

    private static (byte, byte) Ascii(char c) => (0xFF, (byte)c);

    private static (byte, byte) Syl(string pinyin)
    {
        for (byte sm = 0; sm < Shengmu.Length; sm++)
            for (byte ym = 0; ym < Yunmu.Length; ym++)
                if (Shengmu[sm] + Yunmu[ym] == pinyin)
                    return (sm, ym);
        throw new ArgumentException($"测试样本中的音节无法映射: {pinyin}");
    }

    private static byte[] F1(string word, (byte sm, byte ym)[] codes, ushort freq = 0)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write((ushort)codes.Length);
        w.Write(freq);
        foreach (var (sm, ym) in codes)
        {
            w.Write(sm);
            w.Write(ym);
        }
        w.Write(Encoding.Unicode.GetBytes(word));
        return ms.ToArray();
    }

    private static byte[] F2(string word, ushort freq = 0)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write((ushort)word.Length);
        w.Write(freq);
        w.Write(Encoding.ASCII.GetBytes(word));
        return ms.ToArray();
    }

    private static byte[] F3A(string code, string word, ushort freq = 0)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write((ushort)code.Length);
        w.Write(freq);
        w.Write((ushort)0); // 格式三-A 的 0000 标记
        w.Write((ushort)word.Length);
        w.Write(Encoding.Unicode.GetBytes(code));
        w.Write(Encoding.Unicode.GetBytes(word));
        return ms.ToArray();
    }

    private static byte[] F3B(string code, string word)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write((uint)0); // pyLen=0 + freq=0
        w.Write((ushort)code.Length);
        w.Write((ushort)word.Length);
        w.Write(Encoding.Unicode.GetBytes(code));
        w.Write(Encoding.Unicode.GetBytes(word));
        return ms.ToArray();
    }

    private static byte[] Combine(params byte[][] chunks)
    {
        var result = new List<byte>();
        foreach (var chunk in chunks)
            result.AddRange(chunk);
        return result.ToArray();
    }

    private static byte[] Build(byte[] regular, byte[] eng, byte[] mixed)
    {
        var engOffset = DataStart + regular.Length;
        var mixedOffset = engOffset + eng.Length;
        var buffer = new byte[mixedOffset + mixed.Length];

        U32(buffer, 0x40, DataStart);
        U32(buffer, 0x44, regular.Length);
        U32(buffer, 0x50, eng.Length > 0 ? engOffset : 0);
        U32(buffer, 0x54, eng.Length);
        U32(buffer, 0x60, mixed.Length > 0 ? mixedOffset : 0);
        U32(buffer, 0x64, mixed.Length);

        regular.CopyTo(buffer, DataStart);
        eng.CopyTo(buffer, engOffset);
        mixed.CopyTo(buffer, mixedOffset);
        return buffer;
    }

    private static void U32(byte[] buffer, int offset, int value) =>
        BitConverter.GetBytes((uint)value).CopyTo(buffer, offset);
}
