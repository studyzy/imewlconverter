using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// QingsongErbiCodeGenerator 青松二笔编码生成器。
/// </summary>
public sealed class QingsongErbiCodeGenerator : ErbiCodeGeneratorBase
{
    public QingsongErbiCodeGenerator(IPinyinTable pinyinTable, IResourceProvider resources)
        : base(pinyinTable, resources) { }

    public override CodeType SupportedType => CodeType.QingsongErbi;

    protected override int DicColumnIndex => 4;
}
