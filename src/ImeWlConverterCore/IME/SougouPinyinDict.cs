/*!
 * This work contains codes translated from the original work Sogou-User-Dict-Converter by h4x3rotab (https://github.com/h4x3rotab/Sogou-User-Dict-Converter)
 * Copyright h4x3rotab
 * Licensed under the GNU General Public License v3.0.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    public class SougouPinyinDict
    {
        public static readonly ReadOnlyCollection<int> DatatypeHashSize = new List<int>
        {
            0,
            27,
            414,
            512,
            -1,
            -1,
            512,
            0
        }.AsReadOnly();
        public static readonly ReadOnlyCollection<int> KeyItemDataTypeSize = new List<int>
        {
            4,
            1,
            1,
            2,
            1,
            2,
            2,
            4,
            4,
            8,
            4,
            4,
            4,
            0,
            0,
            0
        }.AsReadOnly();

        #region structures
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

            public void Parse(FileStream fs)
            {
                Offset = BinFileHelper.ReadUInt32(fs);
                DataSize = BinFileHelper.ReadInt32(fs);
                UsedDataSize = BinFileHelper.ReadInt32(fs);
            }
        }
        #endregion

        #region public fields
        public List<KeyItem> KeyList;
        public List<AttrItem> AttributeList;
        public List<uint> AIntList;
        public List<HeaderItem> HeaderItemsIdxList;
        public List<HeaderItem> HeaderItemsAttrList;
        public List<HeaderItem> DataStore;
        public long DsBasePos;
        public List<int> DataTypeSize;
        public int[] AttrSize;
        public List<int> BaseHashSize;
        public uint[] KeyHashSize = new uint[10];
        public bool AFlag;
        #endregion

        #region constructors
        public SougouPinyinDict() { }

        public SougouPinyinDict(
            List<KeyItem> keyList,
            List<AttrItem> attributeList,
            List<uint> aIntList
        )
        {
            KeyList = keyList;
            AttributeList = attributeList;
            AIntList = aIntList;
            KeyHashSize[0] = 500;

            Initialise();
        }
        #endregion

        #region public methods

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
            if (header.UsedDataSize > 0 && offset > header.UsedDataSize)
            {
                Debug.WriteLine(
                    $"GetData overflow data_id: {dataId} offset: {offset}\nheader [ used: {header.UsedDataSize} size: {header.DataSize} ]"
                );
            }

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

        #endregion

        #region private methods
        private void Initialise()
        {
            DataTypeSize = new List<int>();
            BaseHashSize = new List<int>();
            AttrSize = new int[AttributeList.Count];

            for (var i = 0; i < KeyList.Count; i++)
            {
                var size = (KeyList[i].DictTypeDef >> 2) & 4;
                var maskedTypeDef = (int)(KeyList[i].DictTypeDef & 0xFFFFFF8F);

                PopulateBaseHashSizList(i, maskedTypeDef);

                if (KeyList[i].AttrIdx < 0)
                {
                    size += GetDataTypesize(KeyList[i], maskedTypeDef);
                    DataTypeSize.Add(size);
                }
                else
                {
                    size += GetNonAttributeDataSize(KeyList[i], maskedTypeDef);
                    DataTypeSize.Add(size);
                    AttrSize[KeyList[i].AttrIdx] = GetAttributeDataSize(KeyList[i]);
                }

                if (AttributeList[KeyList[i].AttrIdx].B2 == 0)
                {
                    AFlag = true;
                }
            }
        }

        private void PopulateBaseHashSizList(int index, int maskedTypeDef)
        {
            if (KeyHashSize[index] > 0)
            {
                BaseHashSize.Add((int)KeyHashSize[index]);
            }
            else
            {
                BaseHashSize.Add(DatatypeHashSize[maskedTypeDef]);
            }
        }

        private int GetDataTypesize(KeyItem key, int maskedTypeDef)
        {
            var val = 0;
            for (var i = 0; i < key.DataType.Count; i++)
            {
                if (i > 0 || maskedTypeDef != 4)
                {
                    val += KeyItemDataTypeSize[key.DataType[i]];
                }
            }

            if (key.AttrIdx == -1)
            {
                val += 4;
            }

            return val;
        }

        private int GetNonAttributeDataSize(KeyItem key, int maskedTypeDef)
        {
            var val = 0;
            var noneAttrCount = key.DataType.Count - AttributeList[key.AttrIdx].Count;
            for (var i = 0; i < noneAttrCount; i++)
            {
                if (i > 0 || maskedTypeDef != 4)
                {
                    val += KeyItemDataTypeSize[key.DataType[i]];
                }
            }

            if ((key.DictTypeDef & 0x60) > 0)
            {
                val += 4;
            }

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
            {
                val += KeyItemDataTypeSize[key.DataType[i]];
            }

            if ((key.DictTypeDef & 0x40) == 0)
            {
                val += 4;
            }

            return val;
        }
        #endregion
    }
}
