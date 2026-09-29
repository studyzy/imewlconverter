using System.Reflection;

namespace ImeWlConverter.CodeData;

/// <summary>
/// 嵌入式码表资源只读访问器（无状态，线程安全）。
/// </summary>
public sealed class EmbeddedResourceProvider : IResourceProvider
{
    public string GetResourceContent(string fileName)
    {
        var assembly = typeof(EmbeddedResourceProvider).Assembly;

        using var stream = assembly.GetManifestResourceStream(
            "ImeWlConverter.CodeData.Resources." + fileName
        );
        if (stream == null)
            throw new InvalidOperationException($"Embedded resource not found: {fileName}");

        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
