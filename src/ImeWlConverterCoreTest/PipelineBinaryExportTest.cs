#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Abstractions.Results;
using ImeWlConverter.Core.Pipeline;
using Xunit;

namespace ImeWlConverterCoreTest;

public class PipelineBinaryExportTest
{
    [Fact]
    public async Task ExecuteAsync_KeepsBinaryOutputAsBytes()
    {
        var inputPath = Path.GetTempFileName();
        try
        {
            var expected = new byte[] { 0x53, 0x47, 0x50, 0x55, 0xA0, 0x80, 0xFF };
            var pipeline = new ConversionPipeline(
                [new TestImporter()],
                [new TestBinaryExporter(expected)]);
            using var output = new MemoryStream();

            var result = await pipeline.ExecuteAsync(new ConversionRequest
            {
                InputFormatId = "test-input",
                OutputFormatId = "test-binary",
                InputPaths = [inputPath],
                OutputStream = output
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Value.ExportData);
            Assert.Null(result.Value.ExportContent);
        }
        finally
        {
            File.Delete(inputPath);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReportsExporterEntryCount()
    {
        var inputPath = Path.GetTempFileName();
        try
        {
            var pipeline = new ConversionPipeline(
                [new TestImporter()],
                [new TestBinaryExporter([0x00], entryCount: 0)]);
            using var output = new MemoryStream();

            var result = await pipeline.ExecuteAsync(new ConversionRequest
            {
                InputFormatId = "test-input",
                OutputFormatId = "test-binary",
                InputPaths = [inputPath],
                OutputStream = output
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value.ExportedCount);
            Assert.Equal(1, result.Value.FilteredCount);
        }
        finally
        {
            File.Delete(inputPath);
        }
    }

    private sealed class TestImporter : IFormatImporter
    {
        public FormatMetadata Metadata { get; } = new("test-input", "Test input", 0, true, false);

        public Task<ImportResult> ImportAsync(Stream input, ImportOptions? options = null, CancellationToken ct = default) =>
            Task.FromResult(new ImportResult
            {
                Entries = [new WordEntry { Word = "test" }]
            });
    }

    private sealed class TestBinaryExporter(byte[] data, int? entryCount = null) : IFormatExporter
    {
        public FormatMetadata Metadata { get; } = new("test-binary", "Test binary", 0, false, true, true, ".bin");

        public async Task<ExportResult> ExportAsync(
            IReadOnlyList<WordEntry> entries,
            Stream output,
            ExportOptions? options = null,
            CancellationToken ct = default)
        {
            await output.WriteAsync(data, ct);
            return new ExportResult { EntryCount = entryCount ?? entries.Count };
        }
    }
}
