using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// ZhuyinCodeGenerator 注音符号编码生成器，在地球拼音的基础上转换为注音符号。
/// </summary>
public sealed class ZhuyinCodeGenerator : ICodeGenerator
{
    private readonly TerraPinyinCodeGenerator _terraPinyinGenerator;
    private readonly IZhuyinTable _zhuyinTable;

    public ZhuyinCodeGenerator(TerraPinyinCodeGenerator terraPinyinGenerator, IZhuyinTable zhuyinTable)
    {
        _terraPinyinGenerator = terraPinyinGenerator;
        _zhuyinTable = zhuyinTable;
    }

    public CodeType SupportedType => CodeType.Zhuyin;

    public bool Is1Char1Code => true;

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return new WordCode { Segments = Array.Empty<IReadOnlyList<string>>() };
        }

        var terraCode = _terraPinyinGenerator.GenerateCode(word);
        var segments = new List<IReadOnlyList<string>>(word.Length);

        foreach (var segment in terraCode.Segments)
        {
            if (segment.Count > 0 && !string.IsNullOrEmpty(segment[0]))
            {
                var zhuyin = _zhuyinTable.GetZhuyin(segment[0]);
                segments.Add(new[] { zhuyin ?? "" });
            }
            else
            {
                segments.Add(new[] { "" });
            }
        }

        return new WordCode { Segments = segments };
    }
}
