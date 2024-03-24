/*!
 * This work contains codes translated from the original work Sogou-User-Dict-Converter by h4x3rotab (https://github.com/h4x3rotab/Sogou-User-Dict-Converter)
 * Copyright h4x3rotab
 * Licensed under the GNU General Public License v3.0.
 //
 * This work contains codes translated from the original work rose by nopdan (https://github.com/flowerime/rose)
 * Copyright nopdan
 * Licensed under the GNU General Public License v3.0.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     搜狗二进制备份词库翻译自Python
    /// </summary>
    [ComboBoxShow(ConstantString.SOUGOU_PINYIN_BIN, ConstantString.SOUGOU_PINYIN_BIN_C, 30)]
    public class SougouPinyinBinFromPython : BaseImport, IWordLibraryImport
    {
        const int UserDictHeaderSize = 80;

        // public override CodeType CodeType { get => CodeType.NoCode; set => base.CodeType = value; }

        public WordLibraryList Import(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            // file check sum
            var header = BinFileHelper.ReadUInt32(fs);
            if (header == 0x55504753)
            {
                return NewParser(fs);
            }

            uint checksum = 0;

            var userDict = InitialiseUserDict(fs, ref checksum);
            var userHeader = ReadUserHeader(fs);
            return ReadAllWords(fs, userDict, userHeader);
        }

        #region new format @nopdan
        private WordLibraryList NewParser(FileStream fs)
        {
            var wordList = new WordLibraryList();

            fs.Seek(0x38, SeekOrigin.Begin);
            var idxBegin = BinFileHelper.ReadUInt32(fs); // index begin
            var idxSize = BinFileHelper.ReadUInt32(fs); // index size
            var wordCount = BinFileHelper.ReadUInt32(fs);
            var dictBegin = BinFileHelper.ReadUInt32(fs); // = idxBegin + idxSize
            var dictTotalSize = BinFileHelper.ReadUInt32(fs); // file total size - dictBegin
            var dictSize = BinFileHelper.ReadUInt32(fs); // effective dict size

            for (var i = 0; i < wordCount; i++)
            {
                fs.Seek(idxBegin + 4 * i, 0);

                var idx = BinFileHelper.ReadUInt32(fs);
                fs.Seek(idx + dictBegin, 0);
                var freq = BinFileHelper.ReadUInt16(fs);
                fs.Seek(2, SeekOrigin.Current);
                fs.Seek(5, SeekOrigin.Current); // unknown 5 bytes, same in every entry

                var n = BinFileHelper.ReadUInt16(fs) / 2; // pinyin size / 2
                var pinyin = new List<string>();
                for (var j = 0; j < n; j++)
                {
                    var p = BinFileHelper.ReadUInt16(fs);
                    pinyin.Add(PinyinData[p]);
                }

                fs.Seek(2, SeekOrigin.Current); // word size + code size（include idx）
                var wordSize = BinFileHelper.ReadUInt16(fs);
                var str = new byte[wordSize];
                fs.Read(str, 0, wordSize);
                string word = Encoding.Unicode.GetString(str);
                var wordLibrary = new WordLibrary()
                {
                    Word = word,
                    Rank = (int)freq,
                    PinYin = pinyin.ToArray()
                };
                // Console.WriteLine(wordLibrary);
                wordList.Add(wordLibrary);
            }
            return wordList;
        }
        #endregion

        #region private methods
        private SougouPinyinDict InitialiseUserDict(FileStream fs, ref uint checksum)
        {
            // file size
            var size = fs.Length;

            // Make sure we start from the 4th byte.
            if (fs.Position != 4)
            {
                fs.Seek(4, SeekOrigin.Begin);
            }

            var uintP4 = BinFileHelper.ReadUInt32(fs);
            var uintP8 = BinFileHelper.ReadUInt32(fs);
            var uintP12 = BinFileHelper.ReadUInt32(fs);
            var uintP16 = BinFileHelper.ReadUInt32(fs);

            checksum += uintP4 + uintP8 + uintP12 + uintP16;

            Debug.Assert(uintP4 <= size);

            // Return to the point where header begins.
            fs.Seek(20, SeekOrigin.Begin);
            // Create SG pinyin dictionary.
            var userDict = new SougouPinyinDict(
                ReadKeys(fs, uintP8),
                ReadAttributes(fs, uintP12),
                ReadAInts(fs, uintP16)
            );

            // All data in sec8 have been processed.
            // The stream pointer should be at uintP4 + 8.
            Debug.Assert(fs.Position == uintP4 + 8);

            var headerSize =
                12
                    * (
                        (userDict.AttributeList.Count)
                        + userDict.AIntList.Count
                        + userDict.KeyList.Count
                    )
                + 24;

            // Version & format, not in use; therefore, skip ahead.
            // var b2Version = BinFileHelper.ReadUInt32(fs);
            // var b2Format = BinFileHelper.ReadUInt32(fs);
            fs.Seek(8, SeekOrigin.Current);

            var totalSize = BinFileHelper.ReadUInt32(fs);

            Debug.Assert(totalSize + headerSize + uintP4 + 8 == size - UserDictHeaderSize);

            var size3B2 = BinFileHelper.ReadUInt32(fs);
            var size4B2 = BinFileHelper.ReadUInt32(fs);
            var size5B2 = BinFileHelper.ReadUInt32(fs);

            userDict.HeaderItemsIdxList = ReadHeaderItems(fs, size3B2, ref checksum);
            userDict.HeaderItemsAttrList = ReadHeaderItems(fs, size4B2, ref checksum);
            userDict.DataStore = ReadHeaderItems(fs, size5B2, ref checksum);

            // The stream pointer should be at where header ends.
            Debug.Assert(uintP4 + 8 + headerSize == fs.Position);
            userDict.DsBasePos = fs.Position;

            return userDict;
        }

        private List<SougouPinyinDict.KeyItem> ReadKeys(FileStream fs, uint length)
        {
            var list = new List<SougouPinyinDict.KeyItem>();
            // parse config
            for (var i = 0; i < length; i++)
            {
                var key = new SougouPinyinDict.KeyItem();
                key.DictTypeDef = BinFileHelper.ReadUInt16(fs);
                Debug.Assert(key.DictTypeDef < 100);
                key.DataType = new List<ushort>();
                var dataTypeCount = BinFileHelper.ReadUInt16(fs);
                for (var j = 0; j < dataTypeCount; j++)
                {
                    key.DataType.Add(BinFileHelper.ReadUInt16(fs));
                }

                key.AttrIdx = BinFileHelper.ReadInt32(fs);
                key.KeyDataIdx = BinFileHelper.ReadInt32(fs);
                key.DataIdx = BinFileHelper.ReadInt32(fs);
                key.V6 = BinFileHelper.ReadUInt32(fs);

                list.Add(key);
            }

            return list;
        }

        private List<SougouPinyinDict.AttrItem> ReadAttributes(FileStream fs, uint length)
        {
            var list = new List<SougouPinyinDict.AttrItem>();
            for (var i = 0; i < length; i++)
            {
                list.Add(
                    new SougouPinyinDict.AttrItem()
                    {
                        Count = BinFileHelper.ReadInt32(fs),
                        A2 = BinFileHelper.ReadUInt32(fs),
                        DataId = BinFileHelper.ReadInt32(fs),
                        B2 = BinFileHelper.ReadUInt32(fs)
                    }
                );
            }

            return list;
        }

        private List<uint> ReadAInts(FileStream fs, uint length)
        {
            var list = new List<uint>();
            for (var i = 0; i < length; i++)
            {
                list.Add(BinFileHelper.ReadUInt32(fs));
            }

            return list;
        }

        private List<SougouPinyinDict.HeaderItem> ReadHeaderItems(
            FileStream fs,
            uint length,
            ref uint checksum
        )
        {
            var list = new List<SougouPinyinDict.HeaderItem>();
            for (var i = 0; i < length; i++)
            {
                var h = new SougouPinyinDict.HeaderItem();
                h.Parse(fs);
                checksum += h.Offset + (uint)h.DataSize + (uint)h.UsedDataSize;
                list.Add(h);
            }

            return list;
        }

        private SougouPinyinUserHeader ReadUserHeader(FileStream fs)
        {
            // Get user header
            fs.Seek(fs.Length - 0x4c, SeekOrigin.Begin);
            var userHeader = new SougouPinyinUserHeader();
            userHeader.Parse(fs);
            return userHeader;
        }

        private WordLibraryList ReadAllWords(
            FileStream fs,
            SougouPinyinDict dict,
            SougouPinyinUserHeader userHeader
        )
        {
            var wordList = new WordLibraryList();
            var size = fs.Length;
            var keyId = 0;
            var key = dict.KeyList[keyId];
            var hashStoreBasePos = dict.GetHashStorePosition(keyId, key.DictTypeDef & 0xFFFFFF8F);
            fs.Seek(hashStoreBasePos, SeekOrigin.Begin);
            var hashStore = new HashStore();
            hashStore.EndPosition = fs.Position;

            var attrHeader = dict.HeaderItemsAttrList[key.AttrIdx];
            var attrCount =
                attrHeader.UsedDataSize == 0 ? attrHeader.DataSize : attrHeader.UsedDataSize; //总条数
            this.CountWord = attrCount;
            var hashStoreCount = dict.BaseHashSize[keyId];

            Debug.WriteLine($"Base hash size: {hashStoreCount}, Attribute count: {attrCount}");
            var processedCount = 0;
            for (var ih = 0; ih < hashStoreCount; ih++)
            {
                // Jump to the last hash store position
                fs.Seek(hashStore.EndPosition, SeekOrigin.Begin);
                hashStore = new HashStore();
                hashStore.Parse(fs);
                Debug.WriteLine(
                    $"Hash store [offset: {hashStore.Offset}, count: {hashStore.Count}]"
                );
                for (var ia = 0; ia < hashStore.Count; ia++)
                {
                    var attrBasePos = dict.GetAttriStorePositionFromIndex(
                        keyId,
                        ia,
                        hashStore.Offset
                    );
                    fs.Seek(attrBasePos + dict.DataTypeSize[keyId] - 4, SeekOrigin.Begin);
                    var offset = BinFileHelper.ReadInt32(fs);
                    for (var ia2 = 0; ia2 < attrCount; ia2++)
                    {
                        var attr2BasePos = dict.GetAttriStorePositionFromAttri(keyId, offset);
                        if (attr2BasePos > size)
                        {
                            Debug.WriteLine($"Attribute 2 out of range @ offset: {offset}");
                            break;
                        }
                        fs.Seek(attr2BasePos + dict.AttrSize[keyId] - 4, SeekOrigin.Begin);
                        offset = BinFileHelper.ReadInt32(fs);
                        //Debug.WriteLine($"id: {ia}-{ia2} pos1: {attrBasePos}; pos2: {attr2BasePos}; offset: {offset}");
                        wordList.Add(GetPyAndWord(fs, dict, attrBasePos, attr2BasePos, userHeader));
                        processedCount++;
                        this.CurrentStatus = processedCount;
                        if (offset == -1)
                            break;
                    }
                }
            }

            return wordList;
        }

        private WordLibrary GetPyAndWord(
            FileStream fs,
            SougouPinyinDict dict,
            long attriPos1,
            long attriPos2,
            SougouPinyinUserHeader userHeader
        )
        {
            fs.Seek(attriPos1, SeekOrigin.Begin);
            var pos = BinFileHelper.ReadUInt32(fs);
            var pinyin = decryptPinyin(fs, dict.GetPyPosition(pos));
            var word = new Word() { Attribute = new WordAttribute(), Pinyin = pinyin };
            fs.Seek(attriPos2, SeekOrigin.Begin);
            word.Attribute.Parse(fs);

            var dataId = dict.GetDataIdByAttriId(dict.KeyList[0].AttrIdx);
            fs.Seek(dict.GetDataPosition(dataId, word.Attribute.Offset), SeekOrigin.Begin);
            word.WordByte = decryptWords(fs, word.Attribute.P1, userHeader.P2, userHeader.P3);
            var wordLibrary = new WordLibrary()
            {
                Word = word.WordString,
                Rank = (int)word.Attribute.Frequency,
                PinYin = word.Pinyin
            };

            return wordLibrary;
        }

        #region decryptPinyin
        private string[] decryptPinyin(FileStream fs, long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);

            var n = BinFileHelper.ReadUInt16(fs) / 2;
            var pinyin = new List<string>();
            for (var i = 0; i < n; i++)
            {
                var p = BinFileHelper.ReadUInt16(fs);
                pinyin.Add(PinyinData[p]);
            }
            return pinyin.ToArray();
        }

        string[] PinyinData =
        {
            "a",
            "ai",
            "an",
            "ang",
            "ao",
            "ba",
            "bai",
            "ban",
            "bang",
            "bao",
            "bei",
            "ben",
            "beng",
            "bi",
            "bian",
            "biao",
            "bie",
            "bin",
            "bing",
            "bo",
            "bu",
            "ca",
            "cai",
            "can",
            "cang",
            "cao",
            "ce",
            "cen",
            "ceng",
            "cha",
            "chai",
            "chan",
            "chang",
            "chao",
            "che",
            "chen",
            "cheng",
            "chi",
            "chong",
            "chou",
            "chu",
            "chua",
            "chuai",
            "chuan",
            "chuang",
            "chui",
            "chun",
            "chuo",
            "ci",
            "cong",
            "cou",
            "cu",
            "cuan",
            "cui",
            "cun",
            "cuo",
            "da",
            "dai",
            "dan",
            "dang",
            "dao",
            "de",
            "dei",
            "den",
            "deng",
            "di",
            "dia",
            "dian",
            "diao",
            "die",
            "ding",
            "diu",
            "dong",
            "dou",
            "du",
            "duan",
            "dui",
            "dun",
            "duo",
            "e",
            "ei",
            "en",
            "eng",
            "er",
            "fa",
            "fan",
            "fang",
            "fei",
            "fen",
            "feng",
            "fiao",
            "fo",
            "fou",
            "fu",
            "ga",
            "gai",
            "gan",
            "gang",
            "gao",
            "ge",
            "gei",
            "gen",
            "geng",
            "gong",
            "gou",
            "gu",
            "gua",
            "guai",
            "guan",
            "guang",
            "gui",
            "gun",
            "guo",
            "ha",
            "hai",
            "han",
            "hang",
            "hao",
            "he",
            "hei",
            "hen",
            "heng",
            "hong",
            "hou",
            "hu",
            "hua",
            "huai",
            "huan",
            "huang",
            "hui",
            "hun",
            "huo",
            "ji",
            "jia",
            "jian",
            "jiang",
            "jiao",
            "jie",
            "jin",
            "jing",
            "jiong",
            "jiu",
            "ju",
            "juan",
            "jue",
            "jun",
            "ka",
            "kai",
            "kan",
            "kang",
            "kao",
            "ke",
            "kei",
            "ken",
            "keng",
            "kong",
            "kou",
            "ku",
            "kua",
            "kuai",
            "kuan",
            "kuang",
            "kui",
            "kun",
            "kuo",
            "la",
            "lai",
            "lan",
            "lang",
            "lao",
            "le",
            "lei",
            "leng",
            "li",
            "lia",
            "lian",
            "liang",
            "liao",
            "lie",
            "lin",
            "ling",
            "liu",
            "lo",
            "long",
            "lou",
            "lu",
            "luan",
            "lve",
            "lun",
            "luo",
            "lv",
            "ma",
            "mai",
            "man",
            "mang",
            "mao",
            "me",
            "mei",
            "men",
            "meng",
            "mi",
            "mian",
            "miao",
            "mie",
            "min",
            "ming",
            "miu",
            "mo",
            "mou",
            "mu",
            "na",
            "nai",
            "nan",
            "nang",
            "nao",
            "ne",
            "nei",
            "nen",
            "neng",
            "ni",
            "nian",
            "niang",
            "niao",
            "nie",
            "nin",
            "ning",
            "niu",
            "nong",
            "nou",
            "nu",
            "nuan",
            "nve",
            "nun",
            "nuo",
            "nv",
            "o",
            "ou",
            "pa",
            "pai",
            "pan",
            "pang",
            "pao",
            "pei",
            "pen",
            "peng",
            "pi",
            "pian",
            "piao",
            "pie",
            "pin",
            "ping",
            "po",
            "pou",
            "pu",
            "qi",
            "qia",
            "qian",
            "qiang",
            "qiao",
            "qie",
            "qin",
            "qing",
            "qiong",
            "qiu",
            "qu",
            "quan",
            "que",
            "qun",
            "ran",
            "rang",
            "rao",
            "re",
            "ren",
            "reng",
            "ri",
            "rong",
            "rou",
            "ru",
            "rua",
            "ruan",
            "rui",
            "run",
            "ruo",
            "sa",
            "sai",
            "san",
            "sang",
            "sao",
            "se",
            "sen",
            "seng",
            "sha",
            "shai",
            "shan",
            "shang",
            "shao",
            "she",
            "shei",
            "shen",
            "sheng",
            "shi",
            "shou",
            "shu",
            "shua",
            "shuai",
            "shuan",
            "shuang",
            "shui",
            "shun",
            "shuo",
            "si",
            "song",
            "sou",
            "su",
            "suan",
            "sui",
            "sun",
            "suo",
            "ta",
            "tai",
            "tan",
            "tang",
            "tao",
            "te",
            "tei",
            "teng",
            "ti",
            "tian",
            "tiao",
            "tie",
            "ting",
            "tong",
            "tou",
            "tu",
            "tuan",
            "tui",
            "tun",
            "tuo",
            "wa",
            "wai",
            "wan",
            "wang",
            "wei",
            "wen",
            "weng",
            "wo",
            "wu",
            "xi",
            "xia",
            "xian",
            "xiang",
            "xiao",
            "xie",
            "xin",
            "xing",
            "xiong",
            "xiu",
            "xu",
            "xuan",
            "xue",
            "xun",
            "ya",
            "yan",
            "yang",
            "yao",
            "ye",
            "yi",
            "yin",
            "ying",
            "yo",
            "yong",
            "you",
            "yu",
            "yuan",
            "yue",
            "yun",
            "za",
            "zai",
            "zan",
            "zang",
            "zao",
            "ze",
            "zei",
            "zen",
            "zeng",
            "zha",
            "zhai",
            "zhan",
            "zhang",
            "zhao",
            "zhe",
            "zhei",
            "zhen",
            "zheng",
            "zhi",
            "zhong",
            "zhou",
            "zhu",
            "zhua",
            "zhuai",
            "zhuan",
            "zhuang",
            "zhui",
            "zhun",
            "zhuo",
            "zi",
            "zong",
            "zou",
            "zu",
            "zuan",
            "zui",
            "zun",
            "zuo",
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "#"
        };
        #endregion

        private byte[] decryptWords(FileStream fs, uint p1, uint p2, uint p3)
        {
            var xk = (((p1 + p2) << 2) + ((p1 + p3) << 2)) & 0xffff;
            var n = BinFileHelper.ReadUInt16(fs) / 2;
            var decwords = new byte[n * 4];
            for (var i = 0; i < n; i++)
            {
                var shift = (int)(p2 % 8);
                var ch = BinFileHelper.ReadUInt16(fs);
                var dch = (ch << (16 - (shift % 8)) | (ch >> shift)) & 0xffff;
                dch ^= (int)xk;
                Buffer.BlockCopy(BitConverter.GetBytes(dch), 0, decwords, i * 4, 4);
            }

            return decwords;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException("搜狗Bin文件为二进制文件，不支持");
        }
        #endregion

        #region structures
        public struct SougouPinyinUserHeader
        {
            public uint P2;
            public uint P3;

            public void Parse(FileStream fs)
            {
                fs.Seek(fs.Position + 56, SeekOrigin.Begin);
                P2 = BinFileHelper.ReadUInt32(fs);
                P3 = BinFileHelper.ReadUInt32(fs);
                //fs.Seek(fs.Position + 12, SeekOrigin.Begin);
            }
        }

        public struct HashStore
        {
            public int Offset;
            public int Count;
            public long EndPosition;

            public void Parse(FileStream fs)
            {
                Offset = BinFileHelper.ReadInt32(fs);
                Count = BinFileHelper.ReadInt32(fs);
                EndPosition = fs.Position;
            }
        }

        public struct WordAttribute
        {
            public int Offset;
            public uint Frequency;
            public uint AFlag;
            public uint I8;
            public uint P1;
            public int IE;

            public void Parse(FileStream fs)
            {
                Offset = BinFileHelper.ReadInt32(fs);
                Frequency = BinFileHelper.ReadUInt16(fs);
                AFlag = BinFileHelper.ReadUInt16(fs);
                I8 = BinFileHelper.ReadUInt32(fs);
                P1 = BinFileHelper.ReadUInt16(fs);
                IE = BinFileHelper.ReadInt32(fs);

                // Advance
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        public struct Word
        {
            public byte[] WordByte;
            public string WordString => Encoding.UTF32.GetString(WordByte);
            public WordAttribute Attribute;
            public string[] Pinyin;

            public override string ToString()
            {
                return $"{WordString}, frequency {Attribute.Frequency}, pinyin {String.Join("'", Pinyin)}";
            }
        }
        #endregion
    }
}
