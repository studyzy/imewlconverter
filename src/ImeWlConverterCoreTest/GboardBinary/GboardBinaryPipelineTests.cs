#nullable enable
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Application.Requests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.GboardBinary;

/// <summary>
/// 端到端回归：sgpy(GBK) → gboardbin 经真实管道。
/// 锁定词面/拼音段逐字对齐：若源文件编码错误（如 UTF-8 被按 GBK 读），
/// 词面变成乱码字数与拼音段数不齐，gboardbin 导出会全部拒绝。
/// </summary>
public class GboardBinaryPipelineTests
{
    [Fact]
    public async Task Pipeline_SgpyToGboardbin_ExportsEntries()
    {
        var dir = Path.Combine(Path.GetTempPath(), "gboardbin-repro");
        Directory.CreateDirectory(dir);
        var src = Path.Combine(dir, "src-sgpy-gbk.txt");
        var dst = Path.Combine(dir, "out.dict");
        // 搜狗 txt 格式真身是 GBK 编码（SougouPinyinImporter.FileEncoding 硬编码 GBK）
        await File.WriteAllBytesAsync(
            src,
            Encoding.GetEncoding("GBK").GetBytes("'ni'hao \u4f60\u597d\r\n'shi'jie \u4e16\u754c\r\n"));

        using var sp = ImeWlConverterBootstrapper.CreateServiceProvider();
        var pipeline = sp.GetRequiredService<IConversionPipeline>();

        var factoryResult = ConversionRequestFactory.Create(
            new CliConversionOptions
            {
                InputFormatId = "sgpy",
                OutputFormatId = "gboardbin",
                CodeType = "pinyin",
                InputFiles = [src],
                OutputPath = dst,
            },
            sp.GetServices<IFormatImporter>().ToList(),
            sp.GetServices<IFormatExporter>().ToList());

        Assert.True(factoryResult.IsSuccess);
        var result = await pipeline.ExecuteAsync(factoryResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.ExportedCount);
        Assert.True(File.Exists(dst) && new FileInfo(dst).Length > 0);
    }
}
