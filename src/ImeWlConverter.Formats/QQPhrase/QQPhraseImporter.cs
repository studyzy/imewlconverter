namespace ImeWlConverter.Formats.QQPhrase;

using System.Globalization;
using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// QQ 输入法自定义短语导入器。格式（UTF-16LE）：「编码=排序位置,词语」，
/// 如 <c>slckzh=1,深蓝词库转换</c>。词语本身可含逗号与 QQ 的 $(...) 日期/时间宏。
/// 以 <c>编码=$[...]</c> 形式出现的整行是 QQ 内置符号集定义，不是普通短语，导入时跳过。
/// </summary>
[FormatPlugin("qqdy", "QQ输入法自定义短语", 51, FileExtension = ".ini")]
public sealed partial class QQPhraseImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var eq = line.IndexOf('=');
        if (eq <= 0)
            yield break;

        var code = line[..eq].Trim();
        if (code.Length == 0)
            yield break;

        var rest = line[(eq + 1)..];
        // QQ 内置符号集：zzbd=$[　、。…]。内容不是单条短语，跳过以免破坏原格式语义。
        if (rest.StartsWith("$[", StringComparison.Ordinal))
            yield break;

        var comma = rest.IndexOf(',');
        if (comma <= 0)
            yield break;

        if (!int.TryParse(rest[..comma], NumberStyles.Integer, CultureInfo.InvariantCulture, out var position)
            || position < 0)
            yield break;

        var word = rest[(comma + 1)..];
        if (word.Length == 0)
            yield break;

        yield return new WordEntry
        {
            Word = word,
            Rank = position,
            CodeType = CodeType.UserDefinePhrase,
            Code = WordCode.FromSingle(new[] { code })
        };
    }
}
