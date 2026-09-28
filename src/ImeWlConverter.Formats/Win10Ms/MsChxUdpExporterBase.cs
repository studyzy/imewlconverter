namespace ImeWlConverter.Formats.Win10Ms;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// mschxudp（微软拼音/五笔用户自定义短语）导出公共实现。
/// 文件结构：0x40 字节头部 + uint32 相对偏移表 + 记录区。
/// 记录：魔数 10 00 10 00 | uint16 词区偏移 | uint8 候选序号 | 0x06 |
/// 4 字节 0 | uint32 2000-01-01 纪元秒 | 编码 UTF-16LE + NUL | 词 UTF-16LE + NUL。
/// </summary>
public abstract class MsChxUdpExporterBase : IFormatExporter
{
    /// <summary>微软用户词库上限约 2 万条，超出部分无法导入。</summary>
    public const int MaxEntries = 20000;

    /// <summary>候选序号只有 1 字节，同一编码最多 255 条候选。</summary>
    private const int MaxCandidatePosition = 255;

    /// <summary>2000-01-01 00:00:00 UTC 的 Unix 秒。</summary>
    private const long Epoch2000UnixSeconds = 946684800L;

    private const int MaxCodeLength = 32;
    private const int MaxWordLength = 64;
    private const int RecordHeaderSize = 16;

    public abstract FormatMetadata Metadata { get; }

    /// <summary>取词条的编码字段（拼音含 ' 分隔 / 五笔全码）。</summary>
    protected abstract string GetCode(WordEntry entry);

    /// <summary>编码字符是否合法（拼音：a-z 与 '；五笔：a-z）。</summary>
    protected abstract bool IsValidCodeChar(char c);

    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        var errorCount = 0;

        // 过滤：词长、空编码、编码字符合法性；按 (编码, 词) 去重（搜狗等词库常见重复）
        var seen = new HashSet<(string Code, string Word)>();
        var valid = new List<(WordEntry Entry, string Code)>(entries.Count);
        foreach (var entry in entries)
        {
            var code = GetCode(entry);
            if (entry.Word.Length == 0 || entry.Word.Length > MaxWordLength
                || code.Length == 0 || code.Length > MaxCodeLength
                || !AllCharsValid(code))
            {
                errorCount++;
                continue;
            }

            if (!seen.Add((code, entry.Word)))
            {
                errorCount++; // 重复词条，微软拼音会显示重复候选
                continue;
            }

            valid.Add((entry, code));
        }

        // 2 万条上限：超出部分无法导入，截断
        if (valid.Count > MaxEntries)
        {
            errorCount += valid.Count - MaxEntries;
            valid.RemoveRange(MaxEntries, valid.Count - MaxEntries);
        }

        // 同编码候选序号（1 起）：超出一字节表示范围的词条跳过
        var counters = new Dictionary<string, int>(valid.Count);
        var accepted = new List<(WordEntry Entry, string Code, byte Position)>(valid.Count);
        foreach (var (entry, code) in valid)
        {
            counters.TryGetValue(code, out var pos);
            pos++;
            counters[code] = pos;
            if (pos > MaxCandidatePosition)
            {
                errorCount++;
                continue;
            }

            accepted.Add((entry, code, (byte)pos));
        }

        using var bw = new BinaryWriter(output, Encoding.Unicode, leaveOpen: true);

        // Header (0x40 字节)
        bw.Write(Encoding.ASCII.GetBytes("mschxudp")); // proto8
        bw.Write(0x00600002); // Unknown
        bw.Write(1); // version
        bw.Write(0x40); // offset table start
        bw.Write(0x40 + 4 * accepted.Count); // record data start
        bw.Write(0); // file size (filled later)
        bw.Write(accepted.Count); // phrase_count
        bw.Write((uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // 0x20 Unix timestamp
        bw.Write(0L); // 0x24-0x3F 必须为 0（28 字节）
        bw.Write(0L);
        bw.Write(0L);
        bw.Write((int)0);

        // 偏移表（相对记录区起始）
        var offsets = new int[accepted.Count];
        var offset = 0;
        for (var i = 0; i < accepted.Count; i++)
        {
            offsets[i] = offset;
            offset += RecordHeaderSize + accepted[i].Code.Length * 2 + 2
                      + accepted[i].Entry.Word.Length * 2 + 2;
        }

        foreach (var off in offsets)
            bw.Write(off);

        // 记录区
        var recordTimestamp = (uint)Math.Max(0, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Epoch2000UnixSeconds);
        var count = 0;
        foreach (var (entry, code, position) in accepted)
        {
            ct.ThrowIfCancellationRequested();
            WriteRecord(bw, code, entry.Word, position, recordTimestamp);
            count++;
        }

        // 回填文件大小
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

    private static void WriteRecord(
        BinaryWriter bw, string code, string word, byte position, uint recordTimestamp)
    {
        bw.Write(0x00100010); // record magic
        bw.Write((short)(RecordHeaderSize + code.Length * 2 + 2)); // 词区偏移
        bw.Write(position); // 同编码候选序号（1 起）
        bw.Write((byte)0x06); // unknown
        bw.Write(0x00000000); // unknown
        bw.Write(recordTimestamp); // 2000-01-01 纪元秒

        bw.Write(Encoding.Unicode.GetBytes(code));
        bw.Write((short)0); // NUL 分隔
        bw.Write(Encoding.Unicode.GetBytes(word));
        bw.Write((short)0); // NUL 结尾
    }

    private bool AllCharsValid(string code)
    {
        foreach (var c in code)
        {
            if (!IsValidCodeChar(c))
                return false;
        }

        return true;
    }
}
