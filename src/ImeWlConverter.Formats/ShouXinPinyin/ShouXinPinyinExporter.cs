namespace ImeWlConverter.Formats.ShouXinPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>ShouXin Pinyin dictionary exporter (text format). Format: word\tpinyin\trank</summary>
[FormatPlugin("sxpy", "手心输入法", 180)]
public sealed partial class ShouXinPinyinExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{entry.Word}\t{pinyin}\t{entry.Rank}";
    }
}
