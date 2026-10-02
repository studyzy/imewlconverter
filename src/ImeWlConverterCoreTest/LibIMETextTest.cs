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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Formats.LibIMEText;
using Xunit;

namespace Studyzy.IMEWLConverter.Test;

/// <summary>
/// LibIME 文本词库格式测试。文本行格式与 libime <c>loadText</c> 一致：<c>汉字 拼音 [词频]</c>。
/// </summary>
public class LibIMETextTest
{
    private readonly LibIMETextExporter _exporter = new();
    private readonly LibIMETextImporter _importer = new();

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

    private static string Export(LibIMETextExporter exporter, params WordEntry[] entries)
    {
        using var ms = new MemoryStream();
        exporter.ExportAsync(entries, ms).GetAwaiter().GetResult();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static IReadOnlyList<WordEntry> Import(LibIMETextImporter importer, string text)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return importer.ImportAsync(ms).GetAwaiter().GetResult().Entries;
    }

    /// <summary>导出应按 libime 文本顺序输出「汉字 拼音 词频」，且 lue/nue 写成 lve/nve。</summary>
    [Fact]
    public void Export_WritesWordPinyinRank()
    {
        var text = Export(_exporter, Entry("你好", 5, "ni", "hao"));
        Assert.Equal("你好 ni'hao 5\n", text.Replace("\r\n", "\n"));

        var lue = Export(_exporter, Entry("绿色", 1, "lue", "se"));
        Assert.StartsWith("绿色 lve'se 1", lue);
    }

    /// <summary>词面在拼音之前（历史 bug：导入器把两个字段读反了）。</summary>
    [Fact]
    public void Import_ReadsWordThenPinyin()
    {
        var entries = Import(_importer, "你好 ni'hao 5\n");
        var entry = Assert.Single(entries);

        Assert.Equal("你好", entry.Word);
        Assert.Equal("ni'hao", entry.Code!.GetPrimaryCode("'"));
        Assert.Equal(5, entry.Rank);
    }

    /// <summary>词频可省略（libime 允许 2 列），缺失时 Rank 归 0。</summary>
    [Fact]
    public void Import_WithoutRank_DefaultsToZero()
    {
        var entry = Assert.Single(Import(_importer, "小企鹅 xiao'qi'e\n"));

        Assert.Equal("小企鹅", entry.Word);
        Assert.Equal("xiao'qi'e", entry.Code!.GetPrimaryCode("'"));
        Assert.Equal(0, entry.Rank);
    }

    /// <summary>导出后再导入应完整往返。</summary>
    [Fact]
    public void ExportThenImport_RoundTrips()
    {
        var entries = new[]
        {
            Entry("你好", 5, "ni", "hao"),
            Entry("小企鹅", 3, "xiao", "qi", "e"),
        };

        var text = Export(_exporter, entries);
        var imported = Import(_importer, text).ToDictionary(e => e.Word);

        Assert.Equal(2, imported.Count);
        Assert.Equal("ni'hao", imported["你好"].Code!.GetPrimaryCode("'"));
        Assert.Equal(5, imported["你好"].Rank);
        Assert.Equal("xiao'qi'e", imported["小企鹅"].Code!.GetPrimaryCode("'"));
        Assert.Equal(3, imported["小企鹅"].Rank);
    }

    /// <summary>字段不足的行应被忽略而不是产出错误词条。</summary>
    [Theory]
    [InlineData("")]
    [InlineData("你好")]
    [InlineData("   ")]
    [InlineData("a b ni'hao 5")]
    [InlineData("你好 ni'hao 5 extra")]
    public void Import_MalformedLine_IsSkipped(string line)
    {
        Assert.Empty(Import(_importer, line));
    }

    /// <summary>词面含空白时导出应按 fcitx escapeForValue 加引号并转义（与 libime saveText 一致）。</summary>
    [Fact]
    public void Export_EscapesWordWithWhitespace()
    {
        var text = Export(_exporter, Entry("a b", 5, "ni", "hao"));
        Assert.Equal("\"a b\" ni'hao 5\n", text.Replace("\r\n", "\n"));

        var tab = Export(_exporter, Entry("a\tb", 1, "ni"));
        Assert.Equal("\"a\\tb\" ni 1\n", tab.Replace("\r\n", "\n"));
    }

    /// <summary>导入时应能还原被引号包裹（含转义）的词面。</summary>
    [Fact]
    public void Import_ReadsQuotedWord()
    {
        var entry = Assert.Single(Import(_importer, "\"a b\" ni'hao 5\n"));

        Assert.Equal("a b", entry.Word);
        Assert.Equal("ni'hao", entry.Code!.GetPrimaryCode("'"));
        Assert.Equal(5, entry.Rank);
    }

    /// <summary>含空白、引号、反斜杠的词面导出后再导入应完整往返。</summary>
    [Theory]
    [InlineData("a b")]
    [InlineData("a\"b")]
    [InlineData("a\\b")]
    [InlineData("a\tb")]
    [InlineData(" 前后空格 ")]
    public void ExportThenImport_RoundTripsEscapedWord(string word)
    {
        var text = Export(_exporter, Entry(word, 7, "ni", "hao"));
        var entry = Assert.Single(Import(_importer, text));

        Assert.Equal(word, entry.Word);
        Assert.Equal("ni'hao", entry.Code!.GetPrimaryCode("'"));
        Assert.Equal(7, entry.Rank);
    }

