namespace ImeWlConverter.Formats.BaiduPinyinBackup;

using System.Text;
using System.Text.RegularExpressions;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Baidu Pinyin backup file importer (custom encoded binary format).</summary>
[FormatPlugin("bdpybin", "百度拼音备份词库bin", 20)]
public sealed partial class BaiduPinyinBackupImporter : BinaryFormatImporter
{
    private const uint Mask = 0x2D382324;

    private static readonly byte[] Table = Encoding.ASCII.GetBytes(
        "qogjOuCRNkfil5p4SQ3LAmxGKZTdesvB6z_YPahMI9t80rJyHW1DEwFbc7nUVX2-"
    );

    private static readonly byte[] DecodeTable = BuildDecodeTable();

    private static byte[] BuildDecodeTable()
    {
        var table = new byte[256];
        for (var i = 0; i < Table.Length; i++)
            table[Table[i]] = (byte)i;
        return table;
    }

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        var entries = new List<WordEntry>();

        // Skip BOM (FF FE)
        input.Seek(2, SeekOrigin.Begin);

        var cnFlag = false;
        while (input.Position < input.Length - 4)
        {
            ct.ThrowIfCancellationRequested();

            // Read one line (UTF-16LE, 2 bytes at a time until newline 0x0A 0x00)
            var lineBytes = new List<byte>();
            var bytes = new byte[2];
            while (true)
            {
                var read = input.Read(bytes, 0, 2);
                if (read < 2) break;
                if (bytes[0] == 0x0A && bytes[1] == 0x00)
                    break;
                lineBytes.Add(bytes[0]);
                lineBytes.Add(bytes[1]);
            }

            var decoded = Decode(lineBytes);
            var line = Encoding.Unicode.GetString(decoded);

            // Stop at <enword> or <sysusrword> sections
            if (cnFlag && (line == "<enword>" || line == "<sysusrword>"))
                break;
            if (line == "<cnword>")
            {
                cnFlag = true;
                continue;
            }

            if (!cnFlag)
                continue;

            // Line format: 百度输入法(bai|du|shu|ru|fa) 2 24 1703756731 N N
            var array = line.Split(' ');
            if (array.Length < 2)
                continue;

            if (!int.TryParse(array[1], out var rank))
                continue;

            // Regex to separate word and pinyin: word(pinyin)
            var match = Regex.Match(array[0], @"([^\(]+)\((.+)\)");
            if (match.Groups.Count != 3)
                continue;

            var word = match.Groups[1].Value;
            var py = match.Groups[2].Value;
            var pinyin = py.Split('|');

            entries.Add(new WordEntry
            {
                Word = word,
                Rank = rank,
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(pinyin)
            });
        }

        return entries;
    }

    private static byte[] Decode(List<byte> data)
    {
        if (data.Count % 4 != 2)
            return [];

        var base64Remainder = (byte)(data[data.Count - 2] - 65);
        if (base64Remainder > 2 || data[data.Count - 1] != 0)
            return [];

        // Map custom base64 encoding table
        for (var i = 0; i < data.Count - 2; i++)
            data[i] = DecodeTable[data[i]];

        // Convert every 4 bytes to 3 bytes
        var transformed = new List<byte>(data.Count / 4 * 3);
        for (var i = 0; i < data.Count - 2; i += 4)
        {
            var highBits = data[i + 3];
            transformed.Add((byte)(data[i] | ((highBits & 0b110000) << 2)));
            transformed.Add((byte)(data[i + 1] | ((highBits & 0b1100) << 4)));
            transformed.Add((byte)(data[i + 2] | ((highBits & 0b11) << 6)));
        }

        if (base64Remainder > 0)
        {
            for (var i = 0; i < 3 - base64Remainder; i++)
            {
                if (transformed.Count == 0 || transformed[transformed.Count - 1] != 0)
                    return [];
                transformed.RemoveAt(transformed.Count - 1);
            }
        }

        var result = transformed.ToArray();
        for (var i = 0; i < result.Length; i += 4)
        {
            uint chunk;
            if (i + 4 > result.Length)
            {
                var chunkBytes = new byte[4];
                Array.Copy(result, i, chunkBytes, 0, result.Length - i);
                chunk = Mask ^ BitConverter.ToUInt32(chunkBytes, 0);
                Buffer.BlockCopy(BitConverter.GetBytes(chunk), 0, result, i, result.Length - i);
                break;
            }

            chunk = Mask ^ BitConverter.ToUInt32(result, i);
            chunk = ((chunk & 0x1FFFFFFF) << 3) | (chunk >> 29);
            Buffer.BlockCopy(BitConverter.GetBytes(chunk), 0, result, i, 4);
        }

        return result;
    }
}
