/*!
 * This work contains codes translated from the original work Sogou-User-Dict-Converter by h4x3rotab
 * Copyright h4x3rotab
 * Licensed under the GNU General Public License v3.0.
 *
 * This work contains codes translated from the original work rose by nopdan
 * Copyright nopdan
 * Licensed under the GNU General Public License v3.0.
 */

namespace ImeWlConverter.Formats.SougouBin;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;

/// <summary>
/// Parser for Sougou Pinyin binary backup dictionaries.
/// Used by SougouBinImporter.
/// </summary>
internal static class SougouBinParser
{
    private const int UserDictHeaderSize = 80;

    public static IReadOnlyList<WordEntry> Parse(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        ms.Position = 0;

        var header = ReadUInt32(ms);
        if (header == 0x55504753)
            return NewParser(ms);

        uint checksum = 0;
        var userDict = InitialiseUserDict(ms, ref checksum);
        var userHeader = ReadUserHeader(ms);
        return ReadAllWords(ms, userDict, userHeader);
    }

    #region new format @nopdan

    private static IReadOnlyList<WordEntry> NewParser(MemoryStream fs)
    {
        var wordList = new List<WordEntry>();

        fs.Seek(0x38, SeekOrigin.Begin);
        var idxBegin = ReadUInt32(fs);
        var idxSize = ReadUInt32(fs);
        var wordCount = ReadUInt32(fs);
        var dictBegin = ReadUInt32(fs);
        var dictTotalSize = ReadUInt32(fs);
        var dictSize = ReadUInt32(fs);

        for (var i = 0; i < wordCount; i++)
        {
            fs.Seek(idxBegin + 4 * i, SeekOrigin.Begin);

            var idx = ReadUInt32(fs);
            fs.Seek(idx + dictBegin, SeekOrigin.Begin);
            var freq = ReadUInt16(fs);
            fs.Seek(2, SeekOrigin.Current);
            fs.Seek(5, SeekOrigin.Current);

            var n = ReadUInt16(fs) / 2;
            var pinyin = new List<string>();
            for (var j = 0; j < n; j++)
            {
                var p = ReadUInt16(fs);
                if (p < PinyinData.Length)
                    pinyin.Add(PinyinData[p]);
            }

            fs.Seek(2, SeekOrigin.Current);
            var wordSize = ReadUInt16(fs);
            var str = new byte[wordSize];
            fs.ReadExactly(str, 0, wordSize);
            var word = Encoding.Unicode.GetString(str);
            wordList.Add(new WordEntry
            {
                Word = word,
                Rank = freq,
                CodeType = CodeType.Pinyin,
                Code = pinyin.Count > 0
                    ? WordCode.FromSingle(pinyin.ToArray())
                    : null
            });
        }

        return wordList;
    }

    #endregion

    #region Old format parsing

    private static SougouPinyinDictData InitialiseUserDict(MemoryStream fs, ref uint checksum)
    {
        var size = fs.Length;

        if (fs.Position != 4) fs.Seek(4, SeekOrigin.Begin);

        var uintP4 = ReadUInt32(fs);
        var uintP8 = ReadUInt32(fs);
        var uintP12 = ReadUInt32(fs);
        var uintP16 = ReadUInt32(fs);

        checksum += uintP4 + uintP8 + uintP12 + uintP16;

        Debug.Assert(uintP4 <= (uint)size);

        fs.Seek(20, SeekOrigin.Begin);
        var userDict = new SougouPinyinDictData(
            ReadKeys(fs, uintP8),
            ReadAttributes(fs, uintP12),
            ReadAInts(fs, uintP16)
        );

        Debug.Assert(fs.Position == uintP4 + 8);

        var headerSize =
            12
            * (
                userDict.AttributeList.Count
                + userDict.AIntList.Count
                + userDict.KeyList.Count
            )
            + 24;

        fs.Seek(8, SeekOrigin.Current);

        var totalSize = ReadUInt32(fs);

        Debug.Assert(totalSize + headerSize + uintP4 + 8 == (uint)size - UserDictHeaderSize);

        var size3B2 = ReadUInt32(fs);
        var size4B2 = ReadUInt32(fs);
        var size5B2 = ReadUInt32(fs);

        userDict.HeaderItemsIdxList = ReadHeaderItems(fs, size3B2, ref checksum);
        userDict.HeaderItemsAttrList = ReadHeaderItems(fs, size4B2, ref checksum);
        userDict.DataStore = ReadHeaderItems(fs, size5B2, ref checksum);

        Debug.Assert(uintP4 + 8 + headerSize == (uint)fs.Position);
        userDict.DsBasePos = fs.Position;

        return userDict;
    }

