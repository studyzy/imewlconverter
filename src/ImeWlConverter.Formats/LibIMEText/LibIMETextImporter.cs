namespace ImeWlConverter.Formats.LibIMEText;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>LibIME Text dictionary importer. Format: word pinyin [rank] (与 libime loadText 一致)</summary>
[FormatPlugin("libimetxt", "LibIME 拼音词库（文本格式）", 500)]
public sealed partial class LibIMETextImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        // libime 文本为「汉字 拼音 [词频]」，词频可省略；
        // 词面含空白/引号/反斜杠时会用引号包裹并按 fcitx 规则转义，故不能直接按空格切分。
        var parts = LibimeTextEscaping.Tokenize(line);
        // 与 libime loadTextImpl 一致：只接受 2 列或 3 列，其余整行忽略。
        if (parts.Count is not (2 or 3))
            yield break;

        var word = parts[0];
        var pinyinParts = parts[1].Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
        if (pinyinParts.Length == 0)
            yield break;

        var rank = parts.Count >= 3 && int.TryParse(parts[2], out var r) ? r : 0;

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
