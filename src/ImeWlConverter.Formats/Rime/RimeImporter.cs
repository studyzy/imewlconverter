namespace ImeWlConverter.Formats.Rime;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Rime dictionary importer (text format). Format: word\tcode\trank</summary>
[FormatPlugin("rime", "Rime中州韵", 150)]
public sealed partial class RimeImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    /// <summary>
    /// 排除注释、空行与 .dict.yaml 的 YAML 头。YAML 头里的值可以带汉字
    /// （name: 朙月拼音 / description: ...），只靠「含汉字」判断会漏进词库，
    /// 因此这里按 YAML 语法特征过滤，而不是按字符集。
    /// </summary>
    protected override bool IsContentLine(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            return false;

        // YAML 文档边界
        if (trimmed == "---" || trimmed == "...")
            return false;

        // YAML 键值行（键是 ASCII 标识符）或列表项，例如 "name: xxx" / "  - luna_pinyin"
        if (IsYamlKeyLine(trimmed) || trimmed.StartsWith("- ", StringComparison.Ordinal))
            return false;

        return true;
    }

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        // Rime 允许「只有词条、没有编码」的行（由输入法自动注音）。这类行只有一个字段，
        // 早期实现直接丢弃导致词条静默丢失；此处保留词条并以空编码交给管道自动补拼音。
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
            yield break;

        // 单列行里仍可能残留 YAML 头的纯标量行（version / sort 等 ASCII 词），
        // 要求至少含一个汉字才认作词条，避免把元数据灌进词库。
        if (parts.Length == 1 && !HasCjkCharacter(parts[0]))
            yield break;

        var word = parts[0];
        var code = parts.Length >= 2 ? parts[1] : "";
        var rank = parts.Length >= 3 && int.TryParse(parts[2], out var r) ? r : 0;
        var codeParts = code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(codeParts)
        };
    }

    private static bool IsYamlKeyLine(string text)
    {
        var colon = text.IndexOf(':');
        if (colon <= 0)
            return false;

        // 键必须是 ASCII 标识符，避免把「中国:xxx」这类词条误判成元数据
        if (!(text[0] is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_'))
            return false;

        for (var i = 1; i < colon; i++)
        {
            var c = text[i];
            if (!(char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.' or ' ' or '\t'))
                return false;
        }
        return true;
    }

    /// <summary>按 Unicode 码点判断汉字，覆盖 BMP 外（代理对）的 CJK 扩展区。</summary>
    private static bool HasCjkCharacter(string text)
    {
        for (var i = 0; i < text.Length;)
        {
            int codePoint;
            if (char.IsSurrogatePair(text, i))
            {
                codePoint = char.ConvertToUtf32(text, i);
                i += 2;
            }
            else
            {
                codePoint = text[i];
                i++;
            }

            if (codePoint is >= 0x3400 and <= 0x9FFF
                or >= 0xF900 and <= 0xFAFF
                or >= 0x20000 and <= 0x323AF
                or 0x3007)
                return true;
        }
        return false;
    }
}
