using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Results;

namespace ImeWlConverter.Application.Preview;

/// <summary>预览结果：文本内容或二进制摘要（二选一）。</summary>
public sealed record PreviewResult
{
    public string? TextPreview { get; init; }
    public string? BinarySummary { get; init; }
}

/// <summary>
/// GUI 预览服务：Stream 输出的转换结果 → 截断文本预览 / 二进制摘要。
/// 吸收 WinForms 与 macOS GUI 各自复制的预览截断逻辑（此前两端口径还不一致）。
/// </summary>
public sealed class PreviewService
{
    /// <summary>预览最大字符数；超长时取首尾各一半拼接省略标记。</summary>
    public const int MaxPreviewChars = 200_000;

    public PreviewService(IConversionPipeline pipeline)
    {
        Pipeline = pipeline;
    }

    private IConversionPipeline Pipeline { get; }

    /// <summary>执行转换并以 Stream 输出，返回预览结果。</summary>
    public async Task<Result<PreviewResult>> PreviewAsync(
        ConversionRequest request,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken ct = default)
    {
        using var output = new MemoryStream();
        var result = await Pipeline.ExecuteAsync(request with { OutputStream = output }, progress, ct);
        if (result.IsFailure)
            return Result<PreviewResult>.Failure(result.Error);

        var value = result.Value;
        if (value.ExportData is not null)
        {
            return Result<PreviewResult>.Success(new PreviewResult
            {
                BinarySummary = BuildBinarySummary(value.ExportData, value.ExportedCount),
            });
        }

        return Result<PreviewResult>.Success(new PreviewResult
        {
            TextPreview = Truncate(value.ExportContent ?? string.Empty),
        });
    }

    /// <summary>文本截断：超过上限时保留首尾各一半，中间以省略提示衔接。</summary>
    public static string Truncate(string content)
    {
        if (content.Length <= MaxPreviewChars)
            return content;

        var half = MaxPreviewChars / 2;
        var omitted = content.Length - MaxPreviewChars;
        return string.Concat(
            content.AsSpan(0, half),
            $"\r\n\r\n……（中间省略 {omitted:N0} 字，请保存后查看完整内容）\r\n\r\n",
            content.AsSpan(^half));
    }

    /// <summary>二进制格式摘要：导出条数 + 头 64 字节的十六进制，便于用户确认产物。</summary>
    public static string BuildBinarySummary(byte[] data, int entryCount)
    {
        const int headBytes = 64;
        var hex = new System.Text.StringBuilder(headBytes * 3);
        var take = Math.Min(headBytes, data.Length);
        for (var i = 0; i < take; i++)
        {
            if (i > 0) hex.Append(' ');
            hex.Append(data[i].ToString("X2"));
        }

        return $"二进制格式已生成 {entryCount} 条词条，共 {data.Length:N0} 字节。\r\n" +
               $"文件头（前 {take} 字节）:\r\n{hex}";
    }
}
