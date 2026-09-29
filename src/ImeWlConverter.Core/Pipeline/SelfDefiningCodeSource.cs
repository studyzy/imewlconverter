using System.Text;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// ISelfDefiningCodeSource 默认实现：包装 UserCodingHelper 读取自定义码表文件。
/// 使管道本体不依赖文件系统，便于测试时替换。
/// </summary>
public sealed class SelfDefiningCodeSource : ISelfDefiningCodeSource
{
    public Task<IReadOnlyDictionary<char, List<string>>> LoadAsync(string codeFilePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var dict = UserCodingHelper.GetCodingDict(codeFilePath, Encoding.UTF8);
        var result = new Dictionary<char, List<string>>(dict.Count);
        foreach (var kv in dict)
            result[kv.Key] = kv.Value.ToList();
        return Task.FromResult((IReadOnlyDictionary<char, List<string>>)result);
    }
}
