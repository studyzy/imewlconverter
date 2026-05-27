using System.Text;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.CodeGeneration.Generators;
using ImeWlConverter.Core.Filters;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// Orchestrates the complete conversion pipeline:
/// Import → Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty → Export.
/// Shared across CLI, WinForms GUI, and Mac GUI.
/// </summary>
public sealed class ConversionPipeline : IConversionPipeline
{
    private readonly IEnumerable<IFormatImporter> _importers;
    private readonly IEnumerable<IFormatExporter> _exporters;
    private readonly IChineseConverter? _chineseConverter;
    private readonly IWordRankGenerator? _wordRankGenerator;
    private readonly CodeGenerationService? _codeGenerationService;

    public ConversionPipeline(
        IEnumerable<IFormatImporter> importers,
        IEnumerable<IFormatExporter> exporters,
        IChineseConverter? chineseConverter = null,
        IWordRankGenerator? wordRankGenerator = null,
        CodeGenerationService? codeGenerationService = null)
    {
        _importers = importers;
        _exporters = exporters;
        _chineseConverter = chineseConverter;
        _wordRankGenerator = wordRankGenerator;
        _codeGenerationService = codeGenerationService;
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

        // Build filter pipeline from request config
        var filterPipeline = BuildFilterPipeline(request.FilterConfig);

        if (request.MergeToOneFile)
        {
            return await ExecuteMergedAsync(
                request, importer, exporter, filterPipeline, progress, ct);
        }
        else
        {
            return await ExecutePerFileAsync(
                request, importer, exporter, filterPipeline, progress, ct);
        }
    }

