using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// WubiCodeGeneratorBase 五笔编码生成器基类，封装五笔系的公共取码逻辑。
/// </summary>
public abstract class WubiCodeGeneratorBase : ICodeGenerator
{
    private readonly ICodeTableLibrary _codeTable;

    protected WubiCodeGeneratorBase(ICodeTableLibrary codeTable)
    {
        _codeTable = codeTable;
    }

    public abstract CodeType SupportedType { get; }

    public bool Is1Char1Code => false;

    /// <summary>
    /// 从 ChineseCode 中获取对应版本的五笔编码。
    /// </summary>
    protected abstract string GetWubiCode(ChineseCode code);

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
            return new WordCode { Segments = [] };

        try
        {
            var wubiCode = GetWordWubiCode(word);
            if (string.IsNullOrEmpty(wubiCode))
                return new WordCode { Segments = [] };

            return new WordCode
            {
                Segments = [new[] { wubiCode }]
            };
        }
        catch
        {
            return new WordCode { Segments = [] };
        }
    }

    private string GetWordWubiCode(string word)
    {
        if (word.Length == 1)
            return GetCharCode(word[0]);

        if (word.Length == 2)
        {
            var code1 = GetCharCode(word[0]);
            var code2 = GetCharCode(word[1]);
            return code1[..2] + code2[..2];
        }

        if (word.Length == 3)
        {
            var code1 = GetCharCode(word[0]);
            var code2 = GetCharCode(word[1]);
            var code3 = GetCharCode(word[2]);
            return $"{code1[0]}{code2[0]}{code3[..2]}";
        }

        // 四字及以上
        var c1 = GetCharCode(word[0]);
        var c2 = GetCharCode(word[1]);
        var c3 = GetCharCode(word[2]);
        var c4 = GetCharCode(word[^1]);
        return $"{c1[0]}{c2[0]}{c3[0]}{c4[0]}";
    }

    private string GetCharCode(char c)
    {
        return GetWubiCode(_codeTable.GetCode(c));
    }
}
