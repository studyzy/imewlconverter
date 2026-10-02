using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;

namespace ImeWlConverter.Core.CodeGeneration;

/// <summary>
/// 编码生成后处理器，根据 CodeGenerationOptions 对已生成的 WordCode 进行调整：
/// 处理英文/数字/标点的编码保留或清除、英文前导下划线、数字转中文、全角转半角。
/// </summary>
public static class CodeGenerationPostProcessor
{
    private static readonly string[] ChineseDigits = ["零", "一", "二", "三", "四", "五", "六", "七", "八", "九"];

    /// <summary>
    /// 根据选项对已生成编码的词条列表进行后处理。
    /// </summary>
    public static IReadOnlyList<WordEntry> Apply(
        IReadOnlyList<WordEntry> entries, CodeGenerationOptions options)
    {
        if (IsNoOp(options))
            return entries;

        var result = new List<WordEntry>(entries.Count);
        for (var i = 0; i < entries.Count; i++)
            result.Add(ApplyToOne(entries[i], options));

        return result;
    }

    /// <summary>
    /// 判断选项是否全部为默认值（无需后处理）。
    /// </summary>
    private static bool IsNoOp(CodeGenerationOptions options)
    {
        return options.KeepEnglishInCode
               && options.KeepNumberInCode
               && options.KeepPunctuationInCode
               && !options.PrefixEnglishWithUnderscore
               && !options.TranslateNumbersToChinese
               && !options.ConvertFullWidth;
    }

    private static WordEntry ApplyToOne(WordEntry entry, CodeGenerationOptions options)
    {
        if (entry.Code is null || entry.Code.Segments.Count == 0)
            return entry;

        var word = entry.Word;
        var segments = entry.Code.Segments;
        var modified = false;
        var newSegments = new IReadOnlyList<string>[segments.Count];

        // 按 Unicode 码点对齐 word 与 segments（一个码点一个 segment）。
        // 不能直接用 UTF-16 下标配对：BMP 之外的汉字（CJK 扩展 B 及以后）在 UTF-16 中
        // 是代理对，按下标配对会把代理对的两半各自当成一个「标点/符号」清空编码，
        // 同时让后续字与 segment 下标整体错位（如 𫚉 的 hong 被清成 ''）。
        var codePoints = new List<(int Value, int Length)>();
        for (var i = 0; i < word.Length;)
        {
            // char.IsSurrogatePair 自带越界与低位校验，落单的代理项（畸形输入）
            // 按单个码元处理，避免 char.ConvertToUtf32 抛异常。
            var isSurrogatePair = char.IsSurrogatePair(word, i);
            codePoints.Add((
                isSurrogatePair ? char.ConvertToUtf32(word, i) : word[i],
                isSurrogatePair ? 2 : 1));
            i += isSurrogatePair ? 2 : 1;
        }

        // libime 文本会省略标点/符号的拼音（如「芭芭拉·巴布科克」8 个字只有 7 个音节），
        // 此时标点不占用 segment；生成路径（含拼音生成器）则为每个码点都产出 segment。
        // 仅当「非标点码点数」正好等于 segment 数时按省略标点对齐，否则退回逐码点 1:1 配对。
        var skipPunctuation = codePoints.Count != segments.Count
            && codePoints.Count(cp => !IsPunctuationOrSymbol(cp.Value)) == segments.Count;

        var segIndex = 0;
        foreach (var (codePoint, _) in codePoints)
        {
            if (segIndex >= segments.Count)
                break;

            // 该码点在 libime 文本中没有对应音节，不占用 segment（也就无编码可清）。
            if (skipPunctuation && IsPunctuationOrSymbol(codePoint))
                continue;

            var seg = segments[segIndex];
            IReadOnlyList<string>? replacement = null;

            if (IsDigit(codePoint))
            {
                if (!options.KeepNumberInCode)
                {
                    replacement = Array.Empty<string>();
                }
                else if (options.TranslateNumbersToChinese)
                {
                    replacement = [ChineseDigits[codePoint - '0']];
                }
            }
            else if (IsEnglishLetter(codePoint))
            {
                if (!options.KeepEnglishInCode)
                {
                    replacement = Array.Empty<string>();
                }
                else if (options.PrefixEnglishWithUnderscore && seg.Count > 0)
                {
                    replacement = seg.Select(s => "_" + s).ToArray();
                }
            }
            else if (IsPunctuationOrSymbol(codePoint) && !options.KeepPunctuationInCode)
            {
                replacement = Array.Empty<string>();
            }
            else if (options.ConvertFullWidth && seg.Count > 0)
            {
                var converted = seg.Select(ConvertFullWidthToHalf).ToList();
                if (!SequenceEqual(seg, converted))
                {
                    replacement = converted;
                }
            }

            if (replacement is null)
            {
                newSegments[segIndex] = seg;
            }
            else
            {
                newSegments[segIndex] = replacement;
                modified = true;
            }

            segIndex++;
        }

        // word 比 segments 短时（编码段多于字），其余 segment 原样保留。
        for (; segIndex < segments.Count; segIndex++)
            newSegments[segIndex] = segments[segIndex];

        if (!modified)
            return entry;

        return entry with
        {
            Code = new WordCode { Segments = newSegments }
        };
    }

