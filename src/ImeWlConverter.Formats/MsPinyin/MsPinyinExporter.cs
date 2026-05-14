namespace ImeWlConverter.Formats.MsPinyin;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Core.Helpers;

/// <summary>Microsoft Pinyin dictionary exporter (XML format with tone-marked pinyin).</summary>
[FormatPlugin("mspy", "微软拼音", 135, FileExtension = ".dctx")]
public sealed partial class MsPinyinExporter : IFormatExporter
{

    public Task<ExportResult> ExportAsync(
        IReadOnlyList<WordEntry> entries, Stream output,
        ExportOptions? options = null, CancellationToken ct = default)
    {
        // UTF-8 with BOM
        var utf8Bom = new UTF8Encoding(true);
        using var writer = new StreamWriter(output, utf8Bom, leaveOpen: true);
        var count = 0;
        var errorCount = 0;

        writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n");
        writer.Write("<ns1:Dictionary xmlns:ns1=\"http://www.microsoft.com/ime/dctx\">");
        writer.Write(
            $@"<ns1:DictionaryHeader>
    <ns1:DictionaryGUID>{{{Guid.NewGuid()}}}</ns1:DictionaryGUID>
    <ns1:DictionaryLanguage>zh-cn</ns1:DictionaryLanguage>
    <ns1:FormatVersion>0</ns1:FormatVersion>
    <ns1:DictionaryVersion>1</ns1:DictionaryVersion>
    <ns1:DictionaryInfo Language=""zh-cn"">
      <ns1:ShortName>深蓝词库</ns1:ShortName>
      <ns1:LongName>深蓝词库转换而成</ns1:LongName>
      <ns1:Description>Dictionary for IME</ns1:Description>
      <ns1:Copyright>深蓝词库转换</ns1:Copyright>
      <ns1:CommentHeader1>CommentTitle1</ns1:CommentHeader1>
      <ns1:CommentHeader2>CommentTitle1</ns1:CommentHeader2>
      <ns1:CommentHeader3>CommentTitle1</ns1:CommentHeader3>
    </ns1:DictionaryInfo>
    <ns1:DictionaryInfo Language=""en-us"">
      <ns1:ShortName>Shenlan</ns1:ShortName>
      <ns1:LongName>Shenlan</ns1:LongName>
      <ns1:Description>Shenlan</ns1:Description>
      <ns1:Copyright>Shenlan</ns1:Copyright>
      <ns1:CommentHeader1>CommentTitle1</ns1:CommentHeader1>
      <ns1:CommentHeader2>CommentTitle1</ns1:CommentHeader2>
      <ns1:CommentHeader3>CommentTitle1</ns1:CommentHeader3>
    </ns1:DictionaryInfo>
    <ns1:ContentCategory>Genral</ns1:ContentCategory>
    <ns1:DictionaryType>Conversion</ns1:DictionaryType>
    <ns1:SourceURL>
    </ns1:SourceURL>
    <ns1:CommentInsertion>true</ns1:CommentInsertion>
    <ns1:IconID>25</ns1:IconID>
  </ns1:DictionaryHeader>
");

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var pinyin = GetTonePinyin(entry);
                writer.Write("<ns1:DictionaryEntry>\r\n");
                writer.Write($"<ns1:InputString>{pinyin}</ns1:InputString>\r\n");
                writer.Write($"<ns1:OutputString>{entry.Word}</ns1:OutputString>\r\n");
                writer.Write("<ns1:Exist>1</ns1:Exist>\r\n");
                writer.Write("</ns1:DictionaryEntry>\r\n");
                count++;
            }
            catch
            {
                errorCount++;
            }
        }

        writer.Write("</ns1:Dictionary>");
        writer.Flush();

        return Task.FromResult(new ExportResult
        {
            EntryCount = count,
            ErrorCount = errorCount
        });
    }

    private static string GetTonePinyin(WordEntry entry)
    {
        if (entry.Code is null || entry.Code.Segments.Count == 0)
            return "";

        var word = entry.Word;
        var segments = entry.Code.Segments;
        var result = new List<string>();

        for (var i = 0; i < segments.Count; i++)
        {
            var py = segments[i][0];
            if (i < word.Length)
            {
                // Add tone number based on character
                var tonePy = PinyinHelper.AddToneToPinyin(word[i], py);
                result.Add(tonePy);
            }
            else
            {
                result.Add(py + "1");
            }
        }

        return string.Join(" ", result);
    }
}
