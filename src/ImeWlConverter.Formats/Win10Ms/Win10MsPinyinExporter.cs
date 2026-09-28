namespace ImeWlConverter.Formats.Win10Ms;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;

/// <summary>Win10 Microsoft Pinyin user dictionary exporter (mschxudp binary format).</summary>
[FormatPlugin("win10mspy", "Win10微软拼音（用户自定义短语）", 130, FileExtension = ".dat")]
public sealed partial class Win10MsPinyinExporter : MsChxUdpExporterBase
{
    protected override string GetCode(WordEntry entry) => entry.Code?.GetPrimaryCode("'") ?? "";

    // 拼音编码只允许小写字母与音节分隔符
    protected override bool IsValidCodeChar(char c) => c == '\'' || (c >= 'a' && c <= 'z');
}
