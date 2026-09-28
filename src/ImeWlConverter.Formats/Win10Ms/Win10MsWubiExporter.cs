namespace ImeWlConverter.Formats.Win10Ms;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;

/// <summary>Win10 Microsoft Wubi user dictionary exporter (mschxudp binary format).</summary>
[FormatPlugin("win10mswb", "Win10微软五笔（用户自定义短语）", 131, FileExtension = ".dat")]
public sealed partial class Win10MsWubiExporter : MsChxUdpExporterBase
{
    protected override string GetCode(WordEntry entry) => entry.Code?.GetPrimaryCode("") ?? "";

    // 五笔编码只允许小写字母
    protected override bool IsValidCodeChar(char c) => c >= 'a' && c <= 'z';
}
