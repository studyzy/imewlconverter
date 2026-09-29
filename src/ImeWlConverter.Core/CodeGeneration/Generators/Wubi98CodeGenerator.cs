using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// Wubi98CodeGenerator 五笔98版编码生成器。
/// </summary>
public sealed class Wubi98CodeGenerator : WubiCodeGeneratorBase
{
    public Wubi98CodeGenerator(ICodeTableLibrary codeTable) : base(codeTable) { }

    public override CodeType SupportedType => CodeType.Wubi98;

    protected override string GetWubiCode(ChineseCode code) => code.Wubi98;
}
