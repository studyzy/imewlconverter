using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// PinyinCodeGenerator 拼音编码生成器，使用多音字词组注音和贪婪匹配算法。
/// 词组词典经 Lazy(ExecutionAndPublication) 惰性构建，构建后只读，无静态可变状态。
/// </summary>
public sealed class PinyinCodeGenerator : ICodeGenerator
{
    private readonly IPinyinTable _pinyinTable;
    private readonly Lazy<WordPinyinDictionary> _wordPinyin;

    public PinyinCodeGenerator(IPinyinTable pinyinTable, IResourceProvider resources)
    {
        _pinyinTable = pinyinTable;
        _wordPinyin = new Lazy<WordPinyinDictionary>(
            () => LoadWordPinyin(resources.GetResourceContent("WordPinyin.txt")),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public CodeType SupportedType => CodeType.Pinyin;

    public bool Is1Char1Code => true;

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return new WordCode { Segments = Array.Empty<IReadOnlyList<string>>() };
        }

        var pinyinList = IsInWordPinYin(word)
            ? GenerateMutiWordPinYin(word)
            : null;

        var segments = new List<IReadOnlyList<string>>(word.Length);
        for (var i = 0; i < word.Length; i++)
        {
            string py;
            if (pinyinList != null && pinyinList[i] != null)
            {
                py = pinyinList[i]!;
            }
            else
            {
                try
                {
                    py = _pinyinTable.GetDefaultPinyin(word[i]);
                }
                catch
                {
                    py = "";
                }
            }

            segments.Add(new[] { py });
        }

        return new WordCode { Segments = segments };
    }

    private WordPinyinDictionary WordPinyin => _wordPinyin.Value;

    // 词典键是 word 的子串 <=> word 的某个子串命中词典。
    // 对短词只需 O(词长²) 次字典查找，远快于遍历全部词典键做 Contains。
    private bool IsInWordPinYin(string word)
    {
        var dict = WordPinyin.Words;
        for (var len = word.Length; len >= 2; len--)
        {
            for (var start = 0; start + len <= word.Length; start++)
            {
                if (dict.ContainsKey(word.Substring(start, len)))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 贪婪匹配算法生成多音字词组拼音，优先匹配较长的词组，避免重复标注。
    /// </summary>
    private List<string?> GenerateMutiWordPinYin(string word)
    {
        var dict = WordPinyin.Words;
        var pinyin = new string?[word.Length];
        var matched = new bool[word.Length];

        foreach (var key in WordPinyin.SortedKeys)
        {
            var index = 0;
            while ((index = word.IndexOf(key, index, StringComparison.Ordinal)) != -1)
            {
                var canMatch = true;
                for (var i = 0; i < key.Length; i++)
                {
                    if (matched[index + i])
                    {
                        canMatch = false;
                        break;
                    }
                }

                if (canMatch)
                {
                    var pinyinValues = dict[key];
                    for (var i = 0; i < pinyinValues.Count; i++)
                    {
                        pinyin[index + i] = pinyinValues[i];
                        matched[index + i] = true;
                    }
                }

                index++;
            }
        }

        return new List<string?>(pinyin);
    }

    internal static WordPinyinDictionary LoadWordPinyin(string content)
    {
        var wlList = new Dictionary<string, List<string>>();
        var lines = content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Split(' ');
            if (line.Length < 2) continue;

            var py = line[0];
            var wordText = line[1];

            var pinyin = new List<string>(
                py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries)
            );
            wlList.TryAdd(wordText, pinyin);
        }

        return new WordPinyinDictionary(
            wlList,
            wlList.Keys.OrderByDescending(k => k.Length).ToList());
    }

    internal sealed record WordPinyinDictionary(
        IReadOnlyDictionary<string, List<string>> Words,
        IReadOnlyList<string> SortedKeys);
}
