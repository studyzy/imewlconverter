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
[FormatPlugin("gboardbin", "Gboard user_dict_3_3", 112, IsBinary = true, FileExtension = ".dict")]
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
                // FPT2 的 F1 字段即词频（用户选中次数，官方量级 1~140）。
                // 原样映射：F1=0（从未选过）由构建器或上游的 DefaultWordRankGenerator
                // 统一归为 1。
                // 早期这里写 +1（为躲开上游把 Rank==0 覆盖成 1），但那样每轮
                // 「导入→导出」都会把所有词频抬 1，反复往返后全部撞到上限、
                // 同音词的相对顺序丢失。现在构建器自己会把 0 归为 1，无需偏移。
                Rank = w.Rank,
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
