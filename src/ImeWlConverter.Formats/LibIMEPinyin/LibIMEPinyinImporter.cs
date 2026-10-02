namespace ImeWlConverter.Formats.LibIMEPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// Fcitx5 / libime 二进制拼音词库导入器（<c>libime_pinyindict</c> 生成的 <c>*.dict</c>）。
///
/// 文件由 libime <c>DATrie&lt;float&gt;</c> 构成，键为
/// <c>encodeFullPinyinWithFlags(pinyin, VE_UE)</c> 的每音节 2 字节编码 + <c>'!'</c> + 汉字 UTF-8，
/// 值为该词条的词频/代价（float）。详见 <see cref="LibimeDictFormat"/>。
/// </summary>
[FormatPlugin("libimebin", "LibIME 拼音词库（二进制）", 501, IsBinary = true, FileExtension = ".dict",
    DefaultFileName = "sc")]
public sealed partial class LibIMEPinyinImporter : IFormatImporter
{
    public Task<ImportResult> ImportAsync(
        Stream input,
        ImportOptions? options = null,
        CancellationToken ct = default)
    {
        var trie = LibimeDictFormat.Read(input);
        var entries = new List<WordEntry>();
        var errors = new List<string>();
        var errorCount = 0;

        foreach (var (key, value) in trie.Enumerate())
        {
            ct.ThrowIfCancellationRequested();

            var separator = Array.IndexOf(key, LibimeDictFormat.HanziSeparator);
            // 键必须形如「偶数个拼音字节 + '!' + 非空汉字」，否则不是拼音词典条目。
            if (separator <= 0 || separator % 2 != 0 || separator == key.Length - 1)
            {
                errorCount++;
                continue;
            }

            var word = Encoding.UTF8.GetString(key, separator + 1, key.Length - separator - 1);
            if (string.IsNullOrEmpty(word))
            {
                errorCount++;
                continue;
            }

            var pinyin = LibimePinyinTable.DecodeFullPinyin(key.AsSpan(0, separator));
            // 不用 RemoveEmptyEntries：空音节意味着编码无法识别，应整体跳过而不是静默丢字。
            var syllables = pinyin.Split('\'', StringSplitOptions.None);
            if (syllables.Length == 0 || syllables.Any(string.IsNullOrEmpty))
            {
                errors.Add($"跳过无法解码的拼音键：{word}");
                errorCount++;
                continue;
            }

            entries.Add(new WordEntry
            {
                Word = word,
                // libime 存储的是 float 词频/代价，与文本格式一致直接作为 Rank 保留。
                Rank = float.IsFinite(value) ? (int)Math.Clamp(value, int.MinValue, int.MaxValue) : 0,
                CodeType = CodeType.Pinyin,
                Code = WordCode.FromSingle(syllables),
            });
        }

        return Task.FromResult(new ImportResult
        {
            Entries = entries,
            ErrorCount = errorCount,
            Errors = errors,
        });
    }
}
