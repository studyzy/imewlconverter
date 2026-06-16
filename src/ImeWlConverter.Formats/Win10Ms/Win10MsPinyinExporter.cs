namespace ImeWlConverter.Formats.Win10Ms;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Win10 Microsoft Pinyin user dictionary exporter (binary format).</summary>
[FormatPlugin("win10mspy", "Win10微软拼音（用户自定义短语）", 130, FileExtension = ".dat")]
public sealed partial class Win10MsPinyinExporter : IFormatExporter
{
    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        // Filter: pinyin length <= 32 chars, word length <= 64 chars
        var filtered = entries
            .Where(e => e.Word.Length <= 64)
            .Where(e =>
            {
                var pyLen = e.Code?.GetPrimaryCode("'").Length ?? 0;
                return pyLen <= 32;
            })
            .ToList();

        var errorCount = entries.Count - filtered.Count;

        using var bw = new BinaryWriter(output, Encoding.Unicode, leaveOpen: true);

        // Header
        bw.Write(Encoding.ASCII.GetBytes("mschxudp")); // proto8
        bw.Write(0x00600002); // Unknown
        bw.Write(1); // version
        bw.Write(0x40); // phrase_offset_start
        bw.Write(0x40 + 4 * filtered.Count); // phrase_start
        bw.Write(0); // phrase_end (filled later)
        bw.Write(filtered.Count); // phrase_count
        bw.Write(DateTime.Now.Ticks); // timestamp
        bw.Write(0L); // padding
        bw.Write(0L); // padding
        bw.Write(0L); // padding

        // Write offsets
        var offset = 0;
        var offsets = new int[filtered.Count];
        for (var i = 0; i < filtered.Count; i++)
        {
            offsets[i] = offset;
            var entry = filtered[i];
            var pyStr = entry.Code?.GetPrimaryCode("'") ?? "";
            offset += 8 + 8 + pyStr.Length * 2 + 2 + entry.Word.Length * 2 + 2;
        }

        foreach (var off in offsets)
            bw.Write(off);

        // Write phrases
        var count = 0;
        for (var i = 0; i < filtered.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var entry = filtered[i];
            try
            {
                WriteEntry(bw, entry);
                count++;
            }
            catch
            {
                errorCount++;
            }
        }

        // Go back and fill phrase_end
        var endPos = (int)output.Position;
        output.Position = 0x18;
        bw.Write(endPos);
        output.Position = endPos;

        return Task.FromResult(new ExportResult
        {
            EntryCount = count,
            ErrorCount = errorCount
        });
    }

    private static void WriteEntry(BinaryWriter bw, WordEntry entry)
    {
        var pyStr = entry.Code?.GetPrimaryCode("'") ?? "";
        var hanziOffset = 8 + 8 + pyStr.Length * 2 + 2;

        bw.Write(0x00100010); // magic
        bw.Write((short)hanziOffset);
        bw.Write((byte)entry.Rank); // rank
        bw.Write((byte)0x06); // unknown
        bw.Write(0x00000000); // unknown
        bw.Write(0xE679CD20); // unknown

        // Pinyin in Unicode with ' separator (matches Win10 format)
        bw.Write(Encoding.Unicode.GetBytes(pyStr));
        // 00 00 separator
        bw.Write((short)0);
        // Word in Unicode
        bw.Write(Encoding.Unicode.GetBytes(entry.Word));
        // 00 00 trailing separator
        bw.Write((short)0);
    }
}
