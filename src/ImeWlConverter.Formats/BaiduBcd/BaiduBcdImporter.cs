namespace ImeWlConverter.Formats.BaiduBcd;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Baidu Shouji bcd dictionary importer (binary format with pinyin encoded as shengmu+yunmu indices).</summary>
[FormatPlugin("bcd", "百度手机bcd", 1020)]
public sealed partial class BaiduBcdImporter : BinaryFormatImporter
{
    private static readonly string[] Shengmu =
    [
        "c", "d", "b", "f", "g", "h", "ch", "j", "k", "l",
        "m", "n", "", "p", "q", "r", "s", "t", "sh", "zh",
        "w", "x", "y", "z"
    ];

    private static readonly string[] Yunmu =
    [
        "uang", "iang", "iong", "ang", "eng", "ian", "iao", "ing", "ong", "uai",
        "uan", "ai", "an", "ao", "ei", "en", "er", "ua", "ie", "in",
        "iu", "ou", "ia", "ue", "ui", "un", "uo", "a", "e", "i",
        "o", "u", "v"
    ];

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        // Format requires seeking, buffer into MemoryStream
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        ms.Position = 0x350;

        var entries = new List<WordEntry>();

        while (ms.Position < ms.Length)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var entry = ReadWord(ms);
                if (entry != null && !string.IsNullOrEmpty(entry.Word))
                    entries.Add(entry);
            }
            catch
            {
                // Skip malformed entries, continue parsing
            }
        }

        return entries;
    }

    private static WordEntry? ReadWord(Stream fs)
    {
        var temp = new byte[2];
        if (fs.Read(temp, 0, 2) < 2) return null;
        var len = BitConverter.ToInt16(temp, 0);

        if (fs.Read(temp, 0, 2) < 2) return null; // skip 2 unknown bytes

        var pinyinList = new List<string>();
        for (var i = 0; i < len; i++)
        {
            var pyBytes = new byte[2];
            if (fs.Read(pyBytes, 0, 2) < 2) return null;

            var smIdx = pyBytes[0];
            var ymIdx = pyBytes[1];

            if (smIdx == 0xFF)
            {
                // 0xFF 表示英文字母/数字/标点按 ASCII 原样编码
                pinyinList.Add(((char)ymIdx).ToString());
            }
            else if (smIdx < Shengmu.Length && ymIdx < Yunmu.Length)
            {
                pinyinList.Add(Shengmu[smIdx] + Yunmu[ymIdx]);
            }
            else
            {
                pinyinList.Add("");
            }
        }

        var wordBytes = new byte[2 * len];
        if (fs.Read(wordBytes, 0, 2 * len) < 2 * len) return null;
        var word = Encoding.Unicode.GetString(wordBytes);

        if (pinyinList.Count == 0 || string.IsNullOrEmpty(word))
            return null;

        return new WordEntry
        {
            Word = word,
            Rank = 0,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinList)
        };
    }
}
