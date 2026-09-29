using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// ErbiCodeGeneratorBase 二笔编码生成器基类，封装二笔系的公共取码逻辑。
/// 二笔编码规则：
/// 单字：取拼音首字母 + 二笔码表中的形码
/// 二字词：取每个字的前两位编码（拼音首字母+形码）
/// 三字词：取第一字的前二位编码和后两个字的第一码
/// 四字词：取每个字的第一码
/// 多字词：取前三字和最后一字的第一码（前三末一）
/// 码表经 Lazy(ExecutionAndPublication) 惰性构建，构建后只读，无静态可变状态。
/// </summary>
public abstract class ErbiCodeGeneratorBase : ICodeGenerator
{
    private readonly IPinyinTable _pinyinTable;
    private readonly Lazy<Dictionary<char, IList<string>>> _erbiDic;

    protected ErbiCodeGeneratorBase(IPinyinTable pinyinTable, IResourceProvider resources)
    {
        _pinyinTable = pinyinTable;
        _erbiDic = new Lazy<Dictionary<char, IList<string>>>(
            () => Load(resources.GetResourceContent("Erbi.txt"), DicColumnIndex),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public abstract CodeType SupportedType { get; }

    public bool Is1Char1Code => false;

    /// <summary>
    /// Erbi.txt 中的列索引：1=现代二笔，2=音形，3=超强二笔，4=青松二笔
    /// </summary>
    protected abstract int DicColumnIndex { get; }

    protected Dictionary<char, IList<string>> ErbiDic => _erbiDic.Value;

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
            return new WordCode { Segments = [] };

        try
        {
            var pinyins = _pinyinTable.GetDefaultPinyin(word);
            var codes = GetErbiCode(word, pinyins);
            if (codes == null || codes.Count == 0)
                return new WordCode { Segments = [] };

            var result = CollectionHelper.Descartes(codes);
            // 二笔是一词一码，所有变体在一个 segment 中
            return new WordCode
            {
                Segments = [result.ToArray()]
            };
        }
        catch
        {
            return new WordCode { Segments = [] };
        }
    }

    /// <summary>
    /// 获取一个词的二笔编码组合（可能多音字产生多种组合）。
    /// </summary>
    protected virtual IList<IList<string>>? GetErbiCode(string str, IList<string> py)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var codes = new List<IList<string>>();

        if (str.Length == 1)
        {
            codes.Add(Get1CharCode(str[0], py[0]));
        }
        else if (str.Length == 2)
        {
            codes.Add(Get1CharCode(str[0], py[0]));
            codes.Add(Get1CharCode(str[1], py[1]));
        }
        else if (str.Length == 3)
        {
            codes.Add(Get1CharCode(str[0], py[0]));
            codes.Add(new List<string> { py[1][0].ToString() });
            codes.Add(new List<string> { py[2][0].ToString() });
        }
        else
        {
            // 四字及以上：前三末一的第一码
            codes.Add(new List<string>
            {
                py[0][0].ToString() + py[1][0] + py[2][0] + py[str.Length - 1][0]
            });
        }

        return codes;
    }

    /// <summary>
    /// 获得一个字的二笔码（拼音首字母 + 形码）。
    /// </summary>
    protected IList<string> Get1CharCode(char c, string py)
    {
        var result = new List<string>();
        if (!ErbiDic.TryGetValue(c, out var codes))
            return [py[0].ToString()];

        foreach (var code in codes)
            result.Add(py[0].ToString() + code[0]);

        return result;
    }

    internal static Dictionary<char, IList<string>> Load(string txt, int dicColumnIndex)
    {
        var erbiDic = new Dictionary<char, IList<string>>();

        foreach (var line in txt.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.RemoveEmptyEntries))
        {
            var arr = line.Split('\t');
            if (arr[0].Length == 0) continue;

            var word = arr[0][0];
            var code = dicColumnIndex < arr.Length ? arr[dicColumnIndex] : "";
            if (string.IsNullOrEmpty(code))
                code = arr.Length > 1 ? arr[1] : "";
            if (string.IsNullOrEmpty(code)) continue;

            var codes = code.Split(' ');
            erbiDic[word] = new List<string>(codes);
        }

        return erbiDic;
    }
}
