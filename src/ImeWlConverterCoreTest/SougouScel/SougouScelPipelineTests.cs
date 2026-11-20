#nullable enable
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Application.Bootstrap;
using ImeWlConverter.Formats.SougouScel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.SougouScel;

/// <summary>
/// issue #433 回归：GUI 预览路径（OutputStream）导出 scel 必须保留二进制字节。
/// 若 Metadata.IsBinary 为 false，管道会把二进制流按文本读出，GUI 保存成带 BOM 的
/// UTF-8 文本（非法字节变 U+FFFD），输入法提示“不是支持的 SCEL 词库”。
/// </summary>
public class SougouScelPipelineTests
{
    [Fact]
    public void ExporterMetadata_IsBinary()
    {
        var exporter = new SougouScelExporter();

        Assert.True(exporter.Metadata.IsBinary);
        Assert.Equal(".scel", exporter.Metadata.FileExtension);
    }

    [Fact]
    public async Task Pipeline_StreamOutput_ProducesBinaryScelBytes()
    {
        var src = Path.GetTempFileName();
        try
        {
            // 搜狗拼音 txt 源为 GBK，与 GUI 中“搜狗拼音txt”输入格式一致
            await File.WriteAllBytesAsync(
                src,
                Encoding.GetEncoding("GBK").GetBytes(
                    "'bi'si'mai'zhuang'jia 俾斯麦装甲\r\n'lu'xing'niao 陆行鸟\r\n"));

            using var sp = ImeWlConverterBootstrapper.CreateServiceProvider();
            var pipeline = sp.GetRequiredService<IConversionPipeline>();
            using var output = new MemoryStream();

            var result = await pipeline.ExecuteAsync(new ConversionRequest
            {
                InputFormatId = "sgpy",
                OutputFormatId = "scel",
                InputPaths = [src],
                OutputStream = output
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.ExportedCount);
            Assert.Null(result.Value.ExportContent);
            Assert.NotNull(result.Value.ExportData);
            // scel 文件签名 40 15 00 00 44 43 53 01，前 3 字节不能是 UTF-8 BOM (EF BB BF)
            Assert.Equal(
                new byte[] { 0x40, 0x15, 0x00, 0x00, 0x44, 0x43, 0x53, 0x01 },
                result.Value.ExportData!.Take(8).ToArray());
        }
        finally
        {
            File.Delete(src);
        }
    }
}
