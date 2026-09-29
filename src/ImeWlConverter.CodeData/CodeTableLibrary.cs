namespace ImeWlConverter.CodeData;

/// <summary>
/// ICodeTableLibrary 默认实现：从 ChineseCode.txt 惰性构建只读字表。
/// Lazy(ExecutionAndPublication) 保证并发下只构建一次，构建完成后字典只读。
/// </summary>
public sealed class CodeTableLibrary : ICodeTableLibrary
{
    private readonly Lazy<IReadOnlyDictionary<char, ChineseCode>> _dictionary;

    public CodeTableLibrary(IResourceProvider resources)
    {
        _dictionary = new Lazy<IReadOnlyDictionary<char, ChineseCode>>(
            () => Load(resources.GetResourceContent("ChineseCode.txt")),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public ChineseCode GetCode(char c)
    {
        if (_dictionary.Value.TryGetValue(c, out var code))
            return code;
        throw new KeyNotFoundException("给定关键字不在字典中，【" + c + "】");
    }

    public IReadOnlyCollection<ChineseCode> GetAll() => _dictionary.Value.Values.ToList();

    internal static IReadOnlyDictionary<char, ChineseCode> Load(string content)
    {
        var dict = new Dictionary<char, ChineseCode>();
        var lines = content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var hzpy = line.Split('\t');
            var hz = Convert.ToChar(hzpy[1]);

            dict.Add(hz, new ChineseCode(
                Code: hzpy[0],
                Word: hzpy[1][0],
                Wubi86: hzpy[2],
                Wubi98: hzpy[3],
                WubiNewAge: hzpy[4],
                Pinyins: hzpy[5],
                Freq: Convert.ToDouble(hzpy[6])
            ));
        }

        return dict;
    }
}
