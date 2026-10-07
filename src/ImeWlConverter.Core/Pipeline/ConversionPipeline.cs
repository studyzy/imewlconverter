using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// Orchestrates the complete conversion pipeline:
/// Import → Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty → Export.
/// Shared across CLI, WinForms GUI, and Mac GUI.
/// 词条中间阶段的处理逻辑在 <see cref="EntryTransformationService"/> 中（合并/逐文件两条路径共用），
/// 过滤器装配在 <see cref="FilterPipelineFactory"/> 中（模块注册制）。
/// </summary>
public sealed class ConversionPipeline : IConversionPipeline
{
    private readonly IEnumerable<IFormatImporter> _importers;
    private readonly IEnumerable<IFormatExporter> _exporters;
    private readonly FilterPipelineFactory _filterPipelineFactory;
    private readonly EntryTransformationService _transformationService;

    public ConversionPipeline(
        IEnumerable<IFormatImporter> importers,
        IEnumerable<IFormatExporter> exporters,
        FilterPipelineFactory filterPipelineFactory,
        EntryTransformationService transformationService)
    {
        _importers = importers;
        _exporters = exporters;
        _filterPipelineFactory = filterPipelineFactory;
        _transformationService = transformationService;
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionResult>> ExecuteAsync(
        ConversionRequest request,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken ct = default)
    {
        // 1. Find importer/exporter by format ID
        var importer = _importers.FirstOrDefault(i => i.Metadata.Id == request.InputFormatId);
        if (importer is null)
            return Result<ConversionResult>.Failure($"Unknown input format: {request.InputFormatId}");

        var exporter = _exporters.FirstOrDefault(e => e.Metadata.Id == request.OutputFormatId);
        if (exporter is null)
            return Result<ConversionResult>.Failure($"Unknown output format: {request.OutputFormatId}");

        var filterPipeline = _filterPipelineFactory.Create(request.FilterConfig);

        return request.MergeToOneFile
            ? await ExecuteMergedAsync(request, importer, exporter, filterPipeline, progress, ct)
            : await ExecutePerFileAsync(request, importer, exporter, filterPipeline, progress, ct);
    }

    private async Task<Result<ConversionResult>> ExecuteMergedAsync(
        ConversionRequest request,
        IFormatImporter importer,
        IFormatExporter exporter,
        FilterPipeline? filterPipeline,
        IProgress<ProgressInfo>? progress,
        CancellationToken ct)
    {
        var errors = new List<ConversionError>();
        var files = request.InputPaths;
        var totalFiles = files.Count;

        // Phase 1: Import all files
        var allEntries = new List<WordEntry>();
        for (var i = 0; i < totalFiles; i++)
        {
            ct.ThrowIfCancellationRequested();
            var fileName = Path.GetFileName(files[i]);
            progress?.Report(new ProgressInfo(i + 1, totalFiles, $"正在导入文件 {i + 1}/{totalFiles}: {fileName}"));

            try
            {
                using var stream = File.OpenRead(files[i]);
                var importResult = await importer.ImportAsync(stream, request.Options.Import, ct);
                allEntries.AddRange(importResult.Entries);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                errors.Add(RecordError(files[i], ex));
            }
        }

        var importedCount = allEntries.Count;

        // Phase 2-6: Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty
        var entries = await _transformationService.ApplyAsync(
            allEntries, request.Options, filterPipeline, progress, ct);

        var exportedCount = entries.Count;
        var filteredCount = importedCount - exportedCount;

        // Phase 7: Export
        ct.ThrowIfCancellationRequested();
        progress?.Report(new ProgressInfo(exportedCount, exportedCount, $"正在导出 {exportedCount} 条词条..."));

        string? exportContent = null;
        byte[]? exportData = null;

        // 源文件名注入（未显式设置时）：供 .qj 等需要以源词库命名的导出格式使用
        var exportOptions = WithSourceFileName(
            request.Options.Export, files.Count > 0 ? Path.GetFileName(files[0]) : null);

        if (request.OutputStream is not null)
        {
            // GUI mode: write to provided stream and retain the exact output for saving.
            var exportResult = await exporter.ExportAsync(entries, request.OutputStream, exportOptions, ct);
            exportedCount = exportResult.EntryCount;
            filteredCount = importedCount - exportedCount;
            if (request.OutputStream.CanSeek && request.OutputStream.CanRead)
            {
                request.OutputStream.Position = 0;
                if (exporter.Metadata.IsBinary)
                {
                    using var copy = new MemoryStream();
                    await request.OutputStream.CopyToAsync(copy, ct);
                    exportData = copy.ToArray();
                }
                else
                {
                    using var reader = new StreamReader(request.OutputStream, exporter.OutputEncoding, leaveOpen: true);
                    exportContent = await reader.ReadToEndAsync(ct);
                }
            }
        }
        else if (request.OutputPath is not null)
        {
            // CLI/file mode: write directly to file
            using var outputStream = File.Create(request.OutputPath);
            var exportResult = await exporter.ExportAsync(entries, outputStream, exportOptions, ct);
            exportedCount = exportResult.EntryCount;
            filteredCount = importedCount - exportedCount;
        }

        return Result<ConversionResult>.Success(BuildResult(
            importedCount, exportedCount, filteredCount, errors, content: exportContent, data: exportData));
    }

    private async Task<Result<ConversionResult>> ExecutePerFileAsync(
        ConversionRequest request,
        IFormatImporter importer,
        IFormatExporter exporter,
        FilterPipeline? filterPipeline,
        IProgress<ProgressInfo>? progress,
        CancellationToken ct)
    {
        var errors = new List<ConversionError>();
        var files = request.InputPaths;
        var totalFiles = files.Count;
        var totalConverted = 0;
        var totalImported = 0;

        for (var i = 0; i < totalFiles; i++)
        {
            ct.ThrowIfCancellationRequested();
            var file = files[i];
            var fileName = Path.GetFileName(file);
            progress?.Report(new ProgressInfo(i + 1, totalFiles, $"正在处理文件 {i + 1}/{totalFiles}: {fileName}"));

            try
            {
                using var stream = File.OpenRead(file);
                var importResult = await importer.ImportAsync(stream, request.Options.Import, ct);
                totalImported += importResult.Entries.Count;

                // Phase 2-6: Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty
                var fileEntries = await _transformationService.ApplyAsync(
                    importResult.Entries, request.Options, filterPipeline, progress, ct);

                var outputFile = Path.Combine(
                    request.OutputDirectory ?? ".",
                    Path.GetFileNameWithoutExtension(file) + exporter.Metadata.FileExtension);
                using var outStream = File.Create(outputFile);
                var exportResult = await exporter.ExportAsync(
                    fileEntries, outStream, WithSourceFileName(request.Options.Export, fileName), ct);

                totalConverted += exportResult.EntryCount;
                progress?.Report(new ProgressInfo(i + 1, totalFiles, $"已导出: {outputFile}"));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                errors.Add(RecordError(file, ex));
            }
        }

        return Result<ConversionResult>.Success(BuildResult(
            totalImported, totalConverted, totalImported - totalConverted, errors));
    }

    private static ConversionError RecordError(string filePath, Exception ex) =>
        new(filePath, ex.Message, ex.GetType().Name);

    /// <summary>
    /// 未显式设置时把源文件名注入导出选项（record with 表达式复制全部属性，新增字段不会遗漏）。
    /// 显式设置过则原样返回，避免覆盖用户指定值。
    /// </summary>
    private static ExportOptions WithSourceFileName(ExportOptions options, string? sourceFileName)
    {
        if (options.SourceFileName is not null || string.IsNullOrEmpty(sourceFileName))
            return options;

        return options with { SourceFileName = sourceFileName };
    }

    private static ConversionResult BuildResult(
        int importedCount, int exportedCount, int filteredCount,
        IReadOnlyList<ConversionError> errors,
        string? content = null, byte[]? data = null)
    {
        return new ConversionResult
        {
            ImportedCount = importedCount,
            ExportedCount = exportedCount,
            FilteredCount = filteredCount,
            ExportContent = content,
            ExportData = data,
            // ErrorMessages 由 Errors 派生（保持既有消费方兼容），Phase 4 之后各端改用 Errors
            ErrorMessages = errors.Count == 0
                ? null
                : string.Join(Environment.NewLine, errors.Select(e =>
                    $"处理 {e.FilePath} 失败: {e.Message}")),
            Errors = errors
        };
    }
}
