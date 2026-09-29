namespace ImeWlConverter.CodeData;

/// <summary>
/// 嵌入式码表资源只读访问器。
/// 资源名相对于本程序集的 Resources 目录（如 "Zhengma.txt"）。
/// </summary>
public interface IResourceProvider
{
    /// <summary>读取指定嵌入式码表资源的全部文本内容。</summary>
    /// <exception cref="InvalidOperationException">资源不存在时抛出。</exception>
    string GetResourceContent(string fileName);
}
