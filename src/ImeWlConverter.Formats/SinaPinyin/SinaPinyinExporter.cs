namespace ImeWlConverter.Formats.SinaPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Sina Pinyin dictionary exporter (text format). Format: pinyin word</summary>
[FormatPlugin("xlpy", "新浪拼音", 180)]
public sealed partial class SinaPinyinExporter : TextFormatExporter
{

    protected override Encoding FileEncoding => Encoding.GetEncoding("GBK");

    protected override string LineEnding => "\n";
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{pinyin} {entry.Word}";
    }
}
