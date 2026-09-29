using System.Diagnostics;
using System.Globalization;

namespace ImeWlConverter.CodeData;

/// <summary>
/// IPinyinTable 默认实现：拼音数据派生自 ChineseCode.txt（经 ICodeTableLibrary）。
/// 全部字典经 Lazy(ExecutionAndPublication) 惰性构建，构建后只读，无静态可变状态。
/// </summary>
public sealed class PinyinTable : IPinyinTable
{
    private readonly ICodeTableLibrary _codeTable;
    private readonly Lazy<IReadOnlyDictionary<char, IReadOnlyList<string>>> _barePinyinDict;
    private readonly Lazy<IReadOnlyDictionary<char, IReadOnlyList<string>>> _tonedPinyinDict;

    public PinyinTable(ICodeTableLibrary codeTable)
    {
        _codeTable = codeTable;
        _barePinyinDict = new Lazy<IReadOnlyDictionary<char, IReadOnlyList<string>>>(
            () => LoadBarePinyin(_codeTable.GetAll()),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _tonedPinyinDict = new Lazy<IReadOnlyDictionary<char, IReadOnlyList<string>>>(
            () => LoadTonedPinyin(_codeTable.GetAll()),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string GetDefaultPinyin(char c)
    {
        if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            return c.ToString().ToLowerInvariant();

        if (c >= '0' && c <= '9')
            return c.ToString();

        if (_barePinyinDict.Value.TryGetValue(c, out var pys) && pys.Count > 0)
            return pys[0];

        throw new KeyNotFoundException($"找不到字:'{c}'的拼音");
    }

    public IList<string> GetDefaultPinyin(string word)
    {
        var result = new List<string>();
        var si = new StringInfo(word);
        for (var i = 0; i < si.LengthInTextElements; i++)
        {
            var textElement = si.SubstringByTextElements(i, 1);
            if (textElement.Length == 1)
            {
                result.Add(GetDefaultPinyin(textElement[0]));
            }
            else
            {
                Debug.WriteLine($"Skipping character beyond BMP: {textElement}");
            }
        }
        return result;
    }

    public IList<string> GetPinyinOfChar(char c) => _barePinyinDict.Value[c].ToList();

    public bool IsMultiPinyinWord(char c) => GetPinyinOfChar(c).Count > 1;

    public string AddTone(char c, string barePinyin)
    {
        if (!_tonedPinyinDict.Value.TryGetValue(c, out var list))
        {
            Debug.WriteLine("找不到" + c + "的拼音,使用其默认拼音对应的音调1");
            return barePinyin + "1";
        }

        foreach (var allpinyin in list)
            foreach (var pinyin in allpinyin.Split(','))
                if (pinyin.Length == barePinyin.Length + 1
                    && pinyin.StartsWith(barePinyin, StringComparison.Ordinal)
                    && pinyin[^1] is >= '0' and <= '5')
                    return pinyin;

        Debug.WriteLine("找不到" + c + "的拼音" + barePinyin + "对应的音调");
        return barePinyin + "1";
    }

    public bool ValidatePinyin(string word, IList<string> pinyin)
    {
        if (word.Length != pinyin.Count) return false;
        for (var i = 0; i < word.Length; i++)
        {
            var charPinyinList = GetPinyinOfChar(word[i]);
            if (!charPinyinList.Contains(pinyin[i])) return false;
        }

        return true;
    }

    /// <summary>字的不含声调拼音字典：从 "py1,py2;..." 形式派生，去音调、去重。</summary>
    private static IReadOnlyDictionary<char, IReadOnlyList<string>> LoadBarePinyin(
        IReadOnlyCollection<ChineseCode> codes)
    {
        var dict = new Dictionary<char, List<string>>();
        foreach (var code in codes)
        {
            var pys = code.Pinyins;
            if (string.IsNullOrEmpty(pys))
                continue;

            foreach (var s in pys.Split(','))
            {
                var py = s.Remove(s.Length - 1); // remove tone
                if (dict.TryGetValue(code.Word, out var list))
                {
                    if (!list.Contains(py)) list.Add(py);
                }
                else
                {
                    dict.Add(code.Word, new List<string> { py });
                }
            }
        }

        return dict.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value.ToList());
    }

    /// <summary>字的带声调拼音字典：Pinyins 按 ';' 分组的原样数据。</summary>
    private static IReadOnlyDictionary<char, IReadOnlyList<string>> LoadTonedPinyin(
        IReadOnlyCollection<ChineseCode> codes)
    {
        var dict = new Dictionary<char, IReadOnlyList<string>>();
        foreach (var code in codes)
        {
            var py = code.Pinyins;
            if (!string.IsNullOrEmpty(py))
                dict.Add(code.Word, py.Split(';'));
        }

        return dict;
    }
}
