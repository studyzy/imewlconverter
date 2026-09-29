using ImeWlConverter.CodeData;
using ImeWlConverter.Core.CodeGeneration.Generators;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest;

/// <summary>
/// 测试用码表装配工厂：为直接构造生成器/格式类的测试提供 CodeData 服务实例。
/// </summary>
internal static class TestCodeData
{
    internal static readonly EmbeddedResourceProvider Resources = new();

    internal static readonly CodeTableLibrary CodeTable = new(Resources);

    internal static readonly PinyinTable Pinyin = new(CodeTable);

    internal static readonly ZhuyinTable Zhuyin = new(Resources);

    internal static readonly ChaoyinTable Chaoyin = new(Resources);

    internal static PinyinCodeGenerator CreatePinyinGenerator() => new(Pinyin, Resources);

    internal static TerraPinyinCodeGenerator CreateTerraGenerator() => new(CreatePinyinGenerator(), Pinyin);

    internal static ZhuyinCodeGenerator CreateZhuyinGenerator() => new(CreateTerraGenerator(), Zhuyin);

    internal static ChaoyinCodeGenerator CreateChaoyinGenerator() => new(CreatePinyinGenerator(), Chaoyin);

    internal static ImeWlConverter.Formats.MsPinyin.MsPinyinExporter CreateMsPinyinExporter() => new(Pinyin);
}