    private static bool IsEnglishLetter(int codePoint) =>
        codePoint is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    /// <summary>ASCII/BMP 数字判断（与 <see cref="char.IsDigit(char)"/> 语义一致）。</summary>
    private static bool IsDigit(int codePoint) =>
        codePoint <= char.MaxValue && char.IsDigit((char)codePoint);

    private static bool IsPunctuationOrSymbol(int codePoint)
    {
        // 非中文、非英文、非数字、非空格的字符视为标点/符号
        if (IsCJK(codePoint)) return false;
        if (IsEnglishLetter(codePoint)) return false;
        if (IsDigit(codePoint)) return false;
        if (codePoint <= char.MaxValue && char.IsWhiteSpace((char)codePoint)) return false;
        return true;
    }

    private static bool IsCJK(int codePoint) =>
        // CJK Unified Ideographs covers most Chinese characters.
        // CJK Extension A (U+3400-U+4DBF) also contains rare Chinese characters
        // (e.g. 㐖 U+3416), which must be treated as CJK, not punctuation/symbol
        // (issue #424: their pinyin segments were wrongly cleared).
        // Extensions B and later (U+20000+, e.g. 𫚉 U+2B689, 𩽾 U+29F7E) live outside
        // the BMP and are encoded as surrogate pairs; they are CJK too and their pinyin
        // segments must be preserved (previously cleared as "punctuation" because each
        // surrogate half was judged on its own).
        // IME 词库还会用私用区（PUA）承载生僻字（如 U+E000 段的「𣲗」类形声字），
        // 它们在词条里同样带拼音，必须保留而不是当标点清空。
        // U+3007 (〇, ideographic number zero) is used as the Chinese numeral "零"
        // and must be treated as CJK, not punctuation/symbol.
        (codePoint >= 0x3400 && codePoint <= 0x4DBF) ||      // CJK Extension A
        (codePoint >= 0x4E00 && codePoint <= 0x9FFF) ||      // CJK Unified Ideographs
        (codePoint >= 0x2E80 && codePoint <= 0x2FDF) ||      // CJK Radicals Supplement + Kangxi Radicals
        (codePoint >= 0x31C0 && codePoint <= 0x31EF) ||      // CJK Strokes
        (codePoint >= 0x3200 && codePoint <= 0x33FF) ||      // Enclosed CJK Letters/Months + CJK Compatibility
        (codePoint >= 0xE000 && codePoint <= 0xF8FF) ||      // Private Use Area（IME 生僻字）
        (codePoint >= 0xF900 && codePoint <= 0xFAFF) ||      // CJK Compatibility Ideographs
        (codePoint >= 0x1F200 && codePoint <= 0x1F2FF) ||    // Enclosed Ideographic Supplement
        (codePoint >= 0x20000 && codePoint <= 0x2FA1F) ||    // CJK Extensions B-F + Supplement
        (codePoint >= 0x30000 && codePoint <= 0x323AF) ||    // CJK Extensions G-H
        codePoint == 0x3007;                                 // 〇

    private static string ConvertFullWidthToHalf(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        var chars = s.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c >= '\uFF01' && c <= '\uFF5E')
                chars[i] = (char)(c - 0xFEE0);
            else if (c == '\u3000')
                chars[i] = ' ';
        }

        return new string(chars);
    }

    private static bool SequenceEqual(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        if (a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }

        return true;
    }
}
