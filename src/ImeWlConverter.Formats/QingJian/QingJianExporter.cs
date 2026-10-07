namespace ImeWlConverter.Formats.QingJian;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>青简输入法 TSV 词库导出。Format: 词\t拼音\t词频，拼音音节间以空格分隔，lue/nue 统一为 lve/nve</summary>
[FormatPlugin("qingjiantsv", "青简 TSV", 160, FileExtension = ".tsv")]
public sealed partial class QingJianExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    protected override string LineEnding => "\n";

    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = QingJianQjSpec.CanonicalizePinyin(entry.Code?.GetPrimaryCode(" ") ?? "");
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{entry.Word}\t{pinyin}\t{entry.Rank}";
    }
}
