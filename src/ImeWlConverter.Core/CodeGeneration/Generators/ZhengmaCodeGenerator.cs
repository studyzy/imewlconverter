using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// ZhengmaCodeGenerator 郑码编码生成器。
/// </summary>
public sealed class ZhengmaCodeGenerator : ICodeGenerator
{
    public CodeType SupportedType => CodeType.Zhengma;

    public bool Is1Char1Code => false;

    private Dictionary<char, ZhengmaEntry>? _dictionary;

    private Dictionary<char, ZhengmaEntry> Dict
    {
        get
        {
            if (_dictionary == null)
            {
                var txt = DictionaryHelper.GetResourceContent("Zhengma.txt");
                _dictionary = new Dictionary<char, ZhengmaEntry>();

                foreach (var line in txt.Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries))
                {
                    var arr = line.Split('\t');
                    if (arr[0].Length == 0) continue;

                    var word = arr[0][0];
                    var shortCode = arr[1].Trim();
                    var codes = new List<string>();
                    for (var i = 1; i < arr.Length; i++)
                    {
                        var code = arr[i].Trim();
                        if (code != "") codes.Add(code);
                    }

                    _dictionary[word] = new ZhengmaEntry(shortCode, codes);
                }
            }

            return _dictionary;
        }
    }

    public WordCode GenerateCode(string word)
    {
        if (string.IsNullOrEmpty(word))
            return new WordCode { Segments = [] };

        foreach (var c in word)
        {
            if (!Dict.ContainsKey(c))
                return new WordCode { Segments = [] };
        }

        if (word.Length == 1)
        {
            // 单字返回所有编码
            var codes = Dict[word[0]].Codes;
            return new WordCode
            {
                Segments = [codes.ToArray()]
            };
        }

        string result;
        if (word.Length == 2)
        {
            // 二字词：2+2 取构词码
            result = Get2Code(word[0]) + Get2Code(word[1]);
        }
        else if (word.Length == 3)
        {
            // 三字词：1+2+1
            result = Get1Code(word[0]) + Get2Code(word[1]) + Get1Code(word[2]);
        }
        else
        {
            // 四字及以上：1+1+1+1（取前4字）
            result = Get1Code(word[0]) + Get1Code(word[1]) + Get1Code(word[2]) + Get1Code(word[3]);
        }

        return new WordCode
        {
            Segments = [new[] { result }]
        };
    }

    private string Get2Code(char c) => Dict[c].ShortCode;

    private string Get1Code(char c) => Dict[c].ShortCode[0].ToString();

    private readonly record struct ZhengmaEntry(string ShortCode, List<string> Codes);
}
