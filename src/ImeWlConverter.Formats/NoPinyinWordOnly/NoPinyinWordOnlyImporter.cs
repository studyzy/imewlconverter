namespace ImeWlConverter.Formats.NoPinyinWordOnly;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Plain word list importer (no pinyin, one word per line). Auto-detects encoding.</summary>
[FormatPlugin("word", "无拼音纯汉字", 2010)]
public sealed partial class NoPinyinWordOnlyImporter : IFormatImporter
{
    public Task<ImportResult> ImportAsync(Stream input, ImportOptions? options = null, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        var bytes = ms.ToArray();

        var encoding = DetectEncoding(bytes, out var bomOffset);
        var text = encoding.GetString(bytes, bomOffset, bytes.Length - bomOffset);
        var entries = new List<WordEntry>();

        foreach (var line in text.Split('\n'))
        {
            ct.ThrowIfCancellationRequested();
            var trimmed = line.Trim('\r', ' ', '\t');
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            entries.Add(new WordEntry
            {
                Word = trimmed,
                Rank = 0,
                CodeType = CodeType.Pinyin,
                Code = null
            });
        }

        return Task.FromResult(new ImportResult
        {
            Entries = entries,
            ErrorCount = 0
        });
    }

    private static Encoding DetectEncoding(byte[] bytes, out int bomOffset)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        bomOffset = 0;

        // Check BOM first
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            bomOffset = 3;
            return Encoding.UTF8;
        }
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            bomOffset = 2;
            return Encoding.Unicode;
        }
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            bomOffset = 2;
            return Encoding.BigEndianUnicode;
        }

        // Try UTF-8 first: if all bytes form valid UTF-8 (with multi-byte sequences), use it
        if (IsValidUtf8(bytes))
            return new UTF8Encoding(false);

        // Not valid UTF-8, assume GBK/GB18030 (this is a Chinese IME tool)
        return Encoding.GetEncoding("GB18030");
    }

    private static bool IsValidUtf8(byte[] bytes)
    {
        var i = 0;
        while (i < bytes.Length)
        {
            var b = bytes[i];
            int continuationBytes;

            if (b <= 0x7F) { i++; continue; }
            else if ((b & 0xE0) == 0xC0) continuationBytes = 1;
            else if ((b & 0xF0) == 0xE0) continuationBytes = 2;
            else if ((b & 0xF8) == 0xF0) continuationBytes = 3;
            else return false;

            if (i + continuationBytes >= bytes.Length) return false;
            for (var j = 1; j <= continuationBytes; j++)
            {
                if ((bytes[i + j] & 0xC0) != 0x80) return false;
            }
            i += 1 + continuationBytes;
        }
        return true;
    }
}
