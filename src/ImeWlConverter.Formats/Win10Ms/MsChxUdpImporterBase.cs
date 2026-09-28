namespace ImeWlConverter.Formats.Win10Ms;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// mschxudp（微软拼音/五笔用户自定义短语）导入公共实现。
/// 校验文件头魔数与结构边界，逐记录校验记录魔数后解析。
/// </summary>
public abstract class MsChxUdpImporterBase : BinaryFormatImporter
{
    private const int RecordHeaderSize = 16;
    private const uint RecordMagic = 0x00100010;

    /// <summary>词条编码类型。</summary>
    protected abstract CodeType EntryCodeType { get; }

    /// <summary>编码音节分隔符（拼音为 '，五笔为 null）。</summary>
    protected abstract char? SyllableSeparator { get; }

    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        if (!input.CanSeek)
            throw new InvalidDataException("输入流不可定位，无法解析 Win10 微软用户词库");

        if (input.Length < 0x40)
            throw new InvalidDataException("文件太小，不是有效的 Win10 微软用户词库（mschxudp）文件");

        input.Position = 0;
        Span<byte> magic = stackalloc byte[8];
        input.ReadExactly(magic);
        if (Encoding.ASCII.GetString(magic) != "mschxudp")
            throw new InvalidDataException("文件头不是 mschxudp，不是有效的 Win10 微软用户词库文件");

        using var reader = new BinaryReader(input, Encoding.Unicode, leaveOpen: true);

        input.Position = 0x10;
        var phraseOffsetStart = reader.ReadInt32(); // @0x10 偏移表起始
        var phraseStart = reader.ReadInt32();       // @0x14 记录区起始
        var phraseEnd = reader.ReadInt32();         // @0x18 文件大小
        var phraseCount = reader.ReadInt32();       // @0x1C 词条数

        // 结构边界校验：偏移表每条至少 4 字节，记录区在文件范围内
        var fileLength = input.Length;
        if (phraseOffsetStart < 0x40
            || phraseCount < 0
            || phraseCount > (fileLength - 0x40) / 4
            || phraseStart < phraseOffsetStart + 4L * phraseCount
            || phraseEnd < phraseStart
            || phraseEnd > fileLength)
        {
            throw new InvalidDataException("文件结构损坏或不受支持，无法解析 Win10 微软用户词库");
        }

        // 偏移表（相对记录区起始），末尾补一项作为最后一条记录的结束位置
        input.Position = phraseOffsetStart;
        var offsets = new List<int>(phraseCount + 1);
        for (var i = 0; i < phraseCount; i++)
            offsets.Add(reader.ReadInt32());
        offsets.Add(phraseEnd - phraseStart);

        var results = new List<WordEntry>(phraseCount);
        for (var i = 0; i < phraseCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            input.Position = phraseStart + offsets[i];
            var entry = ReadOnePhrase(reader, input, phraseStart + offsets[i + 1]);
            if (entry != null)
                results.Add(entry);
        }

        return results;
    }

    private WordEntry? ReadOnePhrase(BinaryReader reader, Stream stream, int nextStartPosition)
    {
        if (reader.ReadUInt32() != RecordMagic)
            return null; // 记录魔数不符，跳过（下一条从绝对偏移定位，不受影响）

        var hanziOffset = reader.ReadUInt16(); // 词区偏移
        // +0x06 为同编码候选序号（1 起），此处存入 Rank 保留往返信息
        var candidatePosition = stream.ReadByte();
        stream.ReadByte(); // 固定 0x06
        reader.ReadInt64(); // +0x08 4 字节 0 + 2000 纪元时间戳

        var codeBytesLen = hanziOffset - RecordHeaderSize - 2;
        if (codeBytesLen <= 0)
            return null;
        var codeStr = Encoding.Unicode.GetString(reader.ReadBytes(codeBytesLen));
        reader.ReadInt16(); // NUL 分隔

        var wordBytesLen = nextStartPosition - (int)stream.Position - 2;
        if (wordBytesLen <= 0)
            return null;
        var word = Encoding.Unicode.GetString(reader.ReadBytes(wordBytesLen));
        reader.ReadInt16(); // NUL 结尾

        if (word.Length == 0)
            return null;

        IReadOnlyList<string> segments;
        if (SyllableSeparator is { } sep)
        {
            segments = codeStr.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Count == 0)
                return null;
        }
        else
        {
            if (codeStr.Length == 0)
                return null;
            segments = new[] { codeStr };
        }

        return new WordEntry
        {
            Word = word,
            Rank = candidatePosition,
            CodeType = EntryCodeType,
            Code = WordCode.FromSingle(segments)
        };
    }
}
