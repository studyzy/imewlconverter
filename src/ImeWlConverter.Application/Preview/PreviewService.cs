using System.Text;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Results;

namespace ImeWlConverter.Application.Preview;

/// <summary>
/// GUI 预览服务：Stream 输出的转换结果 → 文本预览（超长截断）/ 二进制摘要。
/// 文案与截断口径与 WinForms GUI 现状逐字节一致（迁移自 MainForm.HandleConversionCompleted），
/// 供三端复用、消除复制漂移。
/// </summary>
public sealed class PreviewService
{
    /// <summary>触发"只显示首末 10 万字"的长度阈值。</summary>
    public const int TruncateThreshold = 200_000;

    /// <summary>截断时保留的首/末长度。</summary>
    public const int TruncateSegmentLength = 100_000;

    public PreviewService(IConversionPipeline pipeline)
    {
        Pipeline = pipeline;
    }

    private IConversionPipeline Pipeline { get; }

    /// <summary>执行转换并以 Stream 输出，返回转换结果（供调用方做预览展示）。</summary>
    public async Task<Result<ConversionResult>> PreviewAsync(
        ConversionRequest request,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken ct = default)
    {
        using var output = new MemoryStream();
        return await Pipeline.ExecuteAsync(request with { OutputStream = output }, progress, ct);
    }

    /// <summary>
    /// 构建文本预览。showLessOnly 为 GUI"结果只显示首、末10万字"选项；
    /// 与 GUI 现状一致：开启且超长时首末各 10 万字 + 固定提示文案。
    /// </summary>
    public static string BuildTextPreview(string content, bool showLessOnly)
    {
        if (showLessOnly && content.Length > TruncateThreshold)
        {
            return "为避免输出时卡死，\u201c高级设置\u201d中选中了\u201c结果只显示首、末10万字\u201d，本文本框中不显示转换后的全部结果，若要查看转换后的结果再确定是否保存请取消该设置。\n\n"
                   + content.Substring(0, TruncateSegmentLength)
                   + "\n\n\n...\n\n\n"
                   + content.Substring(content.Length - TruncateSegmentLength);
        }

        return content;
    }

    /// <summary>
    /// 构建二进制格式摘要（词条数/文件大小/默认文件名），与 GUI 现状逐字节一致。
    /// </summary>
    public static string BuildBinarySummary(
        string? formatDisplayName, int entryCount, byte[] data, string? defaultFileName)
    {
        var sb = new StringBuilder();
        sb.AppendLine((formatDisplayName ?? "二进制词库") + "已生成。");
        sb.AppendLine();
        sb.AppendLine("  词条数    " + entryCount.ToString("N0"));
        sb.AppendLine("  文件大小  " + data.Length.ToString("N0") + " 字节 (" +
                      (data.Length / 1024.0 / 1024.0).ToString("F2") + " MB)");
        if (!string.IsNullOrEmpty(defaultFileName))
            sb.AppendLine("  文件名    " + defaultFileName);
        return sb.ToString();
    }
}