    /// <summary>EscapeValue 的转义表应与 fcitx escapeForValue 完全一致。</summary>
    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("", "")]
    [InlineData("a b", "\"a b\"")]
    [InlineData("a\"b", "\"a\\\"b\"")]
    [InlineData("a\\b", "\"a\\\\b\"")]
    [InlineData("a\nb", "\"a\\nb\"")]
    [InlineData("a\tb", "\"a\\tb\"")]
    [InlineData("a\rb", "\"a\\rb\"")]
    [InlineData("a\fb", "\"a\\fb\"")]
    [InlineData("a\vb", "\"a\\vb\"")]
    public void EscapeValue_MatchesFcitxRules(string input, string expected)
    {
        Assert.Equal(expected, LibimeTextEscaping.EscapeValue(input));
    }

    /// <summary>Tokenize 应与 libime loadTextImpl 的 consumeMaybeEscapedValue 循环一致。</summary>
    [Theory]
    [InlineData("你好 ni'hao 5", 3)]
    [InlineData("\"a b\" ni'hao 5", 3)]
    [InlineData("你好 ni'hao", 2)]
    [InlineData("你好 ni'hao 5 ", 3)]
    [InlineData("   ", 0)]
    [InlineData("", 0)]
    [InlineData("\"\" ni 1", 3)]
    public void Tokenize_MatchesLibimeRules(string line, int expectedCount)
    {
        var tokens = LibimeTextEscaping.Tokenize(line);
        Assert.Equal(expectedCount, tokens.Count);
    }

    /// <summary>带转义序列的引号 token 应还原为原始字符。</summary>
    [Fact]
    public void Tokenize_UnescapesQuotedTokens()
    {
        Assert.Equal(new[] { "a\"b", "ni", "1" }, LibimeTextEscaping.Tokenize("\"a\\\"b\" ni 1"));
        Assert.Equal(new[] { "a\\b", "ni" }, LibimeTextEscaping.Tokenize("\"a\\\\b\" ni"));
        Assert.Equal(new[] { "a\nb", "ni" }, LibimeTextEscaping.Tokenize("\"a\\nb\" ni"));
        // 非法转义序列按普通字符处理，反斜杠被丢弃（与 fcitx 一致）。
        Assert.Equal(new[] { "aqb", "ni" }, LibimeTextEscaping.Tokenize("\"a\\qb\" ni"));
        // 引号未闭合时整段按未加引号处理。
        Assert.Equal(new[] { "\"abc", "ni" }, LibimeTextEscaping.Tokenize("\"abc ni"));
    }
    /// <summary>
    /// 词面含非 BMP 汉字（CJK 扩展 B 及以后，UTF-16 代理对）时，
    /// 导入的词条自带拼音必须原样保留：扩展区汉字不是标点，且与 segment 的对齐
    /// 必须按 Unicode 码点而非 UTF-16 码元，否则拼音会被清空、后续字会串位。
    /// 期望值取自 libime 自身导出的文本（libime_pinyindict -t）。
    /// </summary>
    [Fact]
    public void NonBmpChars_PreserveImportedPinyin()
    {
        var imported = Import(_importer, LibimeNonBmpText);
        Assert.Equal(34, imported.Count);

        // 与 CLI/GUI 相同：默认 CodeGenerationOptions 走一遍后处理
        var processed = CodeGenerationPostProcessor.Apply(imported, new CodeGenerationOptions());
        Assert.Equal(34, processed.Count);

        var exported = Export(_exporter, processed.ToArray());
        Assert.Equal(LibimeNonBmpText + "\n", exported.Replace("\r\n", "\n"));
    }

    /// <summary>
    /// libime 导出的含非 BMP 生僻字的文本词库（词面用其自带拼音，词频均为 1）。
    /// </summary>
    private const string LibimeNonBmpText = """
        豹江𫚉娘 bao'jiang'hong'niang 1
        𬭛娘 bo'niang 1
        𰻝𰻝面 biang'biang'mian 1
        𰻝之歌 biang'zhi'ge 1
        不会飞的蝴蝶与天空之𩾇 bu'hui'fei'de'hu'die'yu'tian'kong'zhi'hu 1
        喷射𫚉鱼 pen'she'hong'yu 1
        马云𫘧 ma'yun'lu 1
        𫓧娘 fu'niang 1
        𬭊娘 du'niang 1
        乐𬘭 le'lin 1
        𫟷娘 li'niang 1
        𬬻娘 lu'niang 1
        陆生𩽾𩾌 lu'sheng'an'kang 1
        𬬭特 lun'te 1
        𬬭娘 lun'niang 1
        口𠱞子早鸟 kou'ran'zi'zao'niao 1
        𫚉人 hong'ren 1
        祭𠱞子 ji'ran'zi 1
        金𩾇城 jin'hu'cheng 1
        青𮣲剑 qing'gang'jian 1
        𬭳娘 xi'niang 1
        张虎乐𬘭 zhang'hu'le'lin 1
        𥐟部花凛 chai'bu'hua'lin 1
        赤𩾇 chi'hu 1
        深渊的电击𫚉 shen'yuan'de'dian'ji'hong 1
        深渊𩽾𩾌 shen'yuan'an'kang 1
        孙𬘭 sun'lin 1
        郁辰𫍽 yu'chen'xuan 1
        吴𬀪 wu'xian 1
        𩽾𩾌 an'kang 1
        𩽾𩾌鱼 an'kang'yu 1
        𩽾𩾌鱼娘 an'kang'yu'niang 1
        𩽾𩾌鱼舞曲 an'kang'yu'wu'qu 1
        獓𤝱 ao'ye 1
        """;
}