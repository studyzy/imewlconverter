using System.Text;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Application.MergeSplit;

/// <summary>文件分割模式。</summary>
public enum SplitMode
{
    /// <summary>按行数分割。</summary>
    ByLine,

    /// <summary>按大小分割（KB）。</summary>
    BySize,

    /// <summary>按字符长度分割。</summary>
    ByLength,
}

/// <summary>合并结果。</summary>
public sealed record MergeResult(string Content, int EntryCount);

/// <summary>文件分割选项。</summary>
public sealed record SplitOptions
{
    /// <summary>分割模式。</summary>
    public required SplitMode Mode { get; init; }

    /// <summary>ByLine: 每个文件最大行数；BySize: 每个文件最大 KB；ByLength: 每个文件最大字符数。</summary>
    public required int Max { get; init; }
}

/// <summary>
/// 词库合并 / 文件分割服务（三端共享的纯业务逻辑）。
/// 算法迁移自 WinForms MergeWLForm/SplitFileForm 与 macOS MergeWLWindow/SplitFileWindow
/// 的双端重复实现（约 400 行），并做了两处统一（均为 bug 修复/规范化，见各行注释）：
///  1. 合并输出行尾统一为 \r\n（此前 Win 为 \r\n、Mac 为 \n）；
///  2. 按长度分割的行尾对齐修复：行内切断时对齐到下一个行尾
///     （此前 Win 版在 IndexOf 返回 -1 时误判，会把词条从行中间切断，Mac 版已修复）。
/// UI 层只负责文件选择、预览展示与保存对话框，不再持有任何算法。
/// </summary>
public static class MergeSplitService
{
    // ==================================================================== 合并

    /// <summary>
    /// 合并词库文件。主词库 1 个 + 附加词库 N 个，行格式："编码 词1 词2 词3"。
    /// 同编码词条去重合并；sortByCode 时按编码排序。输出编码固定 Unicode（与历史行为一致）。
    /// </summary>
    public static MergeResult MergeFiles(string mainFilePath, IEnumerable<string> userIdleFilePaths, bool sortByCode)
    {
        var mainDict = ParseDictionary(FileOperationHelper.ReadFile(mainFilePath));

        foreach (var userFile in userIdleFilePaths)
        {
            var filePath = userFile.Trim();
            if (filePath.Length == 0) continue;
            var userDict = ParseDictionary(FileOperationHelper.ReadFile(filePath));
            MergeDictionary(mainDict, userDict);
        }

        if (sortByCode)
        {
            var keys = new List<string>(mainDict.Keys);
            keys.Sort();
            var sorted = new Dictionary<string, List<string>>(keys.Count);
            foreach (var key in keys) sorted.Add(key, mainDict[key]);
            mainDict = sorted;
        }

        return new MergeResult(DictionaryToText(mainDict), mainDict.Count);
    }

    /// <summary>解析 "编码 词1 词2" 格式文本 → 编码 → 词列表。</summary>
    internal static Dictionary<string, List<string>> ParseDictionary(string txt)
    {
        var lines = txt.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var dict = new Dictionary<string, List<string>>();
        foreach (var line in lines)
        {
            var array = line.Split(' ');
            var key = array[0];
            if (!dict.TryGetValue(key, out var words))
            {
                words = [];
                dict.Add(key, words);
            }

            for (var i = 1; i < array.Length; i++) words.Add(array[i]);
        }

        return dict;
    }

    /// <summary>把 d2 并入 d1：新编码直接加入，已有编码的词条按词去重追加。</summary>
    internal static void MergeDictionary(Dictionary<string, List<string>> d1, Dictionary<string, List<string>> d2)
    {
        foreach (var pair in d2)
        {
            if (!d1.TryGetValue(pair.Key, out var words))
            {
                d1.Add(pair.Key, pair.Value);
            }
            else
            {
                foreach (var word in pair.Value)
                    if (!words.Contains(word))
                        words.Add(word);
            }
        }
    }

