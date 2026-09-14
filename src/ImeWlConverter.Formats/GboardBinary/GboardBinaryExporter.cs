namespace ImeWlConverter.Formats.GboardBinary;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// Gboard 二进制用户词典(<c>user_dict_3_3</c>)导出器。
///
/// 与 <c>gboard</c>(制表符文本)格式的区别: 本格式生成的是 Gboard **实际使用**的
/// 二进制词典文件, 可直接替换设备上的 user_dict_3_3。
/// <list type="bullet">
///   <item>Android: <c>/data/data/com.google.android.inputmethod.latin/files/user_dict_3_3</c></item>
///   <item>iOS: <c>…/AppGroup/…/UserDict/user_dict_3_3</c></item>
/// </list>
///
/// 词条需要**拼音编码**(<see cref="CodeType.Pinyin"/>)。
/// </summary>
[FormatPlugin("gboardbin", "Gboard user_dict_3_3", 112, IsBinary = true, FileExtension = ".dict",
    DefaultFileName = "user_dict_3_3")]
public sealed partial class GboardBinaryExporter : IFormatExporter
{
    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries,
        Stream output,
        ExportOptions? options = null,
        CancellationToken ct = default)
    {
        var words = new List<GboardWord>(entries.Count);
        var warnings = new List<string>();
        var errorCount = 0;

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            if (TryConvert(entry, warnings, out var word))
                words.Add(word);
            else
                errorCount++;
        }

        if (words.Count == 0)
            throw new InvalidOperationException(
                "没有可导出的词条: 全部缺少拼音或无法编码。请确认使用了拼音编码生成器。");

        var data = GboardDictionaryBuilder.Build(words, warnings);
        output.Write(data, 0, data.Length);
        output.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = words.Count,
            ErrorCount = errorCount,
        });
    }

    /// <summary>把流水线的 <see cref="WordEntry"/> 转成构建器输入。</summary>
    private static bool TryConvert(WordEntry entry, List<string> warnings, out GboardWord word)
    {
        word = null!;

        var text = entry.Word;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // 按码点展开(emoji 是单个码点, 但对应的拼音可能有多个音节)
        var codePoints = text.EnumerateRunes().ToArray();
        var cjkCount = codePoints.Count(r => r.Value >= 128);
        if (cjkCount == 0)
            return false;                                   // 纯 ASCII 词对中文词典无意义

        if (entry.CodeType is not (CodeType.Pinyin or CodeType.UserDefine or CodeType.TerraPinyin))
        {
            warnings.Add($"跳过「{text}」: 编码类型 {entry.CodeType} 不是拼音");
            return false;
        }

        var segments = entry.Code?.Segments;
        if (segments is null || segments.Count == 0)
        {
            warnings.Add($"跳过「{text}」: 没有拼音编码");
            return false;
        }

        // 把编码段整理成"每个非 ASCII 码点的候选读音(音节序列)"
        // 外层 = 码点, 中层 = 候选读音, 内层 = 音节序列
        List<string[]>[] pronunciations;

        // WordCode.Segments 的语义: 每个位置一个段, 段内是该位置的候选编码。
        // 于是:
        //   段数 == 码点数      → 与码点一一对齐(含 ASCII 占位)
        //   段数 == 汉字数      → 与汉字一一对齐
        //   段数 == 1 且是整串  → 按汉字数切分音节
        //   段数 >  汉字数      → 扁平音节表(emoji 的拼音可能有多个音节)
        if (segments.Count == codePoints.Length || segments.Count == cjkCount)
        {
            // 每个位置: 候选 = 段内每个元素各作为一个单音节序列
            var perPosition = new List<string[]>[cjkCount];
            var k = 0;
            for (var i = 0; i < segments.Count; i++)
            {
                if (segments.Count == codePoints.Length && codePoints[i].Value < 128)
                    continue;                                   // ASCII 位置不参与
                var alts = segments[i];
                if (alts.Count == 0 || string.IsNullOrWhiteSpace(alts[0]))
                {
                    warnings.Add($"跳过「{text}」: 第 {k + 1} 个字符缺少拼音");
                    return false;
                }
                // 与其他导出器一致: 只取该位置的主编码(entry.Code 的首个候选)。
                // 源词库带注音时用的就是它; 无注音时上游已按字表生成。
                perPosition[k++] = alts
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a => new[] { a })
                    .ToList();
            }
            pronunciations = perPosition;
        }
        else if (segments.Count == 1)
        {
            // 只给了整串拼音 → 按汉字数切分音节
            var joined = segments[0].Count > 0 ? segments[0][0] : "";
            var split = GboardPinyinSplitter.Split(joined, cjkCount);
            if (split is null)
            {
                warnings.Add($"跳过「{text}」: 无法把「{joined}」切分成 {cjkCount} 个音节");
                return false;
            }
            pronunciations = split
                .Select(s => new List<string[]> { new[] { s } })
                .ToArray();
        }
        else if (segments.Count > cjkCount)
        {
            // 扁平音节表 —— 例如 emoji 的拼音有多个音节(输入 eyu 选中 🐊, 即 e + yu)。
            // 多出来的音节优先分给 BMP 之外的码点(emoji)。
            pronunciations = new List<string[]>[cjkCount];
            var si = 0;
            for (var i = 0; i < cjkCount; i++)
            {
                var remainingSyllables = segments.Count - si;
                var remainingChars = cjkCount - i - 1;
                var take = Math.Max(1, remainingSyllables - remainingChars);
                var seq = new List<string>(take);
                for (var t = 0; t < take; t++)
                {
                    var seg = segments[si++];
                    if (seg.Count == 0 || string.IsNullOrWhiteSpace(seg[0]))
                    {
                        warnings.Add($"跳过「{text}」: 第 {i + 1} 个字符缺少拼音");
                        return false;
                    }
                    seq.Add(seg[0]);
                }
                pronunciations[i] = [seq.ToArray()];
            }
        }
        else
        {
            warnings.Add($"跳过「{text}」: 编码段数 {segments.Count} 少于汉字数 {cjkCount}");
            return false;
        }

        var chars = new List<GboardChar>(codePoints.Length);
        var ci = 0;
        foreach (var r in codePoints)
        {
            if (r.Value < 128)
            {
                // Gboard 只保留 ASCII 中的字母与数字(小写写入路径),
                // 空格/撇号/标点一律剥离 —— 实测 "m A上平安" → "ma"+"上平安",
                // "you'd就" → "youd"+"就"。
                if (!char.IsAsciiLetterOrDigit((char)r.Value))
                    continue;
                chars.Add(new GboardChar { Ascii = (char)r.Value });
            }
            else
            {
                var alts = pronunciations[ci++];
                if (alts.Count == 0 || alts[0].Length == 0)
                {
                    warnings.Add($"跳过「{text}」: 第 {ci} 个字符缺少拼音");
                    return false;
                }
                chars.Add(new GboardChar { Pronunciations = alts });
            }
        }

        word = new GboardWord { Word = text, Chars = chars, Rank = entry.Rank };
        return true;
    }
}
