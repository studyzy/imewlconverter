namespace ImeWlConverter.Formats.LibIMEPinyin;

using System.Buffers.Binary;
using System.IO;
using ZstdSharp;
using ZstdSharp.Unsafe;

/// <summary>
/// Fcitx5 / libime 二进制拼音词库的文件容器。
///
/// 布局（与 libime <c>libime/pinyin/pinyindictionary.cpp</c> 一致）：
/// <code>
/// [be32 magic = 0x000fc613][be32 version]
/// version 0x1: 直接是 DATrie&lt;float&gt; 序列化数据
/// version 0x2: DATrie&lt;float&gt; 序列化数据经 zstd 压缩（带校验和）
/// </code>
/// </summary>
internal static class LibimeDictFormat
{
    /// <summary>libime <c>pinyinBinaryFormatMagic</c>。</summary>
    public const uint Magic = 0x000fc613;

    /// <summary>当前 libime 二进制版本（数据经 zstd 压缩）。</summary>
    public const uint Version = 0x2;

    /// <summary>旧版 libime 二进制版本（数据未压缩）。</summary>
    public const uint LegacyVersion = 0x1;

    /// <summary>拼音与汉字之间的分隔符（libime <c>pinyinHanziSep</c>）。</summary>
    public const byte HanziSeparator = (byte)'!';

    /// <summary>读取完整词库文件（magic/version 头 + 可能被 zstd 压缩的 DATrie）。</summary>
    public static CedarFloatTrie Read(Stream input)
    {
        Span<byte> header = stackalloc byte[8];
        ReadExactly(input, header);

        var magic = BinaryPrimitives.ReadUInt32BigEndian(header);
        if (magic != Magic)
            throw new InvalidDataException($"不是 libime 二进制词库：magic=0x{magic:x8}，期望 0x{Magic:x8}。");

        var version = BinaryPrimitives.ReadUInt32BigEndian(header[4..]);
        switch (version)
        {
            case LegacyVersion:
                return CedarFloatTrie.Load(input);

            case Version:
                using (var decompressed = new DecompressionStream(
                           input, bufferSize: 0, checkEndOfStream: true, leaveOpen: true))
                    return CedarFloatTrie.Load(decompressed);

            default:
                throw new InvalidDataException($"不支持的 libime 二进制版本：{version}。");
        }
    }

    /// <summary>写入完整词库文件（magic/version 头 + zstd 压缩后的 DATrie）。</summary>
    public static void Write(CedarFloatTrie trie, Stream output)
    {
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(header, Magic);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], Version);
        output.Write(header);

        // libime 使用 ZSTD_initCStream(level 0) 并开启校验和（ZSTD_c_checksumFlag=1）。
        using var compressor = new CompressionStream(output, level: 3, bufferSize: 0, leaveOpen: true);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_checksumFlag, 1);
        trie.Save(compressor);
    }

    private static void ReadExactly(Stream input, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = input.Read(buffer[read..]);
            if (n <= 0)
                throw new EndOfStreamException();
            read += n;
        }
    }
}
