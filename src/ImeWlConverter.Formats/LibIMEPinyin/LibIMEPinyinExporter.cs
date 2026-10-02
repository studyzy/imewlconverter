namespace ImeWlConverter.Formats.LibIMEPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// Fcitx5 / libime 二进制拼音词库导出器（可被 <c>libime_pinyindict</c> / Fcitx5 直接加载的 <c>*.dict</c>）。
///
/// 词条需要拼音编码（<see cref="CodeType.Pinyin"/>）。每个音节按 libime 规范编码为 2 字节，
/// 词条值为 <see cref="WordEntry.Rank"/>，与文本格式 <c>libimetxt</c> 保持一致。
/// </summary>
[FormatPlugin("libimebin", "LibIME 拼音词库（二进制）", 501, IsBinary = true, FileExtension = ".dict",
    DefaultFileName = "sc")]
public sealed partial class LibIMEPinyinExporter : IFormatExporter
{
    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries,
        Stream output,
        ExportOptions? options = null,
        CancellationToken ct = default)
    {
        var trie = new CedarFloatTrie();
        var errorCount = 0;
        var entryCount = 0;

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();

            if (!TryBuildKey(entry, out var key))
            {
                errorCount++;
                continue;
            }

            trie.Set(key, entry.Rank);
            entryCount++;
        }

        if (entryCount == 0)
            throw new InvalidOperationException(
                "没有可导出的词条: 全部缺少拼音或无法编码。请确认使用了拼音编码生成器。");

        LibimeDictFormat.Write(trie, output);
        output.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = entryCount,
            ErrorCount = errorCount,
        });
    }

    /// <summary>把词条编码为 DATrie 键：每音节 2 字节 + '!' + 汉字 UTF-8。</summary>
    private static bool TryBuildKey(WordEntry entry, out byte[] key)
    {
        key = [];

        var word = entry.Word;
        if (string.IsNullOrEmpty(word))
            return false;

        var pinyin = entry.Code?.GetPrimaryCode("'");
        if (string.IsNullOrEmpty(pinyin))
            return false;

        var syllables = pinyin.Split('\'', StringSplitOptions.RemoveEmptyEntries);
        if (syllables.Length == 0)
            return false;

        var bytes = new List<byte>(syllables.Length * 2 + 1 + Encoding.UTF8.GetByteCount(word));
        foreach (var syllable in syllables)
        {
            if (!LibimePinyinTable.TryEncode(syllable, out var initial, out var final))
                return false;
            bytes.Add(initial);
            bytes.Add(final);
        }

        bytes.Add(LibimeDictFormat.HanziSeparator);
        bytes.AddRange(Encoding.UTF8.GetBytes(word));
        key = bytes.ToArray();
        return true;
    }
}