    /// <summary>词典 → 文本。行尾统一 \r\n（历史 Mac 版为 \n，统一取 Windows 原版语义）。</summary>
    internal static string DictionaryToText(Dictionary<string, List<string>> dictionary)
    {
        var sb = new StringBuilder();
        foreach (var pair in dictionary)
        {
            sb.Append(pair.Key);
            if (pair.Value is { Count: > 0 })
            {
                sb.Append(' ');
                sb.Append(string.Join(" ", pair.Value));
            }

            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    // ==================================================================== 分割

    /// <summary>
    /// 分割文件，写出的分片与源文件同目录，命名为 "原名NN.扩展名"（如 词库01.txt）。
    /// 返回写出的分片路径列表（按顺序）。源文件编码自动检测，分片沿用同一编码。
    /// </summary>
    public static IReadOnlyList<string> SplitFile(string filePath, SplitOptions options)
    {
        return options.Mode switch
        {
            SplitMode.ByLine => SplitByLine(filePath, options.Max),
            SplitMode.BySize => SplitBySize(filePath, options.Max),
            SplitMode.ByLength => SplitByLength(filePath, options.Max),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.Mode, "未知分割模式"),
        };
    }

    /// <summary>生成分片路径：与源文件同目录，"原名+两位序号+扩展名"（Path.Combine，不再硬编码分隔符）。</summary>
    internal static string GetPartPath(string sourcePath, int index)
    {
        return Path.Combine(
            Path.GetDirectoryName(sourcePath) ?? "",
            Path.GetFileNameWithoutExtension(sourcePath) + index.ToString("00") + Path.GetExtension(sourcePath));
    }

    /// <summary>探测行分隔符：优先 \r\n，退而 \r、\n；都找不到返回 null。</summary>
    internal static string? DetectLineSeparator(string content)
    {
        if (content.IndexOf("\r\n", StringComparison.Ordinal) >= 0) return "\r\n";
        if (content.IndexOf('\r') > 0) return "\r";
        if (content.IndexOf('\n') > 0) return "\n";
        return null;
    }

    private static IReadOnlyList<string> SplitByLine(string filePath, int maxLine)
    {
        var encoding = FileOperationHelper.GetEncodingType(filePath);
        var content = FileOperationHelper.ReadFile(filePath, encoding);

        var separator = DetectLineSeparator(content)
                        ?? throw new InvalidDataException("不能找到行分隔符");

        var lines = content.Split([separator], StringSplitOptions.RemoveEmptyEntries);
        var outputFiles = new List<string>();
        var buffer = new StringBuilder();
        var fileIndex = 1;

        for (var i = 0; i < lines.Length; i++)
        {
            buffer.Append(lines[i]);
            buffer.Append(separator);
            if (((i + 1) % maxLine == 0 || i == lines.Length - 1) && i != 0)
            {
                var partPath = GetPartPath(filePath, fileIndex++);
                FileOperationHelper.WriteFile(partPath, encoding, buffer.ToString());
                outputFiles.Add(partPath);
                buffer = new StringBuilder();
            }
        }

        return outputFiles;
    }

    private static IReadOnlyList<string> SplitBySize(string filePath, int maxKB)
    {
        var encoding = FileOperationHelper.GetEncodingType(filePath);
        var bufferSize = (maxKB - 10) * 1024; // 10K 的余量（历史行为）
        var outputFiles = new List<string>();
        var fileIndex = 1;

        using var input = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        do
        {
            var partPath = GetPartPath(filePath, fileIndex++);
            using (var output = new FileStream(partPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (fileIndex != 2) // 非第一个分片要重写文件头（如 BOM）
                    FileOperationHelper.WriteFileHeader(output, encoding);

                var buffer = new byte[bufferSize];
                var read = input.Read(buffer, 0, bufferSize);
                if (read > 0)
                {
                    output.Write(buffer, 0, read);
                    var hasContent = true;
                    do
                    {
                        var b = input.ReadByte();
                        if (b == 0xA || b == 0xD)
                        {
                            ReadToNextLine(input);
                            hasContent = false;
                        }

                        if (b != -1)
                            output.WriteByte((byte)b);
                        else
                            hasContent = false;
                    } while (hasContent);
                }
            }

            outputFiles.Add(partPath);
        } while (input.Position != input.Length);

        return outputFiles;
    }

    /// <summary>跳过行尾字符，定位到下一行行首（流位置回退一个非行尾字节）。</summary>
    private static bool ReadToNextLine(FileStream fs)
    {
        do
        {
            var b = fs.ReadByte();
            if (b == -1) return false;
            if (b != 0xA && b != 0xD && b != 0)
            {
                fs.Position--;
                return true;
            }
        } while (true);
    }

    private static IReadOnlyList<string> SplitByLength(string filePath, int maxLength)
    {
        var length = maxLength - 100; // 100 字余量（历史行为）
        var encoding = FileOperationHelper.GetEncodingType(filePath);
        var remaining = FileOperationHelper.ReadFile(filePath, encoding);
        var outputFiles = new List<string>();
        var fileIndex = 1;

        do
        {
            if (remaining.Length == 0) break;

            var content = remaining.Substring(0, Math.Min(remaining.Length, length));
            remaining = remaining.Substring(content.Length);

            // 行尾对齐：切断点落在行中间时，把分片延伸到本行行尾。
            // 修复历史 Win 版 bug：IndexOf 返回 -1 时 Math.Min 会误判为 -1 而跳过对齐，
            // 导致词条从行中间被切断（Mac 版已修复，此处取修正后语义）。
            var nextBreak = Math.Min(
                remaining.IndexOf('\r') >= 0 ? remaining.IndexOf('\r') : int.MaxValue,
                remaining.IndexOf('\n') >= 0 ? remaining.IndexOf('\n') : int.MaxValue);
            if (nextBreak != int.MaxValue)
            {
                var take = Math.Min(nextBreak + 2, remaining.Length);
                content += remaining.Substring(0, take);
                remaining = remaining.Substring(take);
            }

            var partPath = GetPartPath(filePath, fileIndex++);
            FileOperationHelper.WriteFile(partPath, encoding, content);
            outputFiles.Add(partPath);
        } while (true);

        return outputFiles;
    }
}
