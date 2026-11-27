namespace ImeWlConverter.Formats.UserPhrase;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// User-defined phrase exporter. Export only, format: "{code},{position}={word}".
/// 第二列是同一编码下的候选排位（1 起），不是词频；与 Win10 微软拼音自定义短语
/// 导出器（MsChxUdpExporterBase）保持一致，按编码自动编号。
/// </summary>
[FormatPlugin("dy", "搜狗用户自定义短语", 11)]
public sealed partial class UserDefinePhraseExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);

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
        return $"{code},{rank}={entry.Word}";
    }
}
