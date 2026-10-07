namespace ImeWlConverter.Formats.QingJian;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>青简输入法 TSV 词库导入。Format: 词\t拼音\t词频，拼音音节间以空格分隔</summary>
[FormatPlugin("qingjiantsv", "青简 TSV", 160, FileExtension = ".tsv")]
public sealed partial class QingJianImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 2)
            yield break;

        var word = parts[0];
        var pinyin = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (pinyin.Length == 0)
            yield break;

        // 青简官方文档：TSV 与 Rime 词库导入时，lue/nue 自动统一为 lve/nve
        for (var i = 0; i < pinyin.Length; i++)
            pinyin[i] = QingJianQjSpec.CanonicalizeSyllable(pinyin[i]);

        var rank = parts.Length >= 3 && int.TryParse(parts[2], out var r) ? r : 0;

        yield return new WordEntry
        {
            Word = word,
            Rank = rank,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyin)
        };
    }
}