    private static List<SougouPinyinDictData.KeyItem> ReadKeys(MemoryStream fs, uint length)
    {
        var list = new List<SougouPinyinDictData.KeyItem>();
        for (var i = 0; i < length; i++)
        {
            var key = new SougouPinyinDictData.KeyItem();
            key.DictTypeDef = ReadUInt16(fs);
            Debug.Assert(key.DictTypeDef < 100);
            key.DataType = new List<ushort>();
            var dataTypeCount = ReadUInt16(fs);
            for (var j = 0; j < dataTypeCount; j++)
                key.DataType.Add(ReadUInt16(fs));

            key.AttrIdx = ReadInt32(fs);
            key.KeyDataIdx = ReadInt32(fs);
            key.DataIdx = ReadInt32(fs);
            key.V6 = ReadUInt32(fs);

            list.Add(key);
        }

        return list;
    }

    private static List<SougouPinyinDictData.AttrItem> ReadAttributes(MemoryStream fs, uint length)
    {
        var list = new List<SougouPinyinDictData.AttrItem>();
        for (var i = 0; i < length; i++)
            list.Add(new SougouPinyinDictData.AttrItem
            {
                Count = ReadInt32(fs),
                A2 = ReadUInt32(fs),
                DataId = ReadInt32(fs),
                B2 = ReadUInt32(fs)
            });

        return list;
    }

    private static List<uint> ReadAInts(MemoryStream fs, uint length)
    {
        var list = new List<uint>();
        for (var i = 0; i < length; i++)
            list.Add(ReadUInt32(fs));

        return list;
    }

    private static List<SougouPinyinDictData.HeaderItem> ReadHeaderItems(
        MemoryStream fs,
        uint length,
        ref uint checksum)
    {
        var list = new List<SougouPinyinDictData.HeaderItem>();
        for (var i = 0; i < length; i++)
        {
            var h = new SougouPinyinDictData.HeaderItem();
            h.Parse(fs);
            checksum += h.Offset + (uint)h.DataSize + (uint)h.UsedDataSize;
            list.Add(h);
        }

        return list;
    }

    private static SougouPinyinUserHeader ReadUserHeader(MemoryStream fs)
    {
        fs.Seek(fs.Length - 0x4c, SeekOrigin.Begin);
        var userHeader = new SougouPinyinUserHeader();
        userHeader.Parse(fs);
        return userHeader;
    }

    private static IReadOnlyList<WordEntry> ReadAllWords(
        MemoryStream fs,
        SougouPinyinDictData dict,
        SougouPinyinUserHeader userHeader)
    {
        var wordList = new List<WordEntry>();
        var size = fs.Length;
        var keyId = 0;
        var key = dict.KeyList[keyId];
        var hashStoreBasePos = dict.GetHashStorePosition(keyId, key.DictTypeDef & 0xFFFFFF8F);
        fs.Seek(hashStoreBasePos, SeekOrigin.Begin);
        var hashStore = new HashStore();
        hashStore.EndPosition = fs.Position;

        var attrHeader = dict.HeaderItemsAttrList[key.AttrIdx];
        var attrCount =
            attrHeader.UsedDataSize == 0 ? attrHeader.DataSize : attrHeader.UsedDataSize;
        var hashStoreCount = dict.BaseHashSize[keyId];

        for (var ih = 0; ih < hashStoreCount; ih++)
        {
            fs.Seek(hashStore.EndPosition, SeekOrigin.Begin);
            hashStore = new HashStore();
            hashStore.Parse(fs);
            for (var ia = 0; ia < hashStore.Count; ia++)
            {
                var attrBasePos = dict.GetAttriStorePositionFromIndex(
                    keyId,
                    ia,
                    hashStore.Offset
                );
                fs.Seek(attrBasePos + dict.DataTypeSize[keyId] - 4, SeekOrigin.Begin);
                var offset = ReadInt32(fs);
                for (var ia2 = 0; ia2 < attrCount; ia2++)
                {
                    var attr2BasePos = dict.GetAttriStorePositionFromAttri(keyId, offset);
                    if (attr2BasePos > size)
                        break;

                    fs.Seek(attr2BasePos + dict.AttrSize[keyId] - 4, SeekOrigin.Begin);
                    offset = ReadInt32(fs);
                    wordList.Add(GetPyAndWord(fs, dict, attrBasePos, attr2BasePos, userHeader));
                    if (offset == -1)
                        break;
                }
            }
        }

        return wordList;
    }

