using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// PhraseCodeGenerator 短语编码生成器。
/// 基于已有编码按组词规则生成短语编码：
/// 二字词：取每个字的前两位编码
/// 三字词：取第一字前二码和后两字各取第一码
/// 四字词：取每个字的第一码
/// 多字词：取前三字和最后一字的第一码
/// </summary>
public sealed class PhraseCodeGenerator : ICodeGenerator
{
    private readonly ICodeGenerator? baseGenerator;
    private readonly IPinyinTable pinyinTable;

    /// <summary>使用拼音表作为单字编码来源。</summary>
    public PhraseCodeGenerator(IPinyinTable pinyinTable)
    {
        this.pinyinTable = pinyinTable;
    }

    /// <summary>使用指定的基础编码生成器来获取单字编码，拼音表仅作兜底。</summary>
    public PhraseCodeGenerator(ICodeGenerator baseGenerator, IPinyinTable pinyinTable)
    {
        this.baseGenerator = baseGenerator;
        this.pinyinTable = pinyinTable;
    }

    public CodeType SupportedType => CodeType.Phrase;

    public bool Is1Char1Code => false;

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
            return new WordCode { Segments = [] };

        try
        {
            // 获取每个字的编码
            var charCodes = new List<string>();
            foreach (var c in word)
            {
                var code = GetCharCode(c);
                if (string.IsNullOrEmpty(code))
                    return new WordCode { Segments = [] };
                charCodes.Add(code);
            }

            var phraseCode = BuildPhraseCode(charCodes);
            if (string.IsNullOrEmpty(phraseCode))
                return new WordCode { Segments = [] };

            return new WordCode
            {
                Segments = [new[] { phraseCode }]
            };
        }
        catch
        {
            return new WordCode { Segments = [] };
        }
    }

    private string GetCharCode(char c)
    {
        if (baseGenerator != null)
        {
            var wordCode = baseGenerator.GenerateCode(c.ToString());
            if (wordCode.Segments.Count > 0 && wordCode.Segments[0].Count > 0)
                return wordCode.Segments[0][0];
        }

        // 回退到拼音
        try
        {
            return pinyinTable.GetDefaultPinyin(c);
        }
        catch
        {
            return "";
        }
    }

    private static string BuildPhraseCode(List<string> charCodes)
    {
        if (charCodes.Count == 0) return "";

        if (charCodes.Count == 1)
            return charCodes[0];

        if (charCodes.Count == 2)
        {
            // 二字词：各取前两位
            var c1 = charCodes[0].Length >= 2 ? charCodes[0][..2] : charCodes[0];
            var c2 = charCodes[1].Length >= 2 ? charCodes[1][..2] : charCodes[1];
            return c1 + c2;
        }

        if (charCodes.Count == 3)
        {
            // 三字词：第一字前两码，后两字各取第一码
            var c1 = charCodes[0].Length >= 2 ? charCodes[0][..2] : charCodes[0];
            var c2 = charCodes[1][..1];
            var c3 = charCodes[2][..1];
            return c1 + c2 + c3;
        }

        if (charCodes.Count == 4)
        {
            // 四字词：每字取第一码
            return $"{charCodes[0][0]}{charCodes[1][0]}{charCodes[2][0]}{charCodes[3][0]}";
        }

        // 多字词：前三末一各取第一码
        return $"{charCodes[0][0]}{charCodes[1][0]}{charCodes[2][0]}{charCodes[^1][0]}";
    }
}
