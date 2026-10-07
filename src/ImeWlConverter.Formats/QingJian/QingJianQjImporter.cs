namespace ImeWlConverter.Formats.QingJian;

using System.Buffers.Binary;
using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>
/// 青简词库（.qj 二进制容器）导入器。
/// 解析 Dictionary kind 的四个分节：TEXT（词文本 arena）、KEYS（拼音键 arena）、
/// INDX（拼音键索引，按键字节序升序）、SLOT（词目，含词频），反解出词、拼音、词频。
/// 格式规格见 <see cref="QingJianQjSpec"/>。
/// </summary>
[FormatPlugin("qj", "青简词库（.qj）", 161, IsBinary = true, FileExtension = ".qj")]
public sealed partial class QingJianQjImporter : BinaryFormatImporter
{
    protected override IReadOnlyList<WordEntry> ParseBinary(Stream input, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        var data = buffer.ToArray();

        if (data.Length < QingJianQjSpec.HeaderSize)
            throw new InvalidDataException("文件太小，不是有效的青简词库（.qj）文件");

        if (!data.AsSpan(0, QingJianQjSpec.Magic.Length).SequenceEqual(QingJianQjSpec.Magic))
            throw new InvalidDataException("文件头不是 QINGJIAN，不是有效的青简词库（.qj）文件");

        var version = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(8, 2));
        if (version != QingJianQjSpec.FormatVersion)
            throw new InvalidDataException(
                $"不支持的 .qj 格式版本 {version}（当前仅支持版本 {QingJianQjSpec.FormatVersion}）");

        var kind = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(10, 2));
        if (kind != QingJianQjSpec.KindDictionary)
            throw new InvalidDataException(
                $".qj 文件数据种类为 {kind}，不是拼音词库（kind={QingJianQjSpec.KindDictionary}）");

        var sectionCount = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(12, 4));
        var tableEnd = QingJianQjSpec.HeaderSize + (long)sectionCount * QingJianQjSpec.SectionEntrySize;
        if (tableEnd > data.Length)
            throw new InvalidDataException(".qj 分节表越界，文件已损坏");

        var sections = new Dictionary<string, (int Offset, int Length)>();
        for (var i = 0; i < sectionCount; i++)
        {
            var entryOffset = QingJianQjSpec.HeaderSize + i * QingJianQjSpec.SectionEntrySize;
            var tag = Encoding.ASCII.GetString(data, entryOffset, 4);
            var offset = (long)QingJianQjSpec.ReadUInt32(data, entryOffset + 8)
                | ((long)QingJianQjSpec.ReadUInt32(data, entryOffset + 12) << 32);
            var length = (long)QingJianQjSpec.ReadUInt32(data, entryOffset + 16)
                | ((long)QingJianQjSpec.ReadUInt32(data, entryOffset + 20) << 32);
            if (offset % QingJianQjSpec.SectionAlign != 0 || offset + length > data.Length)
                throw new InvalidDataException($".qj 分节 {tag} 越界或未对齐，文件已损坏");
            sections[tag] = ((int)offset, (int)length);
        }

        var (textOffset, textLength) = QingJianQjSpec.FindSection(sections, QingJianQjSpec.TextTag);
        var (keysOffset, keysLength) = QingJianQjSpec.FindSection(sections, QingJianQjSpec.KeysTag);
        var (indexOffset, indexLength) = QingJianQjSpec.FindSection(sections, QingJianQjSpec.IndexTag);
        var (slotsOffset, slotsLength) = QingJianQjSpec.FindSection(sections, QingJianQjSpec.SlotsTag);

        if (indexLength % QingJianQjSpec.KeyIndexSize != 0 || slotsLength % QingJianQjSpec.SlotSize != 0)
            throw new InvalidDataException(".qj 索引或词目分节长度不是记录大小的整数倍，文件已损坏");

        var keyCount = indexLength / QingJianQjSpec.KeyIndexSize;
        var slotCount = slotsLength / QingJianQjSpec.SlotSize;
        var entries = new List<WordEntry>(slotCount);

        for (var i = 0; i < keyCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var entryOffset = indexOffset + i * QingJianQjSpec.KeyIndexSize;
            var keyStart = QingJianQjSpec.ReadUInt32(data, entryOffset);
            var firstSlot = QingJianQjSpec.ReadUInt32(data, entryOffset + 4);
            var count = QingJianQjSpec.ReadUInt32(data, entryOffset + 8);
            var keyLen = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(entryOffset + 12, 2));

            if ((long)keyStart + keyLen > keysLength || (long)firstSlot + count > slotCount)
                throw new InvalidDataException(".qj 键索引指向越界，文件已损坏");

            var pinyin = Encoding.UTF8.GetString(data, keysOffset + (int)keyStart, keyLen);
            // 键内音节以单空格分隔，规范键不含连续空格；防御性去掉空音节
            var syllables = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (var s = 0; s < count; s++)
            {
                var slotOffset = slotsOffset + ((int)firstSlot + s) * QingJianQjSpec.SlotSize;
                var textStart = QingJianQjSpec.ReadUInt32(data, slotOffset);
                var frequency = QingJianQjSpec.ReadUInt32(data, slotOffset + 4);
                var textLen = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(slotOffset + 8, 2));

                if ((long)textStart + textLen > textLength)
                    throw new InvalidDataException(".qj 词目指向文本 arena 之外，文件已损坏");

                entries.Add(new WordEntry
                {
                    Word = Encoding.UTF8.GetString(data, textOffset + (int)textStart, textLen),
                    Rank = frequency > int.MaxValue ? int.MaxValue : (int)frequency,
                    CodeType = CodeType.Pinyin,
                    Code = WordCode.FromSingle(syllables)
                });
            }
        }

        return entries;
    }
}
