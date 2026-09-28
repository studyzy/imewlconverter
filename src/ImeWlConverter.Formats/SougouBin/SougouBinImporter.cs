namespace ImeWlConverter.Formats.SougouBin;

using System.IO;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Sougou Pinyin bin backup dictionary importer (binary).</summary>
[FormatPlugin("sgpybin", "搜狗拼音备份词库bin", 30)]
public sealed partial class SougouBinImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        try
        {
            return SougouBinParser.Parse(input);
        }
        catch (EndOfStreamException)
        {
            // 文件头声明的长度超出实际文件大小: 损坏或格式变体, 参见 issue #416
            throw new InvalidDataException(
                "搜狗备份词库解析失败: 文件不完整或格式不兼容, 请确认文件为搜狗输入法\"导出词库\"生成的完整备份文件");
        }
    }
}
