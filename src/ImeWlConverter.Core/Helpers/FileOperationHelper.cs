using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using UtfUnknown;

namespace ImeWlConverter.Core.Helpers;

public static class FileOperationHelper
{
    public static string GetCurrentFolderPath()
    {
        return Path.GetDirectoryName(AppContext.BaseDirectory)!;
    }

    /// <summary>
    /// 自动判断文字编码，然后进行读取
    /// </summary>
    public static string ReadFile(string path)
    {
        if (!File.Exists(path)) return "";
        var c = GetEncodingType(path);
        return ReadFile(path, c);
    }

    public static string ReadFile(string path, Encoding encoding)
    {
        if (!File.Exists(path)) return "";
        using var sr = new StreamReader(path, encoding);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// 获取文件的流式读取器,用于处理大文件避免内存溢出
    /// </summary>
    public static StreamReader? GetStreamReader(string path, Encoding encoding)
    {
        if (!File.Exists(path)) return null;
        return new StreamReader(path, encoding);
    }

    /// <summary>
    /// 检查文件是否应该使用流式处理(文件大于10MB)
    /// </summary>
    public static bool ShouldUseStreaming(string path)
    {
        if (!File.Exists(path)) return false;
        var fileInfo = new FileInfo(path);
        return fileInfo.Length > 10 * 1024 * 1024;
    }

    /// <summary>
    /// 将一个字符串写入文件，采用覆盖的方式
    /// </summary>
    public static bool WriteFile(string path, Encoding coding, string content)
    {
        try
        {
            using var sw = new StreamWriter(path, false, coding);
            sw.Write(content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static StreamWriter WriteFile(string path, Encoding coding)
    {
        return new StreamWriter(path, false, coding);
    }

    /// <summary>
    /// 写一行文本到文件，追加的方式
    /// </summary>
    public static bool WriteFileLine(string path, string line)
    {
        try
        {
            using var sw = new StreamWriter(path, true);
            sw.WriteLine(line);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool WriteFileLine(StreamWriter sw, string line)
    {
        try
        {
            sw.WriteLine(line);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static StreamWriter GetWriteFileStream(string path, Encoding coding)
    {
        return new StreamWriter(path, false, coding);
    }

    public static Encoding GetEncodingType(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("文件路径不能为空", nameof(fileName));
        }

        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"文件不存在: {fileName}");
        }

        try
        {
            // 优先做严格 UTF-8 结构校验：UTF-8 编码结构刚性，整个探针能通过严格校验
            // 且含多字节序列的文件几乎必然是 UTF-8。
            // 历史 bug：短中文 UTF-8（无 BOM）文件被 CharsetDetector 误判为 GBK 家族，
            // 读出"浠?"式乱码（词库合并/分割窗口实证）。
            if (IsValidUtf8WithMultibyte(fileName))
            {
                return new UTF8Encoding(false);
            }

            var result = CharsetDetector.DetectFromFile(fileName);
            var resultDetected = result?.Detected;

            if (resultDetected == null || resultDetected.Confidence < 0.7)
            {
                try
                {
                    return Encoding.GetEncoding("GB18030");
                }
                catch
                {
                    return Encoding.GetEncoding("GB2312");
                }
            }

            return resultDetected.Encoding ?? Encoding.UTF8;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"检测文件编码失败: {fileName}, 错误: {ex.Message}");
            return Encoding.UTF8;
        }
    }

    /// <summary>
    /// 严格 UTF-8 校验：对文件头部（最多 64KB）逐字节验证 UTF-8 结构
    /// （含过长编码/代理区排除），且至少含一个多字节序列才认为是 UTF-8。
    /// 校验失败（含探针边界截断）返回 false，交由后续 CharsetDetector 处理。
    /// </summary>
    private static bool IsValidUtf8WithMultibyte(string fileName)
    {
        const int maxProbeBytes = 64 * 1024;
        using var fs = File.OpenRead(fileName);
        var buffer = new byte[Math.Min(maxProbeBytes, fs.Length)];
        if (buffer.Length == 0) return false;
        fs.ReadExactly(buffer, 0, buffer.Length);

        var hasMultibyte = false;
        var i = 0;
        // 跳过 UTF-8 BOM
        if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            i = 3;

        while (i < buffer.Length)
        {
            var b = buffer[i];
            if (b < 0x80)
            {
                i++;
                continue;
            }

            var len = b switch
            {
                >= 0xC2 and <= 0xDF => 2,
                >= 0xE0 and <= 0xEF => 3,
                >= 0xF0 and <= 0xF4 => 4,
                _ => 0,
            };
            // 非法首字节，或探针边界恰好截断多字节序列（交由 CharsetDetector 兜底）
            if (len == 0 || i + len > buffer.Length)
                return false;

            for (var j = 1; j < len; j++)
                if ((buffer[i + j] & 0xC0) != 0x80)
                    return false;

            // 排除过长编码与 UTF-16 代理区
            if (len == 3)
            {
                if (b == 0xE0 && buffer[i + 1] < 0xA0) return false;
                if (b == 0xED && buffer[i + 1] > 0x9F) return false;
            }
            else if (len == 4)
            {
                if (b == 0xF0 && buffer[i + 1] < 0x90) return false;
                if (b == 0xF4 && buffer[i + 1] > 0x8F) return false;
            }

            hasMultibyte = true;
            i += len;
        }

        return hasMultibyte;
    }

    public static void WriteFileHeader(FileStream fs, Encoding encoding)
    {
        if (encoding == Encoding.UTF8)
        {
            fs.WriteByte(0xEF);
            fs.WriteByte(0xBB);
            fs.WriteByte(0xBF);
        }
        else if (encoding == Encoding.Unicode)
        {
            fs.WriteByte(0xFF);
            fs.WriteByte(0xFE);
        }
        else if (encoding == Encoding.BigEndianUnicode)
        {
            fs.WriteByte(0xFE);
            fs.WriteByte(0xFF);
        }
    }

    /// <summary>
    /// 根据文本框输入的一个路径，返回文件列表
    /// </summary>
    public static IList<string> GetFilesPath(string input)
    {
        var result = new List<string>();
        foreach (var path in input.Split('|')) result.AddRange(GetFilesPathFor1(path.Trim()));
        return result;
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    public static long GetFileSize(string sFullName)
    {
        long lSize = 0;
        if (File.Exists(sFullName))
            lSize = new FileInfo(sFullName).Length;
        return lSize;
    }

    private static IList<string> GetFilesPathFor1(string input)
    {
        if (input.Contains("*"))
        {
            var dic = Path.GetDirectoryName(input)!;
            var filen = Path.GetFileName(input);
            return Directory.GetFiles(dic, filen, SearchOption.AllDirectories);
        }

        if (Directory.Exists(input))
            return Directory.GetFiles(input, "*.*", SearchOption.AllDirectories);
        if (File.Exists(input))
            return new List<string> { input };
        return new List<string>();
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    public static bool ZipFile(string fileToZip, string zipedFile)
    {
        var result = true;
        ZipOutputStream? zipStream = null;
        FileStream? fs = null;
        ZipEntry? ent = null;

        if (!File.Exists(fileToZip))
            return false;

        try
        {
            fs = File.OpenRead(fileToZip);
            var buffer = new byte[fs.Length];
            fs.ReadExactly(buffer);
            fs.Close();

            fs = File.Create(zipedFile);
            zipStream = new ZipOutputStream(fs);
            ent = new ZipEntry(Path.GetFileName(fileToZip));
            zipStream.PutNextEntry(ent);
            zipStream.SetLevel(6);

            zipStream.Write(buffer, 0, buffer.Length);
        }
        catch
        {
            result = false;
        }
        finally
        {
            if (zipStream != null)
            {
                zipStream.Finish();
                zipStream.Close();
            }

            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }
        }

        return result;
    }

    /// <summary>
    /// 解压功能(解压压缩文件到指定目录)
    /// </summary>
    public static bool UnZip(string fileToUnZip, string zipedFolder)
    {
        var result = true;
        ZipInputStream? zipStream = null;

        if (!File.Exists(fileToUnZip))
            return false;

        if (!Directory.Exists(zipedFolder))
            Directory.CreateDirectory(zipedFolder);

        try
        {
            zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));
            ZipEntry? ent;
            while ((ent = zipStream.GetNextEntry()) != null)
                if (!string.IsNullOrEmpty(ent.Name))
                {
                    var fileName = Path.Combine(zipedFolder, ent.Name);
                    fileName = fileName.Replace('/', Path.DirectorySeparatorChar);

                    if (fileName.EndsWith(Path.DirectorySeparatorChar))
                    {
                        Directory.CreateDirectory(fileName);
                        continue;
                    }

                    using var streamWriter = File.Create(fileName);
                    var buffer = new byte[10240];
                    var size = zipStream.Read(buffer, 0, buffer.Length);
                    while (size > 0)
                    {
                        streamWriter.Write(buffer, 0, size);
                        size = zipStream.Read(buffer, 0, buffer.Length);
                    }
                }
        }
        catch
        {
            result = false;
        }
        finally
        {
            if (zipStream != null)
            {
                zipStream.Close();
                zipStream.Dispose();
            }
        }

        return result;
    }
}
