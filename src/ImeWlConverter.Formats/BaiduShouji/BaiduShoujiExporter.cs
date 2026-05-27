namespace ImeWlConverter.Formats.BaiduShouji;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Baidu Mobile dictionary exporter. Format: word(pin|yin) 20000</summary>
[FormatPlugin("bdsj", "百度手机或Mac版百度拼音", 1000)]
public sealed partial class BaiduShoujiExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("|") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{entry.Word}({pinyin}) 20000";
    }
}