    private async Task<Result<ConversionResult>> ExecuteMergedAsync(
        ConversionRequest request,
        IFormatImporter importer,
        IFormatExporter exporter,
        FilterPipeline? filterPipeline,
        IProgress<ProgressInfo>? progress,
        CancellationToken ct)
    {
        var errors = new StringBuilder();
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
                errors.AppendLine($"导入 {files[i]} 失败: {ex.Message}");
            }
        }

        var importedCount = allEntries.Count;

        // Phase 2: Filter
        ct.ThrowIfCancellationRequested();
        progress?.Report(new ProgressInfo(0, importedCount, "正在过滤..."));
        IReadOnlyList<WordEntry> entries = filterPipeline is not null
            ? filterPipeline.Apply(allEntries)
            : allEntries;

        // Phase 3: Chinese conversion
        entries = ApplyChineseConversion(entries, request.Options.ChineseConversion);

        // Phase 4: Word rank generation
        if (_wordRankGenerator is not null)
        {
            ct.ThrowIfCancellationRequested();
            progress?.Report(new ProgressInfo(0, entries.Count, "正在生成词频..."));
            entries = await _wordRankGenerator.GenerateRanksAsync(entries, ct);
        }

        // Phase 5: Code generation
        entries = ApplyCodeGeneration(entries, request.Options.CodeGeneration, progress);

        // Phase 6: Remove entries with empty code (when code generation was requested)
        if (_codeGenerationService is not null && request.Options.CodeGeneration.TargetCodeType != CodeType.NoCode)
        {
            entries = entries.Where(e => e.Code is not null && e.Code.Segments.Count > 0).ToList();
        }

        var exportedCount = entries.Count;
        var filteredCount = importedCount - exportedCount;

        // Phase 7: Export
        ct.ThrowIfCancellationRequested();
        progress?.Report(new ProgressInfo(exportedCount, exportedCount, $"正在导出 {exportedCount} 条词条..."));

        string? exportContent = null;

        if (request.OutputStream is not null)
        {
            // GUI mode: write to provided stream, capture content
            await exporter.ExportAsync(entries, request.OutputStream, request.Options.Export, ct);
            request.OutputStream.Position = 0;
            using var reader = new StreamReader(request.OutputStream, exporter.OutputEncoding, leaveOpen: true);
            exportContent = await reader.ReadToEndAsync(ct);
        }
        else if (request.OutputPath is not null)
        {
            // CLI/file mode: write directly to file
            using var outputStream = File.Create(request.OutputPath);
            await exporter.ExportAsync(entries, outputStream, request.Options.Export, ct);
        }

        var errorStr = errors.Length > 0 ? errors.ToString() : null;

        return Result<ConversionResult>.Success(new ConversionResult
        {
            ImportedCount = importedCount,
            ExportedCount = exportedCount,
            FilteredCount = filteredCount,
            ExportContent = exportContent,
            ErrorMessages = errorStr
        });
    }

    private async Task<Result<ConversionResult>> ExecutePerFileAsync(
        ConversionRequest request,
        IFormatImporter importer,
        IFormatExporter exporter,
        FilterPipeline? filterPipeline,
        IProgress<ProgressInfo>? progress,
        CancellationToken ct)
    {
        var errors = new StringBuilder();
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

                IReadOnlyList<WordEntry> fileEntries = filterPipeline is not null
                    ? filterPipeline.Apply(importResult.Entries.ToList())
                    : importResult.Entries.ToList();

                fileEntries = ApplyChineseConversion(fileEntries, request.Options.ChineseConversion);

                if (_wordRankGenerator is not null)
                    fileEntries = await _wordRankGenerator.GenerateRanksAsync(fileEntries, ct);

                fileEntries = ApplyCodeGeneration(fileEntries, request.Options.CodeGeneration, progress);

                if (_codeGenerationService is not null && request.Options.CodeGeneration.TargetCodeType != CodeType.NoCode)
                    fileEntries = fileEntries.Where(e => e.Code is not null && e.Code.Segments.Count > 0).ToList();

                var outputFile = Path.Combine(
                    request.OutputDirectory ?? ".",
                    Path.GetFileNameWithoutExtension(file) + ".txt");
                using var outStream = File.Create(outputFile);
                await exporter.ExportAsync(fileEntries, outStream, request.Options.Export, ct);

                totalConverted += fileEntries.Count;
                progress?.Report(new ProgressInfo(i + 1, totalFiles, $"已导出: {outputFile}"));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                errors.AppendLine($"处理 {file} 失败: {ex.Message}");
            }
        }

        var errorStr = errors.Length > 0 ? errors.ToString() : null;

        return Result<ConversionResult>.Success(new ConversionResult
        {
            ImportedCount = totalImported,
            ExportedCount = totalConverted,
            FilteredCount = totalImported - totalConverted,
            ErrorMessages = errorStr
        });
    }

    private IReadOnlyList<WordEntry> ApplyChineseConversion(
        IReadOnlyList<WordEntry> entries, ChineseConversionMode mode)
    {
        if (_chineseConverter is null || mode == ChineseConversionMode.None)
            return entries;

        var result = new List<WordEntry>(entries.Count);
        foreach (var entry in entries)
        {
            var converted = mode switch
            {
                ChineseConversionMode.SimplifiedToTraditional =>
                    entry with { Word = _chineseConverter.ToTraditional(entry.Word) },
                ChineseConversionMode.TraditionalToSimplified =>
                    entry with { Word = _chineseConverter.ToSimplified(entry.Word) },
                _ => entry
            };
            result.Add(converted);
        }

        return result;
    }

    private IReadOnlyList<WordEntry> ApplyCodeGeneration(
        IReadOnlyList<WordEntry> entries, CodeGenerationOptions options,
        IProgress<ProgressInfo>? progress)
    {
        if (options.TargetCodeType == CodeType.NoCode)
            return entries;

        // UserDefine 类型需要动态构建 SelfDefiningCodeGenerator
        if (options.TargetCodeType == CodeType.UserDefine && !string.IsNullOrEmpty(options.CodeFilePath))
        {
            progress?.Report(new ProgressInfo(0, entries.Count, "正在生成自定义编码..."));
            var generator = BuildSelfDefiningCodeGenerator(options);
            var result = new List<WordEntry>(entries.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                var code = generator.GenerateCode(entries[i].Word);
                result.Add(entries[i] with { Code = code, CodeType = CodeType.UserDefine });
                progress?.Report(new ProgressInfo(i + 1, entries.Count, "正在生成自定义编码..."));
            }
            return result;
        }

        if (_codeGenerationService is null)
            return entries;

        progress?.Report(new ProgressInfo(0, entries.Count, "正在生成编码..."));
        return _codeGenerationService.GenerateCodes(entries, options.TargetCodeType, progress);
    }

    private static SelfDefiningCodeGenerator BuildSelfDefiningCodeGenerator(CodeGenerationOptions options)
    {
        var dict = UserCodingHelper.GetCodingDict(options.CodeFilePath!, Encoding.UTF8);
        var formatStr = options.MultiCodeFormat?.Replace(',', '\n') ?? "";
        return new SelfDefiningCodeGenerator
        {
            MappingDictionary = dict,
            MutiWordCodeFormat = formatStr,
            Is1Char1Code = false
        };
    }

    /// <summary>
    /// Build a FilterPipeline from FilterConfig.
    /// </summary>
    private static FilterPipeline? BuildFilterPipeline(FilterConfig? config)
    {
        if (config is null || config.NoFilter) return null;

        var filters = new List<IWordFilter>();
        var transforms = new List<IWordTransform>();
        var batchFilters = new List<IBatchFilter>();

        // Single-entry filters
        if (config.IgnoreEnglish) filters.Add(new EnglishFilter());
        if (config.IgnoreFirstCJK) filters.Add(new FirstCJKFilter());
        if (config.WordLengthFrom > 1 || config.WordLengthTo < 9999)
            filters.Add(new LengthFilter { MinLength = config.WordLengthFrom, MaxLength = config.WordLengthTo });
        if (config.WordRankFrom > 1 || config.WordRankTo < 999999)
            filters.Add(new RankFilter { MinRank = config.WordRankFrom, MaxRank = config.WordRankTo });
        if (config.IgnoreSpace) filters.Add(new SpaceFilter());
        if (config.IgnorePunctuation)
        {
            filters.Add(new ChinesePunctuationFilter());
            filters.Add(new EnglishPunctuationFilter());
        }
        if (config.IgnoreNumber) filters.Add(new NumberFilter());
        if (config.IgnoreNoAlphabetCode) filters.Add(new NoAlphabetCodeFilter());

        // Transforms
        if (config.ReplaceEnglish) transforms.Add(new EnglishRemoveTransform());
        if (config.ReplacePunctuation)
        {
            transforms.Add(new EnglishPunctuationRemoveTransform());
            transforms.Add(new ChinesePunctuationRemoveTransform());
        }
        if (config.ReplaceSpace) transforms.Add(new SpaceRemoveTransform());
        if (config.ReplaceNumber) transforms.Add(new NumberRemoveTransform());

        // Batch filters
        if (config.WordRankPercentage < 100)
            batchFilters.Add(new RankPercentageFilter { Percentage = config.WordRankPercentage });

        return filters.Count == 0 && transforms.Count == 0 && batchFilters.Count == 0
            ? null
            : new FilterPipeline(filters, transforms, batchFilters);
    }
}
