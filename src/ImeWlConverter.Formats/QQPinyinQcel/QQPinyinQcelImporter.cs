namespace ImeWlConverter.Formats.QQPinyinQcel;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>QQ Pinyin qcel cell dictionary importer (binary format similar to scel).</summary>
[FormatPlugin("qcel", "QQ分类词库qcel", 60)]
public sealed partial class QQPinyinQcelImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        // Format requires seeking, buffer into MemoryStream
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        ms.Position = 0;

        var entries = new List<WordEntry>();
        var pyDic = new Dictionary<int, string>();

        // Skip first 128 bytes header
        ms.Position = 128;

        // Read word count at offset 0x124
        ms.Position = 0x124;
        var countBytes = new byte[4];
        ms.ReadExactly(countBytes, 0, 4);
        var wordCount = BitConverter.ToInt32(countBytes, 0);

        // Read pinyin table starting at 0x1540
        ms.Position = 0x1540;
        var skipBytes = new byte[4];
        ms.ReadExactly(skipBytes, 0, 4); // skip 4 bytes (0x9D 0x01 0x00 0x00)

        // Build pinyin dictionary
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var num = new byte[4];
            if (ms.Read(num, 0, 4) < 4) break;

            var mark = num[0] + num[1] * 256;
            var pyLen = num[2];
            if (pyLen <= 0) break;

            var pyBytes = new byte[pyLen];
            ms.ReadExactly(pyBytes, 0, pyLen);
            var py = Encoding.Unicode.GetString(pyBytes);

            pyDic[mark] = py;

            if (py == "zuo") // Last pinyin entry
                break;
        }

        // Read word entries
        var currentCount = 0;
        while (ms.Position < ms.Length)
        {
            ct.ThrowIfCancellationRequested();

            var wordEntries = ReadAPinyinWord(ms, pyDic);
            if (wordEntries == null)
                break;

            entries.AddRange(wordEntries);
            currentCount += wordEntries.Count;

            if (currentCount >= wordCount || ms.Position >= ms.Length)
                break;
        }

        return entries;
    }

    private static List<WordEntry>? ReadAPinyinWord(Stream fs, Dictionary<int, string> pyDic)
    {
        var num = new byte[4];
        if (fs.Read(num, 0, 4) < 4) return null;

        var samePYcount = num[0] + num[1] * 256;
        var pinyinLen = num[2] + num[3] * 256;

        if (samePYcount == 0 && pinyinLen == 0) return null;

        // Read pinyin indices
        var pyBytes = new byte[pinyinLen];
        if (fs.Read(pyBytes, 0, pinyinLen) < pinyinLen) return null;

        var wordPY = new List<string>();
        for (var i = 0; i < pinyinLen / 2; i++)
        {
            var key = pyBytes[i * 2] + pyBytes[i * 2 + 1] * 256;
            if (pyDic.TryGetValue(key, out var py))
                wordPY.Add(py);
            else
                wordPY.Add(((char)(key - pyDic.Count + 97)).ToString());
        }

        // Read words with the same pinyin
        var results = new List<WordEntry>();
        for (var s = 0; s < samePYcount; s++)
        {
            var lenBytes = new byte[2];
            if (fs.Read(lenBytes, 0, 2) < 2) break;
            var hzByteCount = lenBytes[0] + lenBytes[1] * 256;

            var wordBytes = new byte[hzByteCount];
            if (fs.Read(wordBytes, 0, hzByteCount) < hzByteCount) break;
            var word = Encoding.Unicode.GetString(wordBytes);

            // Skip 2 bytes (unknown short) + 4 bytes (unknown int)
            var skip1 = new byte[2];
            fs.ReadExactly(skip1, 0, 2);
            var skip2 = new byte[4];
            fs.ReadExactly(skip2, 0, 4);

            results.Add(new WordEntry
            {
                Word = word,
                Rank = 0,
                CodeType = CodeType.Pinyin,
                Code = wordPY.Count > 0 ? WordCode.FromSingle(wordPY) : null
            });

            // Skip trailing 6 bytes per entry
            var trailing = new byte[6];
            fs.ReadExactly(trailing, 0, 6);
        }

        return results;
    }
}
