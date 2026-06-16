namespace ImeWlConverter.Formats.Xiaoxiao;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Xiaoxiao IME dictionary exporter. Groups words by code: "code word1 word2".</summary>
[FormatPlugin("xiaoxiao", "小小输入法", 100)]
public sealed partial class XiaoxiaoExporter : IFormatExporter
{
    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        var encoding = GetEncoding();
        using var writer = new StreamWriter(output, encoding, leaveOpen: true);
        var count = 0;

        var dict = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            var codes = entry.Code?.Segments;
            if (codes == null || codes.Count == 0)
                continue;

            var key = entry.Code!.GetPrimaryCode("");
            if (string.IsNullOrEmpty(key))
                continue;

            if (dict.ContainsKey(key))
                dict[key] += " " + entry.Word;
            else
                dict[key] = entry.Word;
            count++;
        }

        foreach (var kvp in dict)
        {
            writer.Write(kvp.Key);
            writer.Write(' ');
            writer.Write(kvp.Value);
            writer.Write('\n');
        }

        writer.Flush();
        return Task.FromResult(new ExportResult { EntryCount = count });
    }

    private static Encoding GetEncoding()
    {
        try
        {
            return Encoding.GetEncoding("GB18030");
        }
        catch
        {
            return Encoding.GetEncoding("GB2312");
        }
    }
}
