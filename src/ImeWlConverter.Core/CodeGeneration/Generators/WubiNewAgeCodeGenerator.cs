using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// WubiNewAgeCodeGenerator 新世纪五笔编码生成器。
/// </summary>
public sealed class WubiNewAgeCodeGenerator : WubiCodeGeneratorBase
{
    public WubiNewAgeCodeGenerator(ICodeTableLibrary codeTable) : base(codeTable) { }

    public override CodeType SupportedType => CodeType.WubiNewAge;

    protected override string GetWubiCode(ChineseCode code) => code.WubiNewAge;
}
