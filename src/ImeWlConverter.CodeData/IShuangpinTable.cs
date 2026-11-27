namespace ImeWlConverter.CodeData;

/// <summary>
/// 双拼方案映射表：全拼音节 → 双拼码。数据来自嵌入式 Shuangpin.txt。
/// </summary>
public interface IShuangpinTable
{
    /// <summary>可用的双拼方案名（Shuangpin.txt 表头去掉「全拼」列）。</summary>
    IReadOnlyList<string> Schemes { get; }

    /// <summary>取指定方案的映射；未知方案抛出 <see cref="ArgumentException"/>。</summary>
    IReadOnlyDictionary<string, string> GetMapping(string schemeName);
}
