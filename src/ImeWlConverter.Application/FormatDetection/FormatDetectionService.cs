using ImeWlConverter.Abstractions.Contracts;

namespace ImeWlConverter.Application.FormatDetection;

/// <summary>
/// 输入词库格式识别服务。三端共用（此前 Win/macOS 各自维护一份映射表且已发散——
/// macOS 端曾因复制漂移导致 6 处 ID 失配，参见 Phase 1 Bug 修复）。
/// 识别顺序：① 已知别名表（二进制格式的导入扩展名与导出扩展名不同，
/// 不能依赖 Metadata.FileExtension——那是导出默认扩展名）；② 格式元数据扩展名。
/// </summary>
public sealed class FormatDetectionService
{
    /// <summary>导入扩展名 → 格式 ID 别名表（与各格式 Importer 的实际识别能力对应）。</summary>
    private static readonly IReadOnlyDictionary<string, string> ExtensionAliases = new Dictionary<string, string>
    {
        [".scel"] = "scel",
        [".qcel"] = "qcel",
        [".qpyd"] = "qpyd",
        [".uwl"] = "uwl",
        [".bin"] = "sgpybin",
        [".dat"] = "win10mspy",
        [".bcd"] = "bcd",
        [".bdict"] = "bdict",
        [".ld2"] = "ld2",
        [".zip"] = "gboard",
        [".mb"] = "jdmb",
    };

    private readonly IReadOnlyList<IFormatImporter> _importers;

    public FormatDetectionService(IEnumerable<IFormatImporter> importers)
    {
        _importers = importers.ToList();
    }

    /// <summary>按文件扩展名识别格式 ID；无法识别返回 null。</summary>
    public string? DetectByExtension(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) return null;

        // ① 别名表（别名必须指向真实存在的格式才返回）
        if (ExtensionAliases.TryGetValue(ext, out var aliasId)
            && _importers.Any(i => i.Metadata.Id == aliasId))
            return aliasId;

        // ② 导出元数据扩展名兜底
        return _importers
            .FirstOrDefault(i => string.Equals(i.Metadata.FileExtension, ext, StringComparison.OrdinalIgnoreCase))
            ?.Metadata.Id;
    }
}
