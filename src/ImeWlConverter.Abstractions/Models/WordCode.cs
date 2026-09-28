namespace ImeWlConverter.Abstractions.Models;

/// <summary>
/// Represents the encoding/code data for a word.
/// Structure: A list of segments, where each segment contains possible code variations.
/// For pinyin: segments[0] = possible pinyins for char 0, segments[1] = for char 1, etc.
/// </summary>
public sealed record WordCode
{
    /// <summary>The code segments. Each segment is a list of possible codes for that position.</summary>
    public required IReadOnlyList<IReadOnlyList<string>> Segments { get; init; }

    /// <summary>Creates a WordCode from a simple sequence of single codes.</summary>
    public static WordCode FromSingle(IEnumerable<string> codes)
    {
        return new WordCode
        {
            Segments = codes.Select(c => (IReadOnlyList<string>)new[] { c }).ToList()
        };
    }

    /// <summary>Gets the first/primary code string joined by the separator.
    /// 空 segment（如被标点过滤清空）按空串处理，不抛异常。</summary>
    public string GetPrimaryCode(string separator = "'")
    {
        return string.Join(separator, Segments.Select(s => s.Count > 0 ? s[0] : ""));
    }
}
