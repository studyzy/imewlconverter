using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ImeWlConverter.CodeData;

/// <summary>
/// IChaoyinTable 默认实现：从 ChaoyinCodeMapping.txt 惰性构建只读映射。
/// 行格式：拼音\t超音编码\t是否声母Y标记。
/// </summary>
public sealed partial class ChaoyinTable : IChaoyinTable
{
    [GeneratedRegex(@"^[a-zA-Z]+\d$")]
    private static partial Regex ToneSuffixRegex();

    private readonly Lazy<ChaoyinMapping> _mapping;

    public ChaoyinTable(IResourceProvider resources)
    {
        _mapping = new Lazy<ChaoyinMapping>(
            () => Load(resources.GetResourceContent("ChaoyinCodeMapping.txt")),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string? GetChaoyin(string pinyin)
    {
        if (string.IsNullOrEmpty(pinyin)) throw new ArgumentException("找不到拼音", nameof(pinyin));
        var yindiao = 10;
        if (ToneSuffixRegex().IsMatch(pinyin))
        {
            yindiao = Convert.ToInt32(pinyin[^1].ToString());
            pinyin = pinyin[..^1];
        }

        if (!_mapping.Value.CodeByPinyin.TryGetValue(pinyin, out var zy))
        {
            Debug.WriteLine("Can not find Chaoyin code by pinyin=" + pinyin);
            return null;
        }

        Debug.WriteLine("Pinyin:" + pinyin + ",Chaoyin:" + zy);
        return zy;
    }

    public string GetWordChaoyin(IList<string> pinyins)
    {
        var mapping = _mapping.Value.CodeByPinyin;
        var shenmuY = _mapping.Value.ShenmuYPinyins;
        var result = new StringBuilder();

        if (pinyins.Count == 1)
            return GetChaoyin(pinyins[0]) ?? "";

        // 注：与历史实现一致，词模式直接按拼音取码，缺失的拼音会抛 KeyNotFoundException
        if (pinyins.Count == 2)
        {
            result.Append(mapping[pinyins[0]]);
            result.Append(mapping[pinyins[1]]);
            if (shenmuY.Contains(pinyins[1])) result.Append(";");
        }
        else if (pinyins.Count == 3)
        {
            result.Append(mapping[pinyins[0]][0]);
            result.Append(mapping[pinyins[1]][0]);
            result.Append(mapping[pinyins[2]]);
            if (shenmuY.Contains(pinyins[2]))
                result.Append("'");
            else
                result.Append(";");
        }
        else if (pinyins.Count == 4)
        {
            result.Append(mapping[pinyins[0]][0]);
            result.Append(mapping[pinyins[1]][0]);
            result.Append(mapping[pinyins[2]][0]);
            if (shenmuY.Contains(pinyins[3]))
            {
                result.Append(mapping[pinyins[3]][0]);
                result.Append(mapping[pinyins[3]][0]);
            }
            else
            {
                result.Append(mapping[pinyins[3]]);
            }
        }
        else if (pinyins.Count == 5)
        {
            result.Append(mapping[pinyins[0]][0]);
            result.Append(mapping[pinyins[1]][0]);
            result.Append(mapping[pinyins[2]][0]);
            result.Append(mapping[pinyins[3]][0]);
            result.Append(mapping[pinyins[4]][0]);
        }
        else
        {
            result.Append(mapping[pinyins[0]][0]);
            result.Append(mapping[pinyins[1]][0]);
            result.Append(mapping[pinyins[2]][0]);
            result.Append(mapping[pinyins[3]][0]);
            result.Append(mapping[pinyins[^1]][0]);
        }

        return result.ToString();
    }

    internal static ChaoyinMapping Load(string content)
    {
        var codeByPinyin = new Dictionary<string, string>();
        var shenmuY = new HashSet<string>();
        foreach (var line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var array = line.Split('\t');
            codeByPinyin.Add(array[0], array[1]);
            if (array[2] == "Y") shenmuY.Add(array[0]);
        }

        return new ChaoyinMapping(codeByPinyin, shenmuY);
    }

    internal readonly record struct ChaoyinMapping(
        IReadOnlyDictionary<string, string> CodeByPinyin,
        IReadOnlySet<string> ShenmuYPinyins);
}
