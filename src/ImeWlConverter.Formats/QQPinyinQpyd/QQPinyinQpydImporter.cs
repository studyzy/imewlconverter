namespace ImeWlConverter.Formats.QQPinyinQpyd;

using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>QQ Pinyin qpyd dictionary importer (binary format with zlib-compressed word data).</summary>
[FormatPlugin("qpyd", "QQ分类词库qpyd", 60)]
public sealed partial class QQPinyinQpydImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        // qpyd format requires seeking, so buffer into a MemoryStream
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        ms.Position = 0;

        var entries = new List<WordEntry>();

        // Read start address of compressed data at offset 0x38
        ms.Position = 0x38;
        var startAddressByte = new byte[4];
        ms.ReadExactly(startAddressByte, 0, 4);
        var startAddress = BitConverter.ToInt32(startAddressByte, 0);

        // Seek to compressed data and decompress with zlib (Inflater)
        ms.Position = startAddress;
        var zipStream = new InflaterInputStream(ms);

        var bufferSize = 2048;
        var buffer = new byte[bufferSize];
        var byteList = new List<byte>();
        int readCount;
        while ((readCount = zipStream.Read(buffer, 0, bufferSize)) > 0)
        {
            for (var i = 0; i < readCount; i++)
                byteList.Add(buffer[i]);
        }

        zipStream.Close();

        var byteArray = byteList.ToArray();

        // Parse the unzipped data: index entries followed by pinyin+word data
        var unzippedDictStartAddr = -1;
        var idx = 0;
        while (unzippedDictStartAddr == -1 || idx < unzippedDictStartAddr)
        {
            ct.ThrowIfCancellationRequested();

            var pinyinStartAddr = BitConverter.ToInt32(byteArray, idx + 0x6);
            var pinyinLength = byteArray[idx + 0x0] & 0xff;
            var wordStartAddr = pinyinStartAddr + pinyinLength;
            var wordLength = byteArray[idx + 0x1] & 0xff;

            if (unzippedDictStartAddr == -1)
                unzippedDictStartAddr = pinyinStartAddr;

            var pinyin = Encoding.UTF8.GetString(byteArray, pinyinStartAddr, pinyinLength);
            var word = Encoding.Unicode.GetString(byteArray, wordStartAddr, wordLength);

            if (!string.IsNullOrEmpty(pinyin))
            {
                var pinyinParts = pinyin.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
                entries.Add(new WordEntry
                {
                    Word = word,
                    Rank = 1,
                    CodeType = CodeType.Pinyin,
                    Code = WordCode.FromSingle(pinyinParts)
                });
            }

            // Each index entry is 0x0A bytes
            idx += 0x0A;
        }

        return entries;
    }
}
