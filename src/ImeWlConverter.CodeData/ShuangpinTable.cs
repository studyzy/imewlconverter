namespace ImeWlConverter.CodeData;

/// <summary>
/// 双拼映射表实现：惰性解析 Shuangpin.txt，构建完成后只读、线程安全。
/// 表头第一列为「全拼」，其余每列为一个双拼方案；不能按枚举序号取列
/// （当前 PinyinType 枚举顺序与表头列并不对应），一律按方案名索引。
/// </summary>
public sealed class ShuangpinTable : IShuangpinTable
{
    private sealed record ParsedTable(
        IReadOnlyList<string> Schemes,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Mappings);

    private readonly Lazy<ParsedTable> _parsed;

    public ShuangpinTable(IResourceProvider resources)
    {
        _parsed = new Lazy<ParsedTable>(
            () => Load(resources),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IReadOnlyList<string> Schemes => _parsed.Value.Schemes;

    public IReadOnlyDictionary<string, string> GetMapping(string schemeName)
    {
        if (!_parsed.Value.Mappings.TryGetValue(schemeName, out var mapping))
            throw new ArgumentException(
                $"未知双拼方案「{schemeName}」，可用方案：{string.Join("、", Schemes)}",
                nameof(schemeName));
        return mapping;
    }

    private static ParsedTable Load(IResourceProvider resources)
    {
        var lines = resources.GetResourceContent("Shuangpin.txt")
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            throw new InvalidOperationException("Shuangpin.txt 为空");

        var header = lines[0].Split('\t');
        var schemes = header.Skip(1).ToArray();

        var mutable = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var scheme in schemes)
            mutable[scheme] = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split('\t');
            if (columns.Length < header.Length)
                continue; // 残缺行跳过，避免取到越界列

            var syllable = columns[0];
            for (var i = 1; i < header.Length; i++)
                mutable[header[i]][syllable] = columns[i];
        }

        var mappings = mutable.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyDictionary<string, string>)kv.Value,
            StringComparer.Ordinal);

        return new ParsedTable(schemes, mappings);
    }
}
