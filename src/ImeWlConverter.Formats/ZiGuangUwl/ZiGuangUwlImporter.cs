namespace ImeWlConverter.Formats.ZiGuangUwl;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>ZiGuang Pinyin uwl dictionary importer (binary format).</summary>
[FormatPlugin("uwl", "紫光拼音词库uwl", 171)]
public sealed partial class ZiGuangUwlImporter : BinaryFormatImporter
{
    private static readonly string[] Shengmu =
    {
        "", "b", "c", "ch", "d", "f", "g", "h", "j", "k",
        "l", "m", "n", "p", "q", "r", "s", "sh", "t", "w",
        "x", "y", "z", "zh"
    };

    private static readonly string[] Yunmu =
    {
        "ang", "a", "ai", "an", "ang", "ao", "e", "ei", "en", "eng",
        "er", "i", "ia", "ian", "iang", "iao", "ie", "in", "ing", "iong",
        "iu", "o", "ong", "ou", "u", "ua", "uai", "uan", "uang", "ue",
        "ui", "un", "uo", "v"
    };

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        var results = new List<WordEntry>();
        using var reader = new BinaryReader(input, Encoding.Unicode, leaveOpen: true);

        // Read encoding flag at offset 0x02
        input.Position = 0x02;
        var enc = (byte)input.ReadByte();
        var encoding = enc == 0x09 ? Encoding.Unicode : Encoding.GetEncoding("GB18030");

        // Read word count and segment count at offset 0x44
        input.Position = 0x44;
        var countWord = reader.ReadInt32();
        var segmentCount = reader.ReadInt32();

        for (var i = 0; i < segmentCount; i++)
        {
            ct.ThrowIfCancellationRequested();

            input.Position = 0xC00 + 1024 * i;
            ParseSegment(reader, input, encoding, results);
        }

        return results;
    }

    private static void ParseSegment(BinaryReader reader, Stream stream, Encoding encoding, List<WordEntry> results)
    {
        var indexNumber = reader.ReadInt32();
        var ff = reader.ReadInt32();
        var wordLenEnums = reader.ReadInt32();
        var wordByteLen = reader.ReadInt32();

        var bytesRead = 0;
        while (bytesRead < wordByteLen)
        {
            var entry = ParseWord(stream, encoding, out var entryLen);
            bytesRead += entryLen;
            if (entry != null)
                results.Add(entry);
        }
    }

    private static WordEntry? ParseWord(Stream stream, Encoding encoding, out int lenByte)
    {
        var b1 = stream.ReadByte();
        var b2 = stream.ReadByte();

        var lenCode = b2 % 0x10 * 2 + b1 / 0x80;
        var lenWord = b1 % 0x80 - 1;
        lenByte = 4 + lenWord + lenCode * 2;

        var rankLow = stream.ReadByte();
        var rankHigh = stream.ReadByte();
        var rank = rankLow + (rankHigh << 8);

        // Parse pinyin
        var pinyinList = new string[lenCode];
        var valid = true;
        for (var i = 0; i < lenCode; i++)
        {
            var smB = stream.ReadByte();
            var ymB = stream.ReadByte();
            var smIndex = smB & 31;
            var ymIndex = (smB >> 5) + (ymB << 3);

            if (smIndex < Shengmu.Length && ymIndex < Yunmu.Length)
                pinyinList[i] = Shengmu[smIndex] + Yunmu[ymIndex];
            else
            {
                valid = false;
                pinyinList[i] = "";
            }
        }

        // Parse word
        var hzBytes = new byte[lenWord];
        stream.ReadExactly(hzBytes, 0, lenWord);
        var word = encoding.GetString(hzBytes);

        if (!valid || word.Length == 0)
            return null;

        return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinList)
        };
    }
}
