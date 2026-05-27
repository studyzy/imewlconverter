namespace ImeWlConverter.Formats.Win10Ms;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Win10 Microsoft Pinyin user dictionary importer (binary format).</summary>
[FormatPlugin("win10mspy", "Win10微软拼音（用户自定义短语）", 130)]
public sealed partial class Win10MsPinyinImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        var results = new List<WordEntry>();
        using var reader = new BinaryReader(input, Encoding.Unicode, leaveOpen: true);

        // Header: offset 0x10 has phrase_offset_start, phrase_start, phrase_end, phrase_count
        input.Position = 0x10;
        var phraseOffsetStart = reader.ReadInt32();
        var phraseStart = reader.ReadInt32();
        var phraseEnd = reader.ReadInt32();
        var phraseCount = reader.ReadInt32();

        // Read offsets
        input.Position = phraseOffsetStart;
        var offsets = new List<int>(phraseCount + 1);
        for (var i = 0; i < phraseCount; i++)
            offsets.Add(reader.ReadInt32());
        offsets.Add(phraseEnd - phraseStart);

        // Read phrases
        input.Position = phraseStart;
        for (var i = 0; i < phraseCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var nextStartPosition = phraseStart + offsets[i + 1];
            var entry = ReadOnePhrase(reader, input, nextStartPosition);
            if (entry != null)
                results.Add(entry);
        }

        return results;
    }

    private static WordEntry? ReadOnePhrase(BinaryReader reader, Stream stream, int nextStartPosition)
    {
        // magic (4 bytes)
        reader.ReadInt32();
        // hanzi_offset (2 bytes) - offset from magic to where hanzi begins
        var hanziOffset = reader.ReadInt16();
        // rank (1 byte)
        var rank = stream.ReadByte();
        // unknown byte
        stream.ReadByte();
        // unknown 8 bytes (added in 1703)
        reader.ReadInt64();

        // Pinyin bytes: hanzi_offset - 18 (magic:4 + hanziOffset:2 + rank:1 + x6:1 + unknown8:8 + pySplit:2 = 18)
        var pyBytesLen = hanziOffset - 18;
        if (pyBytesLen <= 0)
            return null;
        var pyBytes = reader.ReadBytes(pyBytesLen);
        var pyStr = Encoding.Unicode.GetString(pyBytes);

        // 00 00 split between pinyin and word
        reader.ReadInt16();

        // Word bytes
        var wordBytesLen = nextStartPosition - (int)stream.Position - 2;
        if (wordBytesLen <= 0)
            return null;
        var wordBytes = reader.ReadBytes(wordBytesLen);
        // 00 00 trailing separator
        reader.ReadInt16();

        var word = Encoding.Unicode.GetString(wordBytes);

        WordCode? code = null;
        if (!string.IsNullOrEmpty(pyStr))
        {
            var pinyinParts = pyStr.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
            if (pinyinParts.Length == 0)
                pinyinParts = new[] { pyStr };
            code = WordCode.FromSingle(pinyinParts);
        }

        return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = code
        };
    }
}
