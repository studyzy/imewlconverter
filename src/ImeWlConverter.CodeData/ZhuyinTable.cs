using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ImeWlConverter.CodeData;

/// <summary>
/// IZhuyinTable 默认实现：从 Zhuyin.txt（注音\t拼音）惰性构建双向只读映射。
/// </summary>
public sealed partial class ZhuyinTable : IZhuyinTable
{
    [GeneratedRegex(@"^[a-zA-Z]+\d$")]
    private static partial Regex ToneSuffixRegex();

    private readonly IResourceProvider _resources;
    private readonly Lazy<IReadOnlyDictionary<string, string>> _zhuyinDict;
    private readonly Lazy<IReadOnlyDictionary<string, string>> _pinyinDict;

    public ZhuyinTable(IResourceProvider resources)
    {
        _resources = resources;
        _zhuyinDict = new Lazy<IReadOnlyDictionary<string, string>>(
            () => LoadZhuyinToPinyin(_resources.GetResourceContent("Zhuyin.txt")),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _pinyinDict = new Lazy<IReadOnlyDictionary<string, string>>(
            () => LoadPinyinToZhuyin(_resources.GetResourceContent("Zhuyin.txt")),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string? GetZhuyin(string pinyin)
    {
        if (string.IsNullOrEmpty(pinyin)) throw new ArgumentException("找不到拼音", nameof(pinyin));
        var yindiao = 10;
        if (ToneSuffixRegex().IsMatch(pinyin))
        {
            yindiao = Convert.ToInt32(pinyin[^1].ToString());
            pinyin = pinyin[..^1];
        }

        if (!_zhuyinDict.Value.TryGetValue(pinyin, out var baseZhuyin))
        {
            Debug.WriteLine("Can not find zhuyin by pinyin=" + pinyin);
            return null;
        }

        return baseZhuyin + GetToneMark(yindiao);
    }

    public string? GetPinyin(string zhuyin)
    {
        var lastChar = zhuyin[^1];
        var yindiao = GetToneNumber(lastChar);
        if (yindiao != 1) zhuyin = zhuyin[..^1];
        if (_pinyinDict.Value.TryGetValue(zhuyin, out var pinyin)) return pinyin;
        Debug.WriteLine("can not fine the pinyin of zhuyin:" + zhuyin);
        return null;
    }

    internal static IReadOnlyDictionary<string, string> LoadZhuyinToPinyin(string content)
    {
        var dict = new Dictionary<string, string>();
        foreach (var line in content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var arr = line.Split('\t');
            var zhuyinCode = arr[0];
            var pinyin = arr[1];

            if (!dict.ContainsKey(pinyin))
                dict.Add(pinyin, zhuyinCode);
            else
                Debug.WriteLine(pinyin + " mapping more than 1 zhuyin");
        }

        return dict;
    }

    internal static IReadOnlyDictionary<string, string> LoadPinyinToZhuyin(string content)
    {
        var dict = new Dictionary<string, string>();
        foreach (var line in content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var arr = line.Split('\t');
            var zhuyinCode = arr[0];
            var pinyin = arr[1];

            if (!dict.ContainsKey(zhuyinCode))
                dict.Add(zhuyinCode, pinyin);
            else
                Debug.WriteLine(pinyin + " mapping more than 1 pinyin");
        }

        return dict;
    }

    private static string GetToneMark(int yindiao) => yindiao switch
    {
        1 => "",
        2 => "ˊ",
        3 => "ˇ",
        4 => "ˋ",
        5 => "·",
        _ => ""
    };

    private static int GetToneNumber(char yindiao) => yindiao switch
    {
        'ˊ' => 2,
        'ˇ' => 3,
        'ˋ' => 4,
        '·' => 5,
        _ => 1
    };
}
