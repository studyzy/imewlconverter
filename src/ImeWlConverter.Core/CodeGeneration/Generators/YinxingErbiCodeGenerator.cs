using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// YinxingErbiCodeGenerator 隐形二笔（音形二笔）编码生成器。
/// </summary>
public sealed class YinxingErbiCodeGenerator : ErbiCodeGeneratorBase
{
    public YinxingErbiCodeGenerator(IPinyinTable pinyinTable, IResourceProvider resources)
        : base(pinyinTable, resources) { }

    public override CodeType SupportedType => CodeType.YinxingErbi;

    protected override int DicColumnIndex => 2;
}
