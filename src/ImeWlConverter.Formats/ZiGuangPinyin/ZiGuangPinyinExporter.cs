namespace ImeWlConverter.Formats.ZiGuangPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>ZiGuang (Huayu) Pinyin dictionary exporter (text format). Format: word\tpinyin\t100000</summary>
[FormatPlugin("zgpy", "华宇紫光拼音", 170)]
public sealed partial class ZiGuangPinyinExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override string? GetHeader() => "名称=用户词库\r\n作者=深蓝词库转换\r\n编辑=1\r\n";

    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{entry.Word}\t{pinyin}\t100000";
    }
}
