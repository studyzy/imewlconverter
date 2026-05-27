namespace ImeWlConverter.Formats.LibIMEText;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>LibIME Text dictionary exporter. Format: word pinyin rank (with lue→lve, nue→nve)</summary>
[FormatPlugin("libimetxt", "LibIME 拼音词库（文本格式）", 500)]
public sealed partial class LibIMETextExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;

    protected override string LineEnding => "\n";
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        // LibIME uses lve/nve instead of lue/nue
        pinyin = pinyin.Replace("lue", "lve").Replace("nue", "nve");
        return $"{entry.Word} {pinyin} {entry.Rank}";
    }
}
