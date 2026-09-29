using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// Wubi86CodeGenerator 五笔86版编码生成器。
/// </summary>
public sealed class Wubi86CodeGenerator : WubiCodeGeneratorBase
{
    public Wubi86CodeGenerator(ICodeTableLibrary codeTable) : base(codeTable) { }

    public override CodeType SupportedType => CodeType.Wubi86;

    protected override string GetWubiCode(ChineseCode code) => code.Wubi86;
}
