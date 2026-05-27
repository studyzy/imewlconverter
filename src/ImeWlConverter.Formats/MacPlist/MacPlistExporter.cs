namespace ImeWlConverter.Formats.MacPlist;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Mac Plist dictionary exporter (XML plist format).</summary>
[FormatPlugin("plist", "Mac简体拼音", 150)]
public sealed partial class MacPlistExporter : IFormatExporter
{
    private const string Header =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"><plist version=\"1.0\"><array>";

    private const string Footer = "</array></plist>";
    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        using var writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: true);
        var count = 0;
        var errorCount = 0;

        writer.Write(Header);

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var py = entry.Code?.GetPrimaryCode("") ?? "";
                if (string.IsNullOrEmpty(py))
                    continue;

                writer.Write(
                    $"<dict><key>phrase</key><string>{entry.Word}</string><key>shortcut</key><string>{py}</string></dict>");
                count++;
            }
            catch
            {
                errorCount++;
            }
        }

        writer.Write(Footer);
        writer.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = count,
            ErrorCount = errorCount
        });
    }
}
