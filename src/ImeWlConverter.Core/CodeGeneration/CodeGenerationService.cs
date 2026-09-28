using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;

namespace ImeWlConverter.Core.CodeGeneration;

/// <summary>
/// Coordinates code generation by delegating to the appropriate <see cref="ICodeGenerator"/>
/// based on the target code type.
/// </summary>
public sealed class CodeGenerationService
{
    private readonly IReadOnlyDictionary<CodeType, ICodeGenerator> _generators;

    /// <summary>
    /// Initializes a new instance of <see cref="CodeGenerationService"/>.
    /// </summary>
    /// <param name="generators">Available code generators, keyed by their supported type.</param>
    public CodeGenerationService(IEnumerable<ICodeGenerator> generators)
    {
        _generators = generators.ToDictionary(g => g.SupportedType);
    }

    /// <summary>
    /// Generate code for a word entry using the specified target code type.
    /// Returns the entry unchanged if it already has the target code type or no generator is available.
    /// </summary>
    /// <param name="entry">The word entry to generate code for.</param>
    /// <param name="targetCodeType">The desired code type.</param>
    /// <returns>A new entry with the generated code, or the original entry if unchanged.</returns>
    public WordEntry GenerateCode(WordEntry entry, CodeType targetCodeType)
    {
        if (entry.CodeType == targetCodeType && entry.Code is not null && entry.Code.Segments.Count > 0)
            return entry;

        if (!_generators.TryGetValue(targetCodeType, out var generator))
            return entry;

        var code = generator.GenerateCode(entry.Word);
        return entry with { Code = code, CodeType = targetCodeType };
    }

    /// <summary>
    /// Generate codes for a batch of entries.
    /// </summary>
    /// <param name="entries">The entries to process.</param>
    /// <param name="targetCodeType">The desired code type.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>A new list of entries with generated codes.</returns>
    public IReadOnlyList<WordEntry> GenerateCodes(
        IReadOnlyList<WordEntry> entries,
        CodeType targetCodeType,
        IProgress<ProgressInfo>? progress = null)
    {
        var result = new List<WordEntry>(entries.Count);

        // 进度按 ~1% 节流上报：GUI 端 Progress<T> 每次上报都会跨线程封送，
        // 逐条上报会让 UI 消息队列被淹没，成为大词库转换的主要瓶颈。
        var reportInterval = Math.Max(1, entries.Count / 100);

        for (var i = 0; i < entries.Count; i++)
        {
            result.Add(GenerateCode(entries[i], targetCodeType));
            if (progress is not null && (i % reportInterval == 0 || i == entries.Count - 1))
                progress.Report(new ProgressInfo(i + 1, entries.Count, "正在生成编码..."));
        }

        return result;
    }

    /// <summary>Check if a code type has a registered generator.</summary>
    /// <param name="codeType">The code type to check.</param>
    /// <returns>True if a generator is available for the given code type.</returns>
    public bool IsSupported(CodeType codeType) => _generators.ContainsKey(codeType);
}
