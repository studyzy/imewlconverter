namespace ImeWlConverter.Formats.NoPinyinWordOnly;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Plain word list exporter (no pinyin, one word per line).</summary>
[FormatPlugin("word", "无拼音纯汉字", 2010)]
public sealed partial class NoPinyinWordOnlyExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override string? FormatEntry(WordEntry entry)
    {
        return entry.Word;
    }
}
