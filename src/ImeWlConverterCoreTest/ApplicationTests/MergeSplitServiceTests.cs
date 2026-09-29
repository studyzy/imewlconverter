#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using ImeWlConverter.Application.MergeSplit;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.Application;

/// <summary>
/// MergeSplitService 表征测试：算法迁移自 WinForms/macOS 双端重复实现，
/// 本测试锁定迁移后的行为（含两处有意统一，见各用例注释）。
/// </summary>
public class MergeSplitServiceTests : IDisposable
{
    private readonly string _tempDir;

    public MergeSplitServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "imewl-mergesplit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* 清理失败不影响测试结果 */ }
    }

    private string WriteFile(string name, string content, Encoding? encoding = null)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, encoding ?? Encoding.UTF8);
        return path;
    }

    // ==================================================================== 合并

    [Fact]
    public void Merge_TwoFiles_MergesEntriesAndDeduplicates()
    {
        // 行格式 "编码 词1 词2"：首个空格前为编码，其后均为词条
        var main = WriteFile("main.txt", "a ni'hao 你好\r\nb shijie 世界\r\n");
        var user = WriteFile("user.txt", "a ni'hao 您好\r\nc zhong 中\r\n");

        var result = MergeSplitService.MergeFiles(main, [user], sortByCode: false);

        // a: 词条 [ni'hao, 你好] + [ni'hao(重复跳过), 您好]；b 保留；c 新增
        Assert.Equal(
            "a ni'hao 你好 您好\r\n" +
            "b shijie 世界\r\n" +
            "c zhong 中\r\n",
            result.Content);
        Assert.Equal(3, result.EntryCount);
    }

    [Fact]
    public void Merge_SortByCode_SortsEntriesByKey()
    {
        var main = WriteFile("main.txt", "b shijie 世界\r\na ni'hao 你好\r\n");
        var user = WriteFile("user.txt", "a ni'hao 您好\r\n");

        var result = MergeSplitService.MergeFiles(main, [user], sortByCode: true);

        var firstBreak = result.Content.IndexOf("\r\n", StringComparison.Ordinal);
        Assert.StartsWith("a ", result.Content);
        Assert.StartsWith("b ", result.Content[(firstBreak + 2)..]);
    }

    [Fact]
    public void Merge_OutputLineEndings_AreCrLf()
    {
        // 统一说明：历史 Mac 版输出 \n，迁移后统一为 \r\n（Windows 原版语义）
        var main = WriteFile("main.txt", "a wo 我\n");
        var result = MergeSplitService.MergeFiles(main, [], sortByCode: false);

        Assert.Contains("\r\n", result.Content);
        Assert.DoesNotContain("\n", result.Content.Replace("\r\n", ""));
    }

    [Fact]
    public void Merge_SkipsEmptyPathEntries()
    {
        var main = WriteFile("main.txt", "a wo 我\r\n");
        // UI 以 " | " 连接多选路径，按 '|' 切分后可能产生空白项
        var result = MergeSplitService.MergeFiles(main, ["", "  "], sortByCode: false);

        Assert.Single(ParseLines(result.Content));
    }

    // ==================================================================== 按行分割

    [Fact]
    public void SplitByLine_CrLfFile_ProducesPartsOfMaxLine()
    {
        var source = WriteFile("lines.txt", "l1\r\nl2\r\nl3\r\nl4\r\nl5\r\n");

        var parts = MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 2 });

        Assert.Equal(3, parts.Count);
        Assert.Equal(Path.Combine(_tempDir, "lines01.txt"), parts[0]);
        Assert.Equal("l1\r\nl2\r\n", File.ReadAllText(parts[0]));
        Assert.Equal("l3\r\nl4\r\n", File.ReadAllText(parts[1]));
        Assert.Equal("l5\r\n", File.ReadAllText(parts[2]));
    }

    [Fact]
    public void SplitByLine_LfOnlyFile_DetectsSeparator()
    {
        var source = WriteFile("lf.txt", "l1\nl2\nl3\n");

        var parts = MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 2 });

        Assert.Equal(2, parts.Count);
        Assert.Equal("l1\nl2\n", File.ReadAllText(parts[0]));
        Assert.Equal("l3\n", File.ReadAllText(parts[1]));
    }

    [Fact]
    public void SplitByLine_SingleLine_ThrowsWithReadableMessage()
    {
        // Bug 修复：此前单行文件静默跳过且界面仍提示"分割完成"（无任何分片），
        // 现改为抛出可读异常，GUI 显示"分割失败: 文件只有 1 行，无需分割"
        var source = WriteFile("single.txt", "only1\r\n");

        var ex = Assert.Throws<InvalidDataException>(() =>
            MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 100 }));
        Assert.Contains("无需分割", ex.Message);
    }

    [Fact]
    public void SplitByLine_EmptyFile_ThrowsWithReadableMessage()
    {
        var source = WriteFile("empty.txt", "");

        var ex = Assert.Throws<InvalidDataException>(() =>
            MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 2 }));
        Assert.Contains("内容为空", ex.Message);
    }

    [Fact]
    public void SplitByLine_MaxLineLessThanOne_ThrowsWithReadableMessage()
    {
        // 历史行为：maxLine=0 会在取模处裸崩 DivideByZeroException，现改为可读校验
        var source = WriteFile("case.txt", "l1\r\nl2\r\n");

        Assert.Throws<InvalidDataException>(() =>
            MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 0 }));
    }

    [Fact]
    public void SplitByLine_MaxLineOne_EachLineItsOwnPart()
    {
        var source = WriteFile("each.txt", "l1\r\nl2\r\nl3\r\n");

        var parts = MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 1 });

        Assert.Equal(3, parts.Count);
        Assert.Equal("l1\r\n", File.ReadAllText(parts[0]));
        Assert.Equal("l2\r\n", File.ReadAllText(parts[1]));
        Assert.Equal("l3\r\n", File.ReadAllText(parts[2]));
    }

    [Fact]
    public void SplitByLine_NoSeparator_ThrowsWithReadableMessage()
    {
        // 历史行为为静默 return（无任何输出）；统一为抛出可读异常，UI 显示"分割失败: ..."
        var source = WriteFile("nosep.txt", "onelonglinewithoutseparator");

        Assert.Throws<InvalidDataException>(() =>
            MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLine, Max = 2 }));
    }

    // ==================================================================== 按长度分割

    [Fact]
    public void SplitByLength_AlignsBreakToLineEnd()
    {
        // 修复说明：历史 Win 版在切断点落在行中间时会从行中间切断（IndexOf -1 误判）；
        // 迁移取 macOS 修正语义：切断点对齐到行尾。
        // Max=108 → 有效取字长度 8：每片取 8 字符后对齐到当前行行尾（每行 10 字 + \r\n）。
        var source = WriteFile("len.txt", "aaaaaaaaaa\r\nbbbbbbbbbb\r\ncccccccccc\r\n");

        var parts = MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.ByLength, Max = 108 });

        // 每片至少包含完整行（不会从行中间切断）：3 行 → 3 片，每片恰好一行
        Assert.Equal(3, parts.Count);
        Assert.Equal("aaaaaaaaaa\r\n", File.ReadAllText(parts[0]));
        Assert.Equal("bbbbbbbbbb\r\n", File.ReadAllText(parts[1]));
        Assert.Equal("cccccccccc\r\n", File.ReadAllText(parts[2]));
    }

    // ==================================================================== 按大小分割

    [Fact]
    public void SplitBySize_SplitsBinaryRoughlyByKb()
    {
        // 构造约 3KB 内容（Max=1KB → bufferSize=(1-10)*1024 为负！历史规则要求 Max ≥ 11）
        // UI 限定最小值；这里用 Max=21 → bufferSize=11KB > 文件，全部进第一片
        var content = string.Concat(Enumerable.Repeat("line-of-text\r\n", 100));
        var source = WriteFile("size.txt", content);

        var parts = MergeSplitService.SplitFile(source, new SplitOptions { Mode = SplitMode.BySize, Max = 21 });

        Assert.Single(parts);
        Assert.Equal(content.Length,
            File.ReadAllText(parts[0]).Length); // 无 BOM 编码下字节数一致
    }

    private static string[] ParseLines(string content) =>
        content.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
}
