using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// XiandaiErbiCodeGenerator 现代二笔编码生成器。
/// 现代二笔与普通二笔不同的是，组词时每个字取2码，并没有4码长度的限制。
/// </summary>
public sealed class XiandaiErbiCodeGenerator : ErbiCodeGeneratorBase
{
    public XiandaiErbiCodeGenerator(IPinyinTable pinyinTable, IResourceProvider resources)
        : base(pinyinTable, resources) { }

    public override CodeType SupportedType => CodeType.XiandaiErbi;

    protected override int DicColumnIndex => 1;

    protected override IList<IList<string>>? GetErbiCode(string str, IList<string> py)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var codes = new List<IList<string>>();

        for (var i = 0; i < str.Length; i++)
        {
            codes.Add(Get1CharCode(str[i], py[i]));
        }

        return codes;
    }
}
