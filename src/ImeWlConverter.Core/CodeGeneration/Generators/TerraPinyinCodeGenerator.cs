using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// TerraPinyinCodeGenerator 地球拼音编码生成器，生成带声调数字后缀的拼音（如 "zhong1"）。
/// </summary>
public sealed class TerraPinyinCodeGenerator : ICodeGenerator
{
    private readonly PinyinCodeGenerator _pinyinGenerator;
    private readonly IPinyinTable _pinyinTable;

    public TerraPinyinCodeGenerator(PinyinCodeGenerator pinyinGenerator, IPinyinTable pinyinTable)
    {
        _pinyinGenerator = pinyinGenerator;
        _pinyinTable = pinyinTable;
    }

    public CodeType SupportedType => CodeType.TerraPinyin;

    public bool Is1Char1Code => true;

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return new WordCode { Segments = Array.Empty<IReadOnlyList<string>>() };
        }

        var pinyinCode = _pinyinGenerator.GenerateCode(word);
        var segments = new List<IReadOnlyList<string>>(word.Length);

        for (var i = 0; i < word.Length; i++)
        {
            if (i < pinyinCode.Segments.Count && pinyinCode.Segments[i].Count > 0)
            {
                var basePinyin = pinyinCode.Segments[i][0];
                var terraPinyin = _pinyinTable.AddTone(word[i], basePinyin);
                segments.Add(new[] { terraPinyin });
            }
            else
            {
                segments.Add(new[] { "" });
            }
        }

        return new WordCode { Segments = segments };
    }
}
