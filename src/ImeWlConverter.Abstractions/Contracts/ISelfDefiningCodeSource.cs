namespace ImeWlConverter.Abstractions.Contracts;

/// <summary>
/// 自定义码（UserDefine）码表数据源：从外部码表文件加载 字 → 编码列表 映射。
/// 由管道组合使用，管道本身不直接接触文件系统（可 mock、可测）。
/// </summary>
public interface ISelfDefiningCodeSource
{
    /// <summary>加载自定义码表文件，返回 字 → 该字的所有编码。</summary>
    Task<IReadOnlyDictionary<char, List<string>>> LoadAsync(string codeFilePath, CancellationToken ct = default);
}