    private static WordEntry GetPyAndWord(
        MemoryStream fs,
        SougouPinyinDictData dict,
        long attriPos1,
        long attriPos2,
        SougouPinyinUserHeader userHeader)
    {
        fs.Seek(attriPos1, SeekOrigin.Begin);
        var pos = ReadUInt32(fs);
        var pinyin = DecryptPinyin(fs, dict.GetPyPosition(pos));

        fs.Seek(attriPos2, SeekOrigin.Begin);
        var attr = new WordAttribute();
        attr.Parse(fs);

        var dataId = dict.GetDataIdByAttriId(dict.KeyList[0].AttrIdx);
        fs.Seek(dict.GetDataPosition(dataId, attr.Offset), SeekOrigin.Begin);
        var wordBytes = DecryptWords(fs, attr.P1, userHeader.P2, userHeader.P3);
        var word = Encoding.UTF32.GetString(wordBytes);

        return new WordEntry
        {
            Word = word,
            Rank = (int)attr.Frequency,
            CodeType = CodeType.Pinyin,
            Code = pinyin.Length > 0
                ? WordCode.FromSingle(pinyin)
                : null
        };
    }

    private static string[] DecryptPinyin(MemoryStream fs, long offset)
    {
        fs.Seek(offset, SeekOrigin.Begin);

        var n = ReadUInt16(fs) / 2;
        var pinyin = new List<string>();
        for (var i = 0; i < n; i++)
        {
            var p = ReadUInt16(fs);
            if (p < PinyinData.Length)
                pinyin.Add(PinyinData[p]);
        }

        return pinyin.ToArray();
    }

    private static byte[] DecryptWords(MemoryStream fs, uint p1, uint p2, uint p3)
    {
        var xk = (((p1 + p2) << 2) + ((p1 + p3) << 2)) & 0xffff;
        var n = ReadUInt16(fs) / 2;
        var decwords = new byte[n * 4];
        for (var i = 0; i < n; i++)
        {
            var shift = (int)(p2 % 8);
            var ch = ReadUInt16(fs);
            var dch = ((ch << (16 - shift % 8)) | (ch >> shift)) & 0xffff;
            dch ^= (int)xk;
            Buffer.BlockCopy(BitConverter.GetBytes(dch), 0, decwords, i * 4, 4);
        }

        return decwords;
    }

    #endregion

    #region Structures

    private struct SougouPinyinUserHeader
    {
        public uint P2;
        public uint P3;

        public void Parse(MemoryStream fs)
        {
            fs.Seek(fs.Position + 56, SeekOrigin.Begin);
            P2 = ReadUInt32(fs);
            P3 = ReadUInt32(fs);
        }
    }

    private struct HashStore
    {
        public int Offset;
        public int Count;
        public long EndPosition;

        public void Parse(MemoryStream fs)
        {
            Offset = ReadInt32(fs);
            Count = ReadInt32(fs);
            EndPosition = fs.Position;
        }
    }

    private struct WordAttribute
    {
        public int Offset;
        public uint Frequency;
        public uint AFlag;
        public uint I8;
        public uint P1;
        public int IE;

        public void Parse(MemoryStream fs)
        {
            Offset = ReadInt32(fs);
            Frequency = ReadUInt16(fs);
            AFlag = ReadUInt16(fs);
            I8 = ReadUInt32(fs);
            P1 = ReadUInt16(fs);
            IE = ReadInt32(fs);
            fs.Seek(4, SeekOrigin.Current);
        }
    }

    private sealed class SougouPinyinDictData
    {
        public static readonly ReadOnlyCollection<int> DatatypeHashSize = new List<int>
        {
            0, 27, 414, 512, -1, -1, 512, 0
        }.AsReadOnly();

        public static readonly ReadOnlyCollection<int> KeyItemDataTypeSize = new List<int>
        {
            4, 1, 1, 2, 1, 2, 2, 4, 4, 8, 4, 4, 4, 0, 0, 0
        }.AsReadOnly();

        public struct KeyItem
        {
            public ushort DictTypeDef;
            public List<ushort> DataType;
            public int AttrIdx;
            public int KeyDataIdx;
            public int DataIdx;
            public uint V6;
        }

        public struct AttrItem
        {
            public int Count;
            public uint A2;
            public int DataId;
            public uint B2;
        }

        public struct HeaderItem
        {
            public uint Offset;
            public int DataSize;
            public int UsedDataSize;

            public void Parse(MemoryStream fs)
            {
                Offset = ReadUInt32(fs);
                DataSize = ReadInt32(fs);
                UsedDataSize = ReadInt32(fs);
            }
        }

