using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.CodeGeneration.Generators;

namespace ImeWlConverter.Core.Pipeline;

/// <summary>
/// 词条级五阶段处理服务：Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty。
/// 合并导出模式调用一次，逐文件导出模式每个文件调用一次——两条路径的阶段逻辑由此归一。
/// </summary>
public sealed class EntryTransformationService
{
    private readonly IChineseConverter? _chineseConverter;
    private readonly IWordRankGenerator? _wordRankGenerator;
    private readonly CodeGenerationService? _codeGenerationService;
    private readonly ISelfDefiningCodeSource? _selfDefiningCodeSource;

    public EntryTransformationService(
        IChineseConverter? chineseConverter = null,
        IWordRankGenerator? wordRankGenerator = null,
        CodeGenerationService? codeGenerationService = null,
        ISelfDefiningCodeSource? selfDefiningCodeSource = null)
    {
        _chineseConverter = chineseConverter;
        _wordRankGenerator = wordRankGenerator;
        _codeGenerationService = codeGenerationService;
        _selfDefiningCodeSource = selfDefiningCodeSource;
    }

    /// <summary>对一组词条依序应用过滤、简繁转换、词频、编码生成与空编码移除。</summary>
    public async Task<IReadOnlyList<WordEntry>> ApplyAsync(
        IReadOnlyList<WordEntry> entries,
        ConversionOptions options,
        FilterPipeline? filters,
        IProgress<ProgressInfo>? progress,
        CancellationToken ct)
    {
        // Phase 2: Filter
        if (filters is not null)
        {
            ct.ThrowIfCancellationRequested();
            progress?.Report(new ProgressInfo(0, entries.Count, "正在过滤..."));
            entries = filters.Apply(entries);
        }

        // Phase 3: Chinese conversion
        entries = ApplyChineseConversion(entries, options.ChineseConversion);

        // Phase 4: Word rank generation
        if (_wordRankGenerator is not null)
        {
            ct.ThrowIfCancellationRequested();
            progress?.Report(new ProgressInfo(0, entries.Count, "正在生成词频..."));
            entries = await _wordRankGenerator.GenerateRanksAsync(entries, ct);
        }

        // Phase 5: Code generation
        entries = await ApplyCodeGeneration(entries, options.CodeGeneration, progress, ct);

        // Phase 6: Remove entries with empty code (when code generation was requested)
        if (_codeGenerationService is not null && options.CodeGeneration.TargetCodeType != CodeType.NoCode)
        {
            entries = entries.Where(CodePredicates.HasValidCode).ToList();
        }

        return entries;
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

    private async Task<IReadOnlyList<WordEntry>> ApplyCodeGeneration(
        IReadOnlyList<WordEntry> entries, CodeGenerationOptions options,
        IProgress<ProgressInfo>? progress, CancellationToken ct)
    {
        if (options.TargetCodeType == CodeType.NoCode)
            return entries;

        // UserDefine 类型需要动态构建 SelfDefiningCodeGenerator
        if (options.TargetCodeType == CodeType.UserDefine && !string.IsNullOrEmpty(options.CodeFilePath))
        {
            progress?.Report(new ProgressInfo(0, entries.Count, "正在生成自定义编码..."));
            var generator = await BuildSelfDefiningCodeGenerator(options, ct);

            // 进度按 ~1% 节流上报，避免 GUI 端逐条封送 UI 消息
            var reportInterval = Math.Max(1, entries.Count / 100);

            var result = new List<WordEntry>(entries.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var code = generator.GenerateCode(entries[i].Word);
                result.Add(entries[i] with { Code = code, CodeType = CodeType.UserDefine });
                if (progress is not null && (i % reportInterval == 0 || i == entries.Count - 1))
                    progress.Report(new ProgressInfo(i + 1, entries.Count, "正在生成自定义编码..."));
            }
            return result;
        }

        if (_codeGenerationService is null)
            return entries;

        progress?.Report(new ProgressInfo(0, entries.Count, "正在生成编码..."));
        var generated = _codeGenerationService.GenerateCodes(entries, options.TargetCodeType, progress);

        return CodeGenerationPostProcessor.Apply(generated, options);
    }

    private async Task<SelfDefiningCodeGenerator> BuildSelfDefiningCodeGenerator(
        CodeGenerationOptions options, CancellationToken ct)
    {
        var source = _selfDefiningCodeSource
            ?? throw new InvalidOperationException(
                "未注册 ISelfDefiningCodeSource，无法处理 UserDefine 编码类型");
        var dict = await source.LoadAsync(options.CodeFilePath!, ct);
        var mapping = new Dictionary<char, IList<string>>(dict.Count);
        foreach (var kv in dict)
            mapping[kv.Key] = kv.Value;
        var formatStr = options.MultiCodeFormat?.Replace(',', '\n') ?? "";
        return new SelfDefiningCodeGenerator
        {
            MappingDictionary = mapping,
            MutiWordCodeFormat = formatStr,
            Is1Char1Code = false
        };
    }
}
