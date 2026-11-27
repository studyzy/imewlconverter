namespace ImeWlConverter.Formats.QQPhrase;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// QQ 输入法自定义短语导出器。格式（UTF-16LE）：「编码=排序位置,词语」，
/// 如 <c>slckzh=1,深蓝词库转换</c>。排序位置为同一编码下的候选序号（1 起），
/// 与 dy / win10mspy 导出器保持一致，按编码自动编号。
/// </summary>
[FormatPlugin("qqdy", "QQ输入法自定义短语", 51, FileExtension = ".ini")]
public sealed partial class QQPhraseExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;

    /// <summary>按编码回填排位后再交给基类逐行写出；状态为本次调用局部量，可并发复用实例。</summary>
    public override Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        var positions = new Dictionary<string, int>(StringComparer.Ordinal);
        var numbered = new List<WordEntry>(entries.Count);
        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            var code = entry.Code?.GetPrimaryCode("") ?? "";
            if (string.IsNullOrEmpty(code))
            {
                numbered.Add(entry); // 无编码词条由 FormatEntry 跳过
                continue;
            }

            positions.TryGetValue(code, out var position);
            positions[code] = ++position;
            numbered.Add(entry with { Rank = position });
        }

        return base.ExportAsync(numbered, output, options, ct);
    }

    protected override string? FormatEntry(WordEntry entry)
    {
        var code = entry.Code?.GetPrimaryCode("") ?? "";
        if (string.IsNullOrEmpty(code))
            return null;

        var rank = entry.Rank == 0 ? 1 : entry.Rank;
        return $"{code}={rank},{entry.Word}";
    }
}
