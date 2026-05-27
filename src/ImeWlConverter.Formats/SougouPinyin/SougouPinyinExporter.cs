namespace ImeWlConverter.Formats.SougouPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Sougou Pinyin dictionary exporter (text format).</summary>
[FormatPlugin("sgpy", "搜狗拼音txt", 10)]
public sealed partial class SougouPinyinExporter : TextFormatExporter
{

    protected override Encoding FileEncoding => Encoding.GetEncoding("GBK");
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"'{pinyin} {entry.Word}";
    }
}
