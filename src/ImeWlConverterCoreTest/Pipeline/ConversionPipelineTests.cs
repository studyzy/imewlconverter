#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Core.CodeGeneration;
using ImeWlConverter.Core.Pipeline;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Pipeline;

/// <summary>
/// ConversionPipeline 表征测试（characterization tests）：
/// 在重构管道前固化当前行为（合并模式/逐文件模式/Stream 输出/部分失败/空编码移除）。
/// 重构后这些测试必须全部保持绿。
/// 注：逐文件模式 RemoveEmpty 采用与合并模式统一的严格谓词（Phase 1 Bug 修复，此前两条路径不一致）。
/// </summary>
public class ConversionPipelineTests : IDisposable
{
    private readonly string _tempDir;

    public ConversionPipelineTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "imewl-pipeline-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* 清理失败不影响测试结果 */ }
    }

    // ---------------------------------------------------------------- 合并模式

    [Fact]
    public async Task MergedMode_TwoInputs_ProduceSingleMergedOutput()
    {
        var f1 = CreateInputFile("a.txt", "alpha");
        var f2 = CreateInputFile("b.txt", "beta");
        var pipeline = CreatePipeline();

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [f1, f2],
            OutputPath = Path.Combine(_tempDir, "out.txt"),
            MergeToOneFile = true,
        });

        Assert.True(result.IsSuccess, result.IsSuccess ? string.Empty : result.Error);
        var r = result.Value;
        Assert.Equal(6, r.ImportedCount);   // 每个文件 3 条
        Assert.Equal(6, r.ExportedCount);
        Assert.Equal(0, r.FilteredCount);
        Assert.Null(r.ErrorMessages);
        Assert.Null(r.ExportContent);       // 文件模式不填充预览内容

        var content = File.ReadAllText(Path.Combine(_tempDir, "out.txt"));
        Assert.Equal("alpha1|1\nalpha2|2\nalpha3|3\nbeta1|1\nbeta2|2\nbeta3|3\n", content);
    }

    [Fact]
    public async Task MergedMode_UnknownInputFormat_ReturnsFailure()
    {
        var f1 = CreateInputFile("a.txt", "alpha");
        var pipeline = CreatePipeline();

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = "no-such-format",
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [f1],
            OutputPath = Path.Combine(_tempDir, "out.txt"),
        });

        Assert.True(result.IsFailure);
        Assert.Contains("no-such-format", result.Error);
    }

    [Fact]
    public async Task MergedMode_PartialFileFailure_ContinuesAndAccumulatesErrors()
    {
        var good = CreateInputFile("good.txt", "alpha");
        var boom = CreateInputFile("boom.txt", "boom"); // FakeImporter 对 "boom" 标记抛异常
        var pipeline = CreatePipeline();

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [good, boom],
            OutputPath = Path.Combine(_tempDir, "out.txt"),
            MergeToOneFile = true,
        });

        // 部分文件失败不导致整体失败，错误累积到 ErrorMessages
        Assert.True(result.IsSuccess);
        var r = result.Value;
        Assert.NotNull(r.ErrorMessages);
        Assert.Contains("boom.txt", r.ErrorMessages);
        Assert.Contains("模拟导入失败", r.ErrorMessages);
        Assert.Equal(3, r.ImportedCount);   // 只有 good.txt 成功导入
        Assert.Equal(3, r.ExportedCount);
    }

    // ---------------------------------------------------------------- 逐文件模式

    [Fact]
    public async Task PerFileMode_TwoInputs_ProduceOneOutputPerInput()
    {
        var f1 = CreateInputFile("a.txt", "alpha");
        var f2 = CreateInputFile("b.txt", "beta");
        var pipeline = CreatePipeline();

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [f1, f2],
            OutputDirectory = _tempDir,
            MergeToOneFile = false,
        });

        Assert.True(result.IsSuccess, result.IsSuccess ? string.Empty : result.Error);
        var r = result.Value;
        Assert.Equal(6, r.ImportedCount);
        Assert.Equal(6, r.ExportedCount);
        Assert.Null(r.ErrorMessages);

        // 每个输入文件对应一个输出文件，扩展名取自导出格式元数据
        var outA = Path.Combine(_tempDir, "a.fake");
        var outB = Path.Combine(_tempDir, "b.fake");
        Assert.True(File.Exists(outA), $"缺少输出文件 {outA}");
        Assert.True(File.Exists(outB), $"缺少输出文件 {outB}");
        Assert.Equal("alpha1|1\nalpha2|2\nalpha3|3\n", File.ReadAllText(outA));
        Assert.Equal("beta1|1\nbeta2|2\nbeta3|3\n", File.ReadAllText(outB));
    }

    [Fact]
    public async Task PerFileMode_PartialFileFailure_OtherFilesStillExported()
    {
        var good = CreateInputFile("good.txt", "alpha");
        var boom = CreateInputFile("boom.txt", "boom");
        var pipeline = CreatePipeline();

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [good, boom],
            OutputDirectory = _tempDir,
            MergeToOneFile = false,
        });

        Assert.True(result.IsSuccess);
        var r = result.Value;
        Assert.NotNull(r.ErrorMessages);
        Assert.Contains("boom.txt", r.ErrorMessages);
        Assert.True(File.Exists(Path.Combine(_tempDir, "good.fake")));
        Assert.False(File.Exists(Path.Combine(_tempDir, "boom.fake")));
    }

    // ---------------------------------------------------------------- Stream 输出（GUI 预览）

    [Fact]
    public async Task StreamOutput_PopulatesExportContentForTextFormats()
    {
        var f1 = CreateInputFile("a.txt", "alpha");
        var pipeline = CreatePipeline();
        using var ms = new MemoryStream();

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [f1],
            OutputStream = ms,
            MergeToOneFile = true,
        });

        Assert.True(result.IsSuccess);
        var r = result.Value;
        Assert.Equal(3, r.ExportedCount);
        Assert.Equal("alpha1|1\nalpha2|2\nalpha3|3\n", r.ExportContent);
        Assert.Null(r.ExportData);          // 文本格式不填充二进制数据
        // 管道为回读内容将流回卷到 0，ReadToEnd 后位置停在末尾（表征当前行为）
        Assert.Equal(ms.Length, ms.Position);
    }

    // ---------------------------------------------------------------- RemoveEmpty（空编码移除）

    [Fact]
    public async Task MergedMode_RemoveEmptyCode_UsesStrictPredicate()
    {
        // 四种词条形态：
        //   good: Segments=[["ni"],["hao"]]                       → 保留
        //   bad1: Segments=[]（无任何 segment）                    → 移除
        //   bad2: Segments=[[]]（segment 为空列表）                → 移除
        //   bad3: Segments=[["",""]]（segment 内全是空串）         → 移除
        var f1 = CreateInputFile("case.txt", "removecase");
        var pipeline = CreatePipeline(
            importer: new FakeImporter(RemoveCaseFactory),
            codeGenerator: CreateRemoveCaseGenerator());

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [f1],
            OutputPath = Path.Combine(_tempDir, "out.txt"),
            MergeToOneFile = true,
            Options = new ConversionOptions
            {
                CodeGeneration = new CodeGenerationOptions { TargetCodeType = CodeType.Pinyin },
            },
        });

        Assert.True(result.IsSuccess);
        var r = result.Value;
        Assert.Equal(4, r.ImportedCount);
        Assert.Equal(1, r.ExportedCount);
        var content = File.ReadAllText(Path.Combine(_tempDir, "out.txt"));
        Assert.Equal("good|1\n", content);
    }

    [Fact]
    public async Task PerFileMode_RemoveEmptyCode_UsesSameStrictPredicateAsMerged()
    {
        // Bug 修复固化：此前逐文件路径只检查 Segments.Count > 0（宽松），
        // 与合并路径的严格谓词不一致，bad2/bad3 这类"有 segment 但无有效编码"的词条会被错误保留。
        var f1 = CreateInputFile("case.txt", "removecase");
        var pipeline = CreatePipeline(
            importer: new FakeImporter(RemoveCaseFactory),
            codeGenerator: CreateRemoveCaseGenerator());

        var result = await pipeline.ExecuteAsync(new ConversionRequest
        {
            InputFormatId = FakeImporter.FormatId,
            OutputFormatId = FakeExporter.FormatId,
            InputPaths = [f1],
            OutputDirectory = _tempDir,
            MergeToOneFile = false,
            Options = new ConversionOptions
            {
                CodeGeneration = new CodeGenerationOptions { TargetCodeType = CodeType.Pinyin },
            },
        });

        Assert.True(result.IsSuccess);
        var r = result.Value;
        Assert.Equal(4, r.ImportedCount);
        Assert.Equal(1, r.ExportedCount);
        Assert.Equal("good|1\n", File.ReadAllText(Path.Combine(_tempDir, "case.fake")));
    }

    // ---------------------------------------------------------------- 测试替身

    private string CreateInputFile(string fileName, string token)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, token);
        return path;
    }

    private static ConversionPipeline CreatePipeline(
        FakeImporter? importer = null,
        FakeExporter? exporter = null,
        ICodeGenerator? codeGenerator = null)
    {
        var generators = codeGenerator is null ? [] : new[] { codeGenerator };
        return new ConversionPipeline(
            new IFormatImporter[] { importer ?? new FakeImporter() },
            new IFormatExporter[] { exporter ?? new FakeExporter() },
            codeGenerationService: new CodeGenerationService(generators));
    }

    /// <summary>removecase 词条工厂：good 有码 + 三种"空编码"形态，Rank 依次 1-4。</summary>
    private static IReadOnlyList<WordEntry> RemoveCaseFactory(string token) =>
    [
        new() { Word = "good", Rank = 1 },
        new() { Word = "bad1", Rank = 2 },
        new() { Word = "bad2", Rank = 3 },
        new() { Word = "bad3", Rank = 4 },
    ];

    /// <summary>构造 removecase 场景的编码生成器：good 有码，bad1/bad2/bad3 分别代表三种"空编码"形态。</summary>
    private static ICodeGenerator CreateRemoveCaseGenerator()
    {
        IReadOnlyList<IReadOnlyList<string>> Empty() => [];
        var map = new Dictionary<string, IReadOnlyList<IReadOnlyList<string>>>
        {
            ["good"] = [["ni"], ["hao"]],
            ["bad1"] = Empty(),
            ["bad2"] = [[]],
            ["bad3"] = [["", ""]],
        };
        return new FakeCodeGenerator(map);
    }

    /// <summary>假导入器：文件内容首行为标记 token，按 token 生成词条；token 为 "boom" 时抛异常模拟导入失败。</summary>
    internal sealed class FakeImporter(Func<string, IReadOnlyList<WordEntry>>? factory = null) : IFormatImporter
    {
        public const string FormatId = "fakein";

        public FormatMetadata Metadata { get; } = new(FormatId, "假导入格式", 1, true, false);

        public Task<ImportResult> ImportAsync(Stream input, ImportOptions? options = null, CancellationToken ct = default)
        {
            using var reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);
            var token = reader.ReadLine() ?? string.Empty;
            if (token == "boom")
                throw new IOException("模拟导入失败");

            var entries = factory is not null ? factory(token) : DefaultFactory(token);
            return Task.FromResult(new ImportResult { Entries = entries });
        }

        private static IReadOnlyList<WordEntry> DefaultFactory(string token) =>
            Enumerable.Range(1, 3)
                .Select(i => new WordEntry { Word = $"{token}{i}", Rank = i })
                .ToList();
    }

    /// <summary>假导出器：每条词条写一行 "word|rank\n"。</summary>
    internal sealed class FakeExporter : IFormatExporter
    {
        public const string FormatId = "fakeout";

        public FormatMetadata Metadata { get; } = new(FormatId, "假导出格式", 1, false, true, FileExtension: ".fake");

        public Task<ExportResult> ExportAsync(
            IReadOnlyList<WordEntry> entries, Stream output, ExportOptions? options = null, CancellationToken ct = default)
        {
            using var writer = new StreamWriter(output, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
            foreach (var e in entries)
                writer.Write($"{e.Word}|{e.Rank}\n");
            return Task.FromResult(new ExportResult { EntryCount = entries.Count });
        }
    }

    /// <summary>假编码生成器：按字典映射生成编码，未命中的词返回单段 "x"。</summary>
    private sealed class FakeCodeGenerator(IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<string>>> map) : ICodeGenerator
    {
        public CodeType SupportedType => CodeType.Pinyin;
        public bool Is1Char1Code => false;

        public WordCode GenerateCode(string word) => new()
        {
            Segments = map.TryGetValue(word, out var segments)
                ? segments
                : [["x"]],
        };
    }
}
