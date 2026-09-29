using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// ChaoqiangErbiCodeGenerator 超强二笔编码生成器。
/// </summary>
public sealed class ChaoqiangErbiCodeGenerator : ErbiCodeGeneratorBase
{
    public ChaoqiangErbiCodeGenerator(IPinyinTable pinyinTable, IResourceProvider resources)
        : base(pinyinTable, resources) { }

    public override CodeType SupportedType => CodeType.ChaoqiangErbi;

    protected override int DicColumnIndex => 3;
}