        public List<KeyItem> KeyList;
        public List<AttrItem> AttributeList;
        public List<uint> AIntList;
        public List<HeaderItem> HeaderItemsIdxList = new();
        public List<HeaderItem> HeaderItemsAttrList = new();
        public List<HeaderItem> DataStore = new();
        public long DsBasePos;
        public List<int> DataTypeSize = new();
        public int[] AttrSize = Array.Empty<int>();
        public List<int> BaseHashSize = new();
        public uint[] KeyHashSize = new uint[10];
        public bool AFlag;

        public SougouPinyinDictData(
            List<KeyItem> keyList,
            List<AttrItem> attributeList,
            List<uint> aIntList)
        {
            KeyList = keyList;
            AttributeList = attributeList;
            AIntList = aIntList;
            KeyHashSize[0] = 500;
            Initialise();
        }

        public long GetHashStorePosition(int indexId, uint dataType)
        {
            return DsBasePos + HeaderItemsIdxList[indexId].Offset - 8 * BaseHashSize[indexId];
        }

        public long GetIndexStorePosition(int indexId)
        {
            return DsBasePos + HeaderItemsIdxList[indexId].Offset;
        }

        public long GetAttriStorePosition(int attrId)
        {
            return DsBasePos + HeaderItemsAttrList[attrId].Offset;
        }

        public long GetAttriStorePositionFromIndex(int indexId, int attrId, long offset)
        {
            return GetIndexStorePosition(indexId) + offset + attrId * DataTypeSize[indexId];
        }

        public long GetAttriStorePositionFromAttri(int keyId, long offset)
        {
            return GetAttriStorePosition(KeyList[keyId].AttrIdx) + offset;
        }

        public long GetDataStorePosition(int dataId)
        {
            return DsBasePos + DataStore[dataId].Offset;
        }

        public long GetDataPosition(int dataId, long offset)
        {
            var header = DataStore[dataId];
            Debug.Assert(offset <= header.DataSize);
            return GetDataStorePosition(dataId) + offset;
        }

        public long GetPyPosition(long offset)
        {
            return GetDataPosition(KeyList[0].KeyDataIdx, offset);
        }

        public int GetDataIdByAttriId(int attrId)
        {
            return AttributeList[attrId].DataId;
        }

        private void Initialise()
        {
            DataTypeSize = new List<int>();
            BaseHashSize = new List<int>();
            AttrSize = new int[AttributeList.Count];

            for (var i = 0; i < KeyList.Count; i++)
            {
                var size = (KeyList[i].DictTypeDef >> 2) & 4;
                var maskedTypeDef = (int)(KeyList[i].DictTypeDef & 0xFFFFFF8F);

                PopulateBaseHashSizeList(i, maskedTypeDef);

                if (KeyList[i].AttrIdx < 0)
                {
                    size += GetDataTypeSize(KeyList[i], maskedTypeDef);
                    DataTypeSize.Add(size);
                }
                else
                {
                    size += GetNonAttributeDataSize(KeyList[i], maskedTypeDef);
                    DataTypeSize.Add(size);
                    AttrSize[KeyList[i].AttrIdx] = GetAttributeDataSize(KeyList[i]);
                }

                if (AttributeList[KeyList[i].AttrIdx].B2 == 0) AFlag = true;
            }
        }

        private void PopulateBaseHashSizeList(int index, int maskedTypeDef)
        {
            if (KeyHashSize[index] > 0)
                BaseHashSize.Add((int)KeyHashSize[index]);
            else
                BaseHashSize.Add(DatatypeHashSize[maskedTypeDef]);
        }

        private static int GetDataTypeSize(KeyItem key, int maskedTypeDef)
        {
            var val = 0;
            for (var i = 0; i < key.DataType.Count; i++)
                if (i > 0 || maskedTypeDef != 4)
                    val += KeyItemDataTypeSize[key.DataType[i]];

            if (key.AttrIdx == -1) val += 4;

            return val;
        }

        private int GetNonAttributeDataSize(KeyItem key, int maskedTypeDef)
        {
            var val = 0;
            var noneAttrCount = key.DataType.Count - AttributeList[key.AttrIdx].Count;
            for (var i = 0; i < noneAttrCount; i++)
                if (i > 0 || maskedTypeDef != 4)
                    val += KeyItemDataTypeSize[key.DataType[i]];

            if ((key.DictTypeDef & 0x60) > 0) val += 4;

            return val + 4;
        }

