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

        for (var i = 0; i < word.Length && i < segments.Count; i++)
        {
            var c = word[i];
            var seg = segments[i];

            if (char.IsDigit(c))
            {
                if (!options.KeepNumberInCode)
                {
                    newSegments[i] = Array.Empty<string>();
                    modified = true;
                    continue;
                }

                if (options.TranslateNumbersToChinese)
                {
                    var chineseDigit = ChineseDigits[c - '0'];
                    newSegments[i] = [chineseDigit];
                    modified = true;
                    continue;
                }
            }

            if (IsEnglishLetter(c))
            {
                if (!options.KeepEnglishInCode)
                {
                    newSegments[i] = Array.Empty<string>();
                    modified = true;
                    continue;
                }

                if (options.PrefixEnglishWithUnderscore && seg.Count > 0)
                {
                    newSegments[i] = seg.Select(s => "_" + s).ToArray();
                    modified = true;
                    continue;
                }
            }

            if (IsPunctuationOrSymbol(c) && !options.KeepPunctuationInCode)
            {
                newSegments[i] = Array.Empty<string>();
                modified = true;
                continue;
            }

            if (options.ConvertFullWidth && seg.Count > 0)
            {
                var converted = seg.Select(ConvertFullWidthToHalf).ToList();
                if (!SequenceEqual(seg, converted))
                {
                    newSegments[i] = converted;
                    modified = true;
                    continue;
                }
            }

            newSegments[i] = seg;
        }

        if (!modified)
            return entry;

        return entry with
        {
            Code = new WordCode { Segments = newSegments }
        };
    }

    private static bool IsEnglishLetter(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    private static bool IsPunctuationOrSymbol(char c)
    {
        // 非中文、非英文、非数字、非空格的字符视为标点/符号
        if (IsCJK(c)) return false;
        if (IsEnglishLetter(c)) return false;
        if (char.IsDigit(c)) return false;
        if (char.IsWhiteSpace(c)) return false;
        return true;
    }

    private static bool IsCJK(char c) =>
        // CJK Unified Ideographs covers most Chinese characters.
        // U+3007 (〇, ideographic number zero) is used as the Chinese numeral "零"
        // and must be treated as CJK, not punctuation/symbol.
        (c >= '\u4E00' && c <= '\u9FFF') || c == '\u3007';

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
