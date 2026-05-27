namespace ImeWlConverter.Formats.SougouBin;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Sougou Pinyin bin backup dictionary importer (binary).</summary>
[FormatPlugin("sgpybin", "搜狗拼音备份词库bin", 30)]
public sealed partial class SougouBinImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        return SougouBinParser.Parse(input);
    }
}
