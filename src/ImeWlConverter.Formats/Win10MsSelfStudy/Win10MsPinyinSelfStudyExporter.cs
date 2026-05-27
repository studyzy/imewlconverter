namespace ImeWlConverter.Formats.Win10MsSelfStudy;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Win10 Microsoft Pinyin self-study dictionary exporter (binary DAT format).</summary>
[FormatPlugin("win10mspyss", "Win10微软拼音（自学习词汇）", 130, FileExtension = ".dat")]
public sealed partial class Win10MsPinyinSelfStudyExporter : IFormatExporter
{
    private const int UserWordBase = 0x2400;
    private const int EntrySize = 60;

    // Header: "55AA8881 02006000 55AA55AA"
    private static readonly byte[] HeaderMagic =
    {
        0x55, 0xAA, 0x88, 0x81,
        0x02, 0x00, 0x60, 0x00,
        0x55, 0xAA, 0x55, 0xAA
    };

    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        // Filter: word length 2-12
        var filtered = entries.Where(e => e.Word.Length is >= 2 and <= 12).ToList();

        using var bw = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);

        // Header (12 bytes)
        bw.Write(HeaderMagic);

        // Word count (8 bytes: int64)
        bw.Write((long)filtered.Count);

        // Timestamp (4 bytes)
        bw.Write((int)DateTime.Now.Ticks);

        // Padding to 0x2400
        var headerWritten = 12 + 8 + 4; // 24 bytes
        var padding = UserWordBase - headerWritten;
        for (var i = 0; i < padding; i++)
            bw.Write((byte)0);

        // Write entries (each 60 bytes)
        var count = 0;
        var errorCount = 0;
        for (var i = 0; i < filtered.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var entry = filtered[i];
            try
            {
                WriteEntry(bw, entry, i);
                count++;
            }
            catch
            {
                // Write empty entry on error
                var pos = output.Position;
                var remaining = EntrySize - (int)(output.Position - (UserWordBase + i * EntrySize));
                if (remaining > 0)
                    for (var j = 0; j < remaining; j++)
                        bw.Write((byte)0);
                errorCount++;
            }
        }

        // Pad to next 1KB boundary
        var totalSize = (int)output.Position;
        var alignedSize = (int)Math.Ceiling(totalSize / 1024.0) * 1024;
        while (output.Position < alignedSize)
            bw.Write((byte)0);

        return Task.FromResult(new ExportResult
        {
            EntryCount = count,
            ErrorCount = errorCount
        });
    }

    private static void WriteEntry(BinaryWriter bw, WordEntry entry, int index)
    {
        var word = entry.Word;

        // Bytes 0-1: index + 0x6D1B
        bw.Write((short)(index + 0x6D1B));
        // Bytes 2-3: 0x261A
        bw.Write((byte)0x1A);
        bw.Write((byte)0x26);
        // Bytes 4-6: zeros
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        // Bytes 7-9: 0x000400
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0x04);
        // Byte 10: word length
        bw.Write((byte)word.Length);
        // Byte 11: 0x5A
        bw.Write((byte)0x5A);

        // Bytes 12+: word in Unicode
        bw.Write(Encoding.Unicode.GetBytes(word));

        // Pinyin indices (2 bytes each)
        var pinyins = entry.Code?.GetPrimaryCode(" ").Split(' ') ?? Array.Empty<string>();
        for (var j = 0; j < word.Length; j++)
        {
            if (j < pinyins.Length)
            {
                var pyIndex = GetPinyinIndex(pinyins[j]);
                bw.Write(pyIndex);
            }
            else
            {
                bw.Write((short)0);
            }
        }

        // Pad to 60 bytes
        var used = 12 + word.Length * 2 + word.Length * 2;
        for (var j = used; j < EntrySize; j++)
            bw.Write((byte)0);
    }

    private static short GetPinyinIndex(string pinyin)
    {
        _pinyinMapInit ??= BuildPinyinMap();
        return _pinyinMapInit.TryGetValue(pinyin.ToLowerInvariant(), out var index) ? index : (short)0;
    }

    private static Dictionary<string, short>? _pinyinMapInit;

    private static Dictionary<string, short> BuildPinyinMap()
    {
        var map = new Dictionary<string, short>();
        var table = Win10MsPinyinSelfStudyImporter.PinyinTable;
        for (short i = 0; i < table.Length; i++)
            map[table[i]] = i;
        return map;
    }
}
