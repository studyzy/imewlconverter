using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// ChaoyinCodeGenerator 超音速写输入法编码生成器。
/// </summary>
public sealed class ChaoyinCodeGenerator : ICodeGenerator
{
    private readonly PinyinCodeGenerator _pinyinGenerator;
    private readonly IChaoyinTable _chaoyinTable;

    public ChaoyinCodeGenerator(PinyinCodeGenerator pinyinGenerator, IChaoyinTable chaoyinTable)
    {
        _pinyinGenerator = pinyinGenerator;
        _chaoyinTable = chaoyinTable;
    }

    public CodeType SupportedType => CodeType.Chaoyin;

    public bool Is1Char1Code => true;

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return new WordCode { Segments = Array.Empty<IReadOnlyList<string>>() };
        }

        var pinyinCode = _pinyinGenerator.GenerateCode(word);
        var pinyinList = new List<string>();
        foreach (var segment in pinyinCode.Segments)
        {
            if (segment.Count > 0 && !string.IsNullOrEmpty(segment[0]))
            {
                pinyinList.Add(segment[0]);
            }
        }

        if (pinyinList.Count == 0)
        {
            return new WordCode { Segments = Array.Empty<IReadOnlyList<string>>() };
        }

        var chaoyinCode = _chaoyinTable.GetWordChaoyin(pinyinList);
        return WordCode.FromSingle(new[] { chaoyinCode });
    }
}