        private int GetAttributeDataSize(KeyItem key)
        {
            var val = 0;
            for (
                var i = key.DataType.Count - AttributeList[key.AttrIdx].Count;
                i < key.DataType.Count;
                i++
            )
                val += KeyItemDataTypeSize[key.DataType[i]];

            if ((key.DictTypeDef & 0x40) == 0) val += 4;

            return val;
        }
    }

    #endregion

    #region Binary helpers

    private static short ReadInt16(Stream fs)
    {
        var temp = new byte[2];
        fs.ReadExactly(temp, 0, 2);
        return BitConverter.ToInt16(temp, 0);
    }

    private static int ReadInt32(Stream fs)
    {
        var temp = new byte[4];
        fs.ReadExactly(temp, 0, 4);
        return BitConverter.ToInt32(temp, 0);
    }

    private static ushort ReadUInt16(Stream fs)
    {
        var temp = new byte[2];
        fs.ReadExactly(temp, 0, 2);
        return BitConverter.ToUInt16(temp, 0);
    }

    private static uint ReadUInt32(Stream fs)
    {
        var temp = new byte[4];
        fs.ReadExactly(temp, 0, 4);
        return BitConverter.ToUInt32(temp, 0);
    }

    #endregion

    #region Pinyin data

    private static readonly string[] PinyinData =
    {
        "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao",
        "bei", "ben", "beng", "bi", "bian", "biao", "bie", "bin", "bing", "bo",
        "bu", "ca", "cai", "can", "cang", "cao", "ce", "cen", "ceng", "cha",
        "chai", "chan", "chang", "chao", "che", "chen", "cheng", "chi", "chong", "chou",
        "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci", "cong",
        "cou", "cu", "cuan", "cui", "cun", "cuo", "da", "dai", "dan", "dang",
        "dao", "de", "dei", "den", "deng", "di", "dia", "dian", "diao", "die",
        "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", "e",
        "ei", "en", "eng", "er", "fa", "fan", "fang", "fei", "fen", "feng",
        "fiao", "fo", "fou", "fu", "ga", "gai", "gan", "gang", "gao", "ge",
        "gei", "gen", "geng", "gong", "gou", "gu", "gua", "guai", "guan", "guang",
        "gui", "gun", "guo", "ha", "hai", "han", "hang", "hao", "he", "hei",
        "hen", "heng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui",
        "hun", "huo", "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing",
        "jiong", "jiu", "ju", "juan", "jue", "jun", "ka", "kai", "kan", "kang",
        "kao", "ke", "kei", "ken", "keng", "kong", "kou", "ku", "kua", "kuai",
        "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan", "lang", "lao",
        "le", "lei", "leng", "li", "lia", "lian", "liang", "liao", "lie", "lin",
        "ling", "liu", "lo", "long", "lou", "lu", "luan", "lve", "lun", "luo",
        "lv", "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng",
        "mi", "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu",
        "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ni",
        "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nou", "nu",
        "nuan", "nve", "nun", "nuo", "nv", "o", "ou", "pa", "pai", "pan",
        "pang", "pao", "pei", "pen", "peng", "pi", "pian", "piao", "pie", "pin",
        "ping", "po", "pou", "pu", "qi", "qia", "qian", "qiang", "qiao", "qie",
        "qin", "qing", "qiong", "qiu", "qu", "quan", "que", "qun", "ran", "rang",
        "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", "rua", "ruan",
        "rui", "run", "ruo", "sa", "sai", "san", "sang", "sao", "se", "sen",
        "seng", "sha", "shai", "shan", "shang", "shao", "she", "shei", "shen", "sheng",
        "shi", "shou", "shu", "shua", "shuai", "shuan", "shuang", "shui", "shun", "shuo",
        "si", "song", "sou", "su", "suan", "sui", "sun", "suo", "ta", "tai",
        "tan", "tang", "tao", "te", "tei", "teng", "ti", "tian", "tiao", "tie",
        "ting", "tong", "tou", "tu", "tuan", "tui", "tun", "tuo", "wa", "wai",
        "wan", "wang", "wei", "wen", "weng", "wo", "wu", "xi", "xia", "xian",
        "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu", "xu", "xuan", "xue",
        "xun", "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo",
        "yong", "you", "yu", "yuan", "yue", "yun", "za", "zai", "zan", "zang",
        "zao", "ze", "zei", "zen", "zeng", "zha", "zhai", "zhan", "zhang", "zhao",
        "zhe", "zhei", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai",
        "zhuan", "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", "zuan",
        "zui", "zun", "zuo",
        // single letters and digits used in new format
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j",
        "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
        "u", "v", "w", "x", "y", "z",
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "#"
    };

    #endregion
}
