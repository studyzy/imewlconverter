namespace ImeWlConverter.Formats.GboardBinary;

using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>
/// Gboard 二进制用户词典(<c>user_dict_3_3</c>)导入器。
/// 解析设备上的词典文件, 还原词面与拼音编码。
/// </summary>
[FormatPlugin("gboardbin", "Gboard 二进制词典", 112, IsBinary = true, FileExtension = ".dict")]
public sealed partial class GboardBinaryImporter : IFormatImporter
{
    public Task<ImportResult> ImportAsync(
        Stream input,
        ImportOptions? options = null,
        CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        var data = ms.ToArray();

        var decoded = GboardBinaryReader.Read(data);
        var entries = new List<WordEntry>(decoded.Count);

        foreach (var w in decoded)
        {
            ct.ThrowIfCancellationRequested();

            WordCode? code = null;
            if (w.Pinyins.Count > 0)
            {
                code = new WordCode
                {
                    Segments = w.Pinyins
                        .Select(p => (IReadOnlyList<string>)new[] { p })
                        .ToList(),
                };
            }

            entries.Add(new WordEntry
            {
                Word = w.Word,
                CodeType = CodeType.Pinyin,
                Code = code,
            });
        }

        return Task.FromResult(new ImportResult
        {
            Entries = entries,
            ErrorCount = 0,
        });
    }
}
