namespace ImeWlConverter.Formats.BaiduShouji;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Baidu Mobile dictionary importer. Format: word(pin|yin) rank</summary>
[FormatPlugin("bdsj", "百度手机或Mac版百度拼音", 1000)]
public sealed partial class BaiduShoujiImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => Encoding.Unicode;
    protected override IEnumerable<WordEntry> ParseLine(string line)
    {
        var parenIdx = line.IndexOf('(');
        if (parenIdx < 0)
            yield break;

        var word = line[..parenIdx];
        var closeIdx = line.IndexOf(')', parenIdx);
        if (closeIdx < 0)
            yield break;

        var py = line[(parenIdx + 1)..closeIdx];
        var pinyinParts = py.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        yield return new WordEntry
        {
            Word = word,
            Rank = 1,
            CodeType = CodeType.Pinyin,
            Code = WordCode.FromSingle(pinyinParts)
        };
    }
}
