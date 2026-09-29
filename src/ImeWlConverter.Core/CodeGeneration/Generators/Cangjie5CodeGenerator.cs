using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.CodeData;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Core.CodeGeneration.Generators;

/// <summary>
/// Cangjie5CodeGenerator 仓颉五代编码生成器。
/// 码表经 Lazy(ExecutionAndPublication) 惰性构建，构建后只读，无静态可变状态。
/// </summary>
public sealed class Cangjie5CodeGenerator : ICodeGenerator
{
    public CodeType SupportedType => CodeType.Cangjie5;

    public bool Is1Char1Code => false;

    private static readonly Dictionary<char, string> OneCodeChar = new()
    {
        { '日', "a" }, { '月', "b" }, { '金', "c" }, { '木', "d" },
        { '水', "e" }, { '火', "f" }, { '土', "g" }, { '竹', "h" },
        { '戈', "i" }, { '十', "j" }, { '大', "k" }, { '中', "l" },
        { '一', "m" }, { '弓', "n" }, { '人', "o" }, { '心', "p" },
        { '手', "q" }, { '口', "r" }, { '尸', "s" }, { '廿', "t" },
        { '山', "u" }, { '女', "v" }, { '田', "w" }, { '卜', "y" },
        { '曰', "a" }, { '八', "c" }, { '儿', "c" }, { '又', "e" },
        { '小', "f" }, { '士', "g" }, { '广', "i" }, { '厂', "m" },
        { '工', "m" }, { '乙', "n" }, { '入', "o" }, { '匕', "p" },
        { '七', "p" }
    };

    private readonly Lazy<Dictionary<char, IList<CangjieEntry>>> _dictionary;

    public Cangjie5CodeGenerator(IResourceProvider resources)
    {
        _dictionary = new Lazy<Dictionary<char, IList<CangjieEntry>>>(
            () => Load(resources.GetResourceContent("Cangjie5.txt")),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private Dictionary<char, IList<CangjieEntry>> Dict => _dictionary.Value;

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
            var codes = new List<string>();
            foreach (var entry in Dict[word[0]])
                codes.Add(entry.Code);

            return new WordCode
            {
                Segments = [codes.ToArray()]
            };
        }

        IList<IList<string>> codeSegments = new List<IList<string>>();

        if (word.Length == 2)
        {
            // 第一个字2码（首尾码），第二字3码（首次尾码）
            codeSegments.Add(GetFirstAndLastCode(word[0]));
            codeSegments.Add(GetFirstSecondLastCode(word[1]));
        }
        else if (word.Length == 3)
        {
            // 221取码
            codeSegments.Add(GetFirstAndLastCode(word[0]));
            var code2 = GetFirstAndLastCode(word[1]);
            codeSegments.Add(code2);
            if (code2[0].Length == 1)
                // 212取码
                codeSegments.Add(GetFirstAndLastCode(word[2]));
            else
                codeSegments.Add(GetLastCode(word[2]));
        }
        else if (word.Length == 4)
        {
            // 首2字当字首取2码，剩下的2字当字身3码（第3字2码，第4字尾码）
            codeSegments.Add(GetFirstCode(word[0]));
            codeSegments.Add(GetLastCode(word[1]));
            var code3 = GetFirstAndLastCode(word[2]);
            codeSegments.Add(code3);
            if (code3[0].Length == 1)
                codeSegments.Add(GetFirstAndLastCode(word[3]));
            else
                codeSegments.Add(GetLastCode(word[3]));
        }
        else
        {
            // 5字及以上：首2字当字首取2码，剩下的3字当字身3码
            codeSegments.Add(GetFirstCode(word[0]));
            codeSegments.Add(GetLastCode(word[1]));
            codeSegments.Add(GetFirstCode(word[2]));
            codeSegments.Add(GetLastCode(word[^2]));
            codeSegments.Add(GetLastCode(word[^1]));
        }

        var result = CollectionHelper.Descartes(codeSegments);

        return new WordCode
        {
            Segments = [result.ToArray()]
        };
    }

    private IList<string> GetLastCode(char c)
    {
        if (OneCodeChar.TryGetValue(c, out var oneCode))
            return [oneCode];

        var entries = Dict[c];
        var result = new List<string>();
        foreach (var entry in entries)
        {
            if (entry.SplitCode != null)
            {
                var code = GetSplitedCode(entry.SplitCode).ToString();
                if (!result.Contains(code)) result.Add(code);
            }
            else
            {
                var lcode = entry.Code[^1].ToString();
                if (!result.Contains(lcode)) result.Add(lcode);
            }
        }

        return result;
    }

    private IList<string> GetFirstCode(char c)
    {
        if (OneCodeChar.TryGetValue(c, out var oneCode))
            return [oneCode];

        var entries = Dict[c];
        var result = new List<string>();
        foreach (var entry in entries)
        {
            var code = entry.Code[0].ToString();
            if (!result.Contains(code)) result.Add(code);
        }

        return result;
    }

    private IList<string> GetFirstAndLastCode(char c)
    {
        if (OneCodeChar.TryGetValue(c, out var oneCode))
            return [oneCode];

        var entries = Dict[c];
        var result = new List<string>();
        foreach (var entry in entries)
        {
            var firstCode = entry.Code[0];
            var lastCode = entry.Code[^1];

            if (entry.SplitCode != null)
            {
                var arr = entry.SplitCode.Split('\'');
                if (arr[0].Length > 1)
                    lastCode = GetSplitedCode(entry.SplitCode);
            }

            var code = $"{firstCode}{lastCode}";
            if (!result.Contains(code)) result.Add(code);
        }

        return result;
    }

    private IList<string> GetFirstSecondLastCode(char c)
    {
        if (OneCodeChar.TryGetValue(c, out var oneCode))
            return [oneCode];

        var entries = Dict[c];
        var result = new List<string>();
        foreach (var entry in entries)
        {
            if (entry.Code.Length == 1)
            {
                if (!result.Contains(entry.Code)) result.Add(entry.Code);
                continue;
            }

            var code = $"{entry.Code[0]}{entry.Code[1]}";
            if (entry.Code.Length > 2)
            {
                var lastCode = entry.Code[^1];
                if (entry.SplitCode != null)
                {
                    var arr = entry.SplitCode.Split('\'');
                    if (arr[0].Length > 2)
                        lastCode = arr[0][^1];
                }

                code += lastCode;
            }

            if (!result.Contains(code)) result.Add(code);
        }

        return result;
    }

    private static char GetSplitedCode(string splitCode)
    {
        var arr = splitCode.Split('\'');
        return arr[0][^1];
    }

    private static Dictionary<char, IList<CangjieEntry>> Load(string txt)
    {
        var dictionary = new Dictionary<char, IList<CangjieEntry>>();

        foreach (var line in txt.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var arr = line.Split('\t');
            if (arr.Length < 2 || arr[0].Length == 0) continue;

            var word = arr[0][0];
            var entry = new CangjieEntry(arr[1], arr.Length >= 3 ? arr[2] : null);

            if (dictionary.TryGetValue(word, out var list))
                list.Add(entry);
            else
                dictionary[word] = new List<CangjieEntry> { entry };
        }

        return dictionary;
    }

    private readonly record struct CangjieEntry(string Code, string? SplitCode);
}
