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

    /// <summary>
    /// 文本内容嗅探：读取前几行按已知文本词库格式的样式匹配（迁移自 WinForms MainForm，逻辑原样）。
    /// 二进制格式无法嗅探，仅覆盖纯文本格式。
    /// </summary>
    public string? DetectByContent(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return null;

            var encoding = Core.Helpers.FileOperationHelper.GetEncodingType(filePath);
            string? example = null;
            using (var sr = new StreamReader(filePath, encoding))
            {
                for (var i = 0; i < 5; i++)
                {
                    example = sr.ReadLine();
                    if (example == null) break;
                }
            }

            if (string.IsNullOrEmpty(example)) return null;

            // 搜狗拼音txt: 'ni'hao 你好
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^('[a-z]+)+\s[\u4E00-\u9FA5]+$"))
                return "sgpy";
            // FIT输入法: ni'hao,你好
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^([a-z]+')+[a-z]+\,[\u4E00-\u9FA5]+$"))
                return "fit";
            // QQ拼音: ni'hao 你好 123
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^[a-z']+\s[\u4E00-\u9FA5]+\s\d+$"))
                return "qqpy";
            // 拼音加加: 你ni好hao
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^([\u4E00-\u9FA5]+[a-z]+)+([\u4E00-\u9FA5]+[a-z]*)*$"))
                return "pyjj";
            // 华宇紫光拼音: 你好\tni'hao\t100
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^[\u4E00-\u9FA5]+\t[a-z']+\t\d+$"))
                return "zgpy";
            // 谷歌拼音: 你好\t100ni hao
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^[\u4E00-\u9FA5]+\t\d+[a-z\s]+$"))
                return "ggpy";
            // 百度手机: 你好 ni|hao 100
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^[\u4E00-\u9FA5]+\s[a-z\|]+\s\d+$"))
                return "bdsj";
            // 极点五笔: abcd 你好
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^[a-z]{1,4}\s[\u4E00-\u9FA5]+$"))
                return "jd";
            // 新浪拼音: nihao 你好
            if (System.Text.RegularExpressions.Regex.IsMatch(example, @"^[a-z']+\s[\u4E00-\u9FA5]+$"))
                return "xlpy";

            return null;
        }
        catch
        {
            return null;
        }
    }
}
