namespace ImeWlConverter.Formats.MacPlist;

using System.Text;
using System.Xml;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

/// <summary>Mac Plist dictionary importer (XML plist format).</summary>
[FormatPlugin("plist", "Mac简体拼音", 150)]
public sealed partial class MacPlistImporter : IFormatImporter
{

    public Task<ImportResult> ImportAsync(Stream input, ImportOptions? options = null, CancellationToken ct = default)
    {
        var entries = new List<WordEntry>();
        var errors = new List<string>();

        using var reader = new StreamReader(input, Encoding.UTF8);
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(reader.ReadToEnd());

        var nodes = xmlDoc.SelectNodes("//plist/array/dict");
        if (nodes == null)
            return Task.FromResult(new ImportResult { Entries = entries, Errors = errors });

        for (var i = 0; i < nodes.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var xn = nodes[i]!;
                var stringNodes = xn.SelectNodes("string");
                if (stringNodes == null || stringNodes.Count < 2)
                    continue;

                var word = stringNodes[0]!.InnerText;
                var shortcut = stringNodes[1]!.InnerText;

                entries.Add(new WordEntry
                {
                    Word = word,
                    Rank = 1,
                    CodeType = CodeType.Pinyin,
                    Code = WordCode.FromSingle(shortcut.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                });
            }
            catch (Exception ex)
            {
                errors.Add($"Entry {i}: {ex.Message}");
            }
        }

        return Task.FromResult(new ImportResult
        {
            Entries = entries,
            ErrorCount = errors.Count,
            Errors = errors
        });
    }
}
