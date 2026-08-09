namespace ImeWlConverter.Formats.SougouBin;

using System.Buffers.Binary;
using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// Writes Sougou Pinyin SGPU backup dictionaries.
/// </summary>
[FormatPlugin("sgpybin", "搜狗拼音备份词库bin", 30, IsBinary = true, FileExtension = ".bin")]
public sealed partial class SougouBinExporter : IFormatExporter
{
    private const int HeaderSize = 0x8C;
    private const int DefaultIndexCapacity = 10_000;
    private const int DefaultDictionaryCapacity = 400_000;
    private const int LargeDictionaryCapacityUnit = 10_000;
    private const int LargeDictionaryCapacityReserve = 90_000;
    private const uint LargeDictionaryMarkerStride = 0x000B4AA0;

    public async Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries,
        Stream output,
        ExportOptions? options = null,
        CancellationToken ct = default)
    {
        var pendingRecords = new List<PendingRecord>(entries.Count);
        var pinyinToIndex = BuildPinyinIndex();

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();

            if (TryCreateRecord(entry, pinyinToIndex, out var record))
                pendingRecords.Add(record);
        }

        var isLargeDictionary = pendingRecords.Count > DefaultIndexCapacity;
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var records = new List<SougouRecord>(pendingRecords.Count);
        for (var i = 0; i < pendingRecords.Count; i++)
        {
            var pending = pendingRecords[i];
            var key = pending.Word + "\0" + string.Join(',', pending.PinyinIndices);
            var isDuplicate = !seenKeys.Add(key);
            var id = isLargeDictionary ? (ushort)0 : checked((ushort)(i + 2));
            records.Add(BuildRecord(pending, id, isDuplicate));
        }

        var dictionaryUsed = records.Sum(record => record.Data.Length);
        var indexCapacity = RoundUp(Math.Max(DefaultIndexCapacity, records.Count), DefaultIndexCapacity);
        var dictionaryCapacity = isLargeDictionary
            ? checked(RoundUp(dictionaryUsed, LargeDictionaryCapacityUnit) + LargeDictionaryCapacityReserve)
            : RoundUp(
                Math.Max(DefaultDictionaryCapacity, dictionaryUsed),
                DefaultDictionaryCapacity);
        var dictionaryBegin = checked(HeaderSize + indexCapacity * sizeof(uint));
        var data = new byte[checked(dictionaryBegin + dictionaryCapacity)];

        WriteHeader(
            data,
            records,
            indexCapacity,
            dictionaryBegin,
            dictionaryCapacity,
            dictionaryUsed,
            isLargeDictionary);

        var dictionaryOffset = 0;
        foreach (var record in records)
        {
            record.DictionaryOffset = dictionaryOffset;
            record.Data.CopyTo(data, dictionaryBegin + dictionaryOffset);
            dictionaryOffset += record.Data.Length;
        }

        var logicalIndex = records
            .OrderBy(record => record, SougouRecordComparer.Instance)
            .ToList();
        for (var i = 0; i < logicalIndex.Count; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(
                data.AsSpan(HeaderSize + i * sizeof(uint), sizeof(uint)),
                checked((uint)logicalIndex[i].DictionaryOffset));
        }

        await output.WriteAsync(data, ct);
        await output.FlushAsync(ct);

        return new ExportResult
        {
            EntryCount = records.Count,
            ErrorCount = entries.Count - records.Count
        };
    }

    private static Dictionary<string, ushort> BuildPinyinIndex()
    {
        var result = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < SougouBinParser.PinyinData.Length; i++)
            result.TryAdd(SougouBinParser.PinyinData[i], checked((ushort)i));

        return result;
    }

    private static bool TryCreateRecord(
        WordEntry entry,
        IReadOnlyDictionary<string, ushort> pinyinToIndex,
        out PendingRecord record)
    {
        record = default!;
        if (string.IsNullOrWhiteSpace(entry.Word) || entry.Code?.Segments.Count is not > 0)
            return false;

        var pinyinIndices = new ushort[entry.Code.Segments.Count];
        for (var i = 0; i < entry.Code.Segments.Count; i++)
        {
            var segment = entry.Code.Segments[i];
            if (segment.Count == 0 || !pinyinToIndex.TryGetValue(NormalizePinyin(segment[0]), out pinyinIndices[i]))
                return false;
        }

        var wordBytes = Encoding.Unicode.GetBytes(entry.Word);
        var pinyinByteCount = checked(pinyinIndices.Length * sizeof(ushort));
        if (wordBytes.Length > ushort.MaxValue || pinyinByteCount > ushort.MaxValue)
            return false;

        record = new PendingRecord(
            entry.Word,
            pinyinIndices,
            wordBytes,
            Math.Clamp(entry.Rank, 0, ushort.MaxValue));
        return true;
    }

    private static SougouRecord BuildRecord(PendingRecord pending, ushort id, bool isDuplicate)
    {
        var pinyinByteCount = checked(pending.PinyinIndices.Length * sizeof(ushort));
        var trailingData = isDuplicate
            ? BuildPinyinTail(pending.PinyinIndices)
            : new byte[] { 0x02, 0x00, 0xA0, 0x03 };
        var data = new byte[checked(15 + pinyinByteCount + pending.WordBytes.Length + trailingData.Length)];

        BinaryPrimitives.WriteUInt16LittleEndian(data, checked((ushort)pending.Rank));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), id);
        data[4] = 0;
        data[5] = 0;
        data[6] = 1;
        data[7] = 0;
        data[8] = checked((byte)Math.Min(pending.PinyinIndices.Length, byte.MaxValue));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(9), checked((ushort)pinyinByteCount));

        for (var i = 0; i < pending.PinyinIndices.Length; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(11 + i * sizeof(ushort)), pending.PinyinIndices[i]);

        var wordInfoOffset = 11 + pinyinByteCount;
        BinaryPrimitives.WriteUInt16LittleEndian(
            data.AsSpan(wordInfoOffset),
            checked((ushort)(sizeof(ushort) + pending.WordBytes.Length + trailingData.Length)));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(wordInfoOffset + 2), checked((ushort)pending.WordBytes.Length));
        pending.WordBytes.CopyTo(data, wordInfoOffset + 4);
        trailingData.CopyTo(data, wordInfoOffset + 4 + pending.WordBytes.Length);

        return new SougouRecord(pending.Word, pending.PinyinIndices, id, data);
    }

    private static byte[] BuildPinyinTail(IReadOnlyList<ushort> pinyinIndices)
    {
        var data = new byte[checked(sizeof(ushort) + pinyinIndices.Count * sizeof(ushort))];
        BinaryPrimitives.WriteUInt16LittleEndian(data, checked((ushort)(pinyinIndices.Count * sizeof(ushort))));
        for (var i = 0; i < pinyinIndices.Count; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(sizeof(ushort) + i * sizeof(ushort)), pinyinIndices[i]);
        return data;
    }

    private static string NormalizePinyin(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .TrimEnd('0', '1', '2', '3', '4', '5')
            .Replace("u:", "v", StringComparison.Ordinal)
            .Replace('ü', 'v');
    }

    private static void WriteHeader(
        byte[] data,
        IReadOnlyList<SougouRecord> records,
        int indexCapacity,
        int dictionaryBegin,
        int dictionaryCapacity,
        int dictionaryUsed,
        bool isLargeDictionary)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(data, 0x55504753); // SGPU
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x04), 0x28);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x08), 0x0133A009);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10), checked((uint)data.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x14), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x18), 16);
        // Sogou validates this value when importing. Each index block after the
        // first 10,000 records adds the allocator stride used by Sogou itself.
        var zeroRankCount = records.Count(record => record.Rank == 0);
        var additionalBlocks = isLargeDictionary
            ? Math.Max(records.Count - 1, 0) / DefaultIndexCapacity
            : 0;
        var marker = checked(
            0x5691F359u
            + (uint)dictionaryUsed
            + (uint)Math.Max(records.Count - 1, 0)
            + (uint)zeroRankCount
            + checked((uint)additionalBlocks * LargeDictionaryMarkerStride));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x20), marker);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x24), 84);
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(0x28),
            isLargeDictionary ? 1u : checked((uint)(records.Count + 1)));
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(0x30),
            isLargeDictionary ? 0u : checked((uint)records.Count(record => record.Rank > 0)));
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(0x34),
            isLargeDictionary
                ? 0u
                : checked((uint)records.Where(record => record.Rank > 0).Sum(record => (long)record.Id)));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x38), HeaderSize);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x3C), checked((uint)(indexCapacity * sizeof(uint))));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x40), checked((uint)records.Count));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x44), checked((uint)dictionaryBegin));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x48), checked((uint)dictionaryCapacity));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x4C), checked((uint)dictionaryUsed));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x50), 9);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x54), checked((uint)zeroRankCount));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x64), uint.MaxValue);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x68), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x6C), uint.MaxValue);
    }

    private static int RoundUp(int value, int unit)
    {
        return checked(((value + unit - 1) / unit) * unit);
    }

    private sealed record PendingRecord(string Word, ushort[] PinyinIndices, byte[] WordBytes, int Rank);

    private sealed class SougouRecord(string word, ushort[] pinyinIndices, ushort id, byte[] data)
    {
        public string Word { get; } = word;
        public ushort[] PinyinIndices { get; } = pinyinIndices;
        public ushort Id { get; } = id;
        public int Rank => BinaryPrimitives.ReadUInt16LittleEndian(Data);
        public byte[] Data { get; } = data;
        public int DictionaryOffset { get; set; }
    }

    private sealed class SougouRecordComparer : IComparer<SougouRecord>
    {
        public static SougouRecordComparer Instance { get; } = new();

        public int Compare(SougouRecord? x, SougouRecord? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var length = Math.Min(x.PinyinIndices.Length, y.PinyinIndices.Length);
            for (var i = 0; i < length; i++)
            {
                var result = x.PinyinIndices[i].CompareTo(y.PinyinIndices[i]);
                if (result != 0) return result;
            }

            var countResult = x.PinyinIndices.Length.CompareTo(y.PinyinIndices.Length);
            if (countResult != 0) return countResult;

            var wordResult = StringComparer.Ordinal.Compare(x.Word, y.Word);
            return wordResult != 0 ? wordResult : y.Id.CompareTo(x.Id);
        }
    }
}
