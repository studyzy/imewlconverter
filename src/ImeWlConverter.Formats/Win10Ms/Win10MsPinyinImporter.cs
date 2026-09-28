namespace ImeWlConverter.Formats.Win10Ms;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;

/// <summary>Win10 Microsoft Pinyin user dictionary importer (mschxudp binary format).</summary>
[FormatPlugin("win10mspy", "Win10微软拼音（用户自定义短语）", 130)]
public sealed partial class Win10MsPinyinImporter : MsChxUdpImporterBase
{
    protected override CodeType EntryCodeType => CodeType.Pinyin;

    protected override char? SyllableSeparator => '\'';
}
