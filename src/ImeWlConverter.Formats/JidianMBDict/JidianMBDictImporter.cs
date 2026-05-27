namespace ImeWlConverter.Formats.JidianMBDict;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Jidian MB dictionary importer (binary .mb format, header "Freeime Dictionary").</summary>
[FormatPlugin("jdmb", "极点五笔.mb文件", 190)]
public sealed partial class JidianMBDictImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        // The format requires seeking, buffer into MemoryStream
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        ms.Position = 0;

        var entries = new List<WordEntry>();

        // Verify header "Freeime Dictionary"
        var headerStr = "Freeime Dictionary";
        var headerBytes = new byte[headerStr.Length];
        ms.ReadExactly(headerBytes, 0, headerStr.Length);
        // Header check (non-fatal, continue anyway)

        // Determine code type from header at offset 0x23
        ms.Position = 0x23;
        var headerTypeBytes = new byte[4];
        ms.ReadExactly(headerTypeBytes, 0, 4);
        var headerTypeStr = Encoding.Unicode.GetString(headerTypeBytes);

        CodeType codeType;
        if (headerTypeStr == "拼音")
            codeType = CodeType.Pinyin;
        else if (headerTypeStr == "五笔")
            codeType = CodeType.Wubi98;
        else
            codeType = CodeType.UserDefine;

        // Phrases start at fixed offset 0x1B620
        const int phraseStart = 0x1B620;
        ms.Position = phraseStart;

        while (ms.Position < ms.Length)
        {
            ct.ThrowIfCancellationRequested();

            var entry = ReadOnePhrase(ms, codeType);
            if (entry != null)
                entries.Add(entry);
        }

        return entries;
    }

    private static WordEntry? ReadOnePhrase(Stream fs, CodeType codeType)
    {
        var codeBytesLen = fs.ReadByte();
        if (codeBytesLen < 0) return null; // EOF

        var wordBytesLen = fs.ReadByte();
        if (wordBytesLen < 0) return null;

        var split = fs.ReadByte();
        if (split < 0) return null;

        var codeBytes = new byte[codeBytesLen];
        fs.ReadExactly(codeBytes, 0, codeBytesLen);
        var codeStr = Encoding.ASCII.GetString(codeBytes);

        var wordBytes = new byte[wordBytesLen];
        fs.ReadExactly(wordBytes, 0, wordBytesLen);
        var word = Encoding.Unicode.GetString(wordBytes);

        // 0x32 means simplified/traditional pair like "醃(腌)", take first char only
        if (split == 0x32)
            word = word.Substring(0, 1);

        WordCode? code = null;
        var entryCodeType = codeType;

        if (codeType == CodeType.Pinyin)
        {
            // Pinyin code is stored as concatenated syllables, e.g. "nihao"
            // Split by treating it as a single code segment
            code = WordCode.FromSingle(new[] { codeStr });
        }
        else if (codeType == CodeType.Wubi98)
        {
            code = WordCode.FromSingle(new[] { codeStr });
        }
        else
        {
            entryCodeType = CodeType.UserDefine;
            code = WordCode.FromSingle(new[] { codeStr });
        }

        return new WordEntry
        {
            Word = word,
            Rank = 0,
            CodeType = entryCodeType,
            Code = code
        };
    }
}
