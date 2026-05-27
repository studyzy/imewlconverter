namespace ImeWlConverter.Formats.BaiduBdict;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Baidu Pinyin bdict dictionary importer (binary format).</summary>
[FormatPlugin("bdict", "百度分类词库bdict", 100)]
public sealed partial class BaiduBdictImporter : BinaryFormatImporter
{
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

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        var results = new List<WordEntry>();
        using var reader = new BinaryReader(input, Encoding.Unicode, leaveOpen: true);

        // Read end position from header at 0x60
        input.Position = 0x60;
        var endPosition = reader.ReadInt32();

        // Words start at 0x350
        input.Position = 0x350;

        while (input.Position < endPosition)
        {
            ct.ThrowIfCancellationRequested();

            var entry = ReadOneWord(reader, input);
            if (entry == null)
                break;
            if (entry.Word.Length > 0 && entry.Code != null)
                results.Add(entry);
        }

        return results;
    }

    private static WordEntry? ReadOneWord(BinaryReader reader, Stream stream)
    {
        var len = reader.ReadInt32();
        if (len > 1000)
            throw new InvalidDataException("有异常的词库，解析失败");
        if (len == 0)
            return null;

        var pinyinList = new string[len];
        for (var i = 0; i < len; i++)
        {
            var smIndex = reader.ReadByte();
            var ymIndex = reader.ReadByte();
            if (smIndex < Shengmu.Length && ymIndex < Yunmu.Length)
                pinyinList[i] = Shengmu[smIndex] + Yunmu[ymIndex];
            else
                pinyinList[i] = "";
        }

        var wordBytes = reader.ReadBytes(2 * len);
        var word = Encoding.Unicode.GetString(wordBytes);

        var hasValidPinyin = false;
        for (var i = 0; i < pinyinList.Length; i++)
        {
            if (pinyinList[i].Length > 0)
            {
                hasValidPinyin = true;
                break;
            }
        }

        return new WordEntry
        {
            Word = word,
            Rank = 0,
            CodeType = CodeType.Pinyin,
            Code = hasValidPinyin ? WordCode.FromSingle(pinyinList) : null
        };
    }
}
