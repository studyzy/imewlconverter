namespace ImeWlConverter.Formats.LibIMEText;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>LibIME Text dictionary exporter. Format: word pinyin rank (with lue→lve, nue→nve)</summary>
[FormatPlugin("libimetxt", "LibIME 拼音词库（文本格式）", 500)]
public sealed partial class LibIMETextExporter : TextFormatExporter
{
    // libime 的 saveText 输出不带 BOM；带 BOM 会让第一行的汉字带上 U+FEFF。
    protected override Encoding FileEncoding => new UTF8Encoding(false);

    protected override string LineEnding => "\n";
    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("'") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        // LibIME uses lve/nve instead of lue/nue
        pinyin = pinyin.Replace("lue", "lve").Replace("nue", "nve");
        // 与 libime saveText 一致：词面走 fcitx escapeForValue（含空白/引号/反斜杠时加引号转义）。
        return $"{LibimeTextEscaping.EscapeValue(entry.Word)} {pinyin} {entry.Rank}";
    }
}
