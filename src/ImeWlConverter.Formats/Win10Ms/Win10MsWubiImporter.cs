namespace ImeWlConverter.Formats.Win10Ms;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;

/// <summary>Win10 Microsoft Wubi user dictionary importer (mschxudp binary format).</summary>
[FormatPlugin("win10mswb", "Win10微软五笔（用户自定义短语）", 131)]
public sealed partial class Win10MsWubiImporter : MsChxUdpImporterBase
{
    /// <remarks>.dat 文件本身不区分五笔 86/98 版本，此处按 98 解释。</remarks>
    protected override CodeType EntryCodeType => CodeType.Wubi98;

    protected override char? SyllableSeparator => null;
}
