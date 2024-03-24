/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.LINGOES_LD2, ConstantString.LINGOES_LD2_C, 200)]
    public class LingoesLd2 : BaseImport, IWordLibraryImport
    {
        //private readonly Encoding[] AVAIL_ENCODINGS = new[]
        //    {
        //        Encoding.UTF8,
        //        Encoding.Unicode,
        //        Encoding.BigEndianUnicode,
        //        Encoding.GetEncoding("euc-jp")
        //    };

        private readonly Regex regex = new Regex("[\u4E00-\u9FA5]+");

        public LingoesLd2()
        {
            WordEncoding = Encoding.UTF8;
            XmlEncoding = Encoding.UTF8;
            IncludeMeaning = false;
            CodeType = CodeType.English;
        }

        /// <summary>
        ///     词汇的编码
        /// </summary>
        public Encoding WordEncoding { get; set; }

        /// <summary>
        ///     解释的编码
        /// </summary>
        public Encoding XmlEncoding { get; set; }

        /// <summary>
        ///     在导出的Word内容中是否包含注释，默认不包含
        /// </summary>
        public bool IncludeMeaning { get; set; }

        #region IWordLibraryImport Members

        public WordLibraryList Import(string path)
        {
            //IWordCodeGenerater pinyinFactory = new PinyinGenerater();
            IList<string> words = Parse(path);
            var wll = new WordLibraryList();
            foreach (string word in words)
            {
                var wl = new WordLibrary();
                //词典转换，不进行注音操作，以提高速度
                //if (IsChinese(word)) //是中文就要进行注音
                //{
                //    var list = pinyinFactory.GetCodeOfString(word);
                //    wl.PinYin = CollectionHelper.ToArray(list);
                //}
                //else
                {
                    wl.IsEnglish = true;
                }
                wl.Word = word;
                wl.Rank = DefaultRank;
                wll.Add(wl);
            }
            return wll;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException();
        }

        public override bool IsText
        {
            get { return false; }
        }

        #endregion

        private bool IsChinese(string str)
        {
            return regex.IsMatch(str);
        }

        private IList<string> Parse(string ld2File)
        {
            using (var fs = new FileStream(ld2File, FileMode.Open, FileAccess.Read))
            {
                Debug.WriteLine("文件：" + ld2File);
                byte[] bs = BinFileHelper.ReadArray(fs, 4);
                string v = Encoding.ASCII.GetString(bs);
                Debug.WriteLine("类型：" + v);
                fs.Position = 0x18;
                Debug.WriteLine(
                    "版本：" + BinFileHelper.ReadInt16(fs) + "." + BinFileHelper.ReadInt16(fs)
                );
                Debug.WriteLine("ID: 0x" + (BinFileHelper.ReadInt64(fs).ToString("x")));

                fs.Position = 0x5c;
                int offsetData = BinFileHelper.ReadInt32(fs) + 0x60;
                if (fs.Length > offsetData)
                {
                    Debug.WriteLine("简介地址：0x" + (offsetData).ToString("x"));
                    fs.Position = offsetData;
                    int type = BinFileHelper.ReadInt32(fs);
                    Debug.WriteLine("简介类型：0x" + (type).ToString("x"));
                    fs.Position = offsetData + 4;
                    int offsetWithInfo = BinFileHelper.ReadInt32(fs) + offsetData + 12;
                    if (type == 3)
                    {
                        // without additional information
                        return ReadDictionary(fs, offsetData);
                    }
                    if (fs.Length > offsetWithInfo - 0x1C)
                    {
                        return ReadDictionary(fs, offsetWithInfo);
                    }
                    Debug.WriteLine("文件不包含字典数据。网上字典？");
                }
                else
                {
                    Debug.WriteLine("文件不包含字典数据。网上字典？");
                }

                return null;
            }
        }

        private IList<string> ReadDictionary(FileStream fs, int offsetWithIndex)
        {
            fs.Position = offsetWithIndex;
            int type = BinFileHelper.ReadInt32(fs);
            Debug.WriteLine("词典类型：0x" + type);
            int limit = BinFileHelper.ReadInt32(fs) + offsetWithIndex + 8; //文件结束地址
            int offsetIndex = offsetWithIndex + 0x1C; //索引开始的地址
            int offsetCompressedDataHeader = BinFileHelper.ReadInt32(fs) + offsetIndex; //索引结束，数据头地址
            int inflatedWordsIndexLength = BinFileHelper.ReadInt32(fs);
            int inflatedWordsLength = BinFileHelper.ReadInt32(fs);
            int inflatedXmlLength = BinFileHelper.ReadInt32(fs);
            int definitions = (offsetCompressedDataHeader - offsetIndex) / 4;
            var deflateStreams = new List<int>();
            fs.Position = offsetCompressedDataHeader + 8;
            int offset = BinFileHelper.ReadInt32(fs);
            while (offset + fs.Position < limit)
            {
                offset = BinFileHelper.ReadInt32(fs);
                deflateStreams.Add(offset);
            }
            long offsetCompressedData = fs.Position;
            Debug.WriteLine("索引词组数目：" + definitions);

            //CountWord = definitions;

            Debug.WriteLine(
                "索引地址/大小：0x"
                    + offsetIndex.ToString("x")
                    + " / "
                    + (offsetCompressedDataHeader - offsetIndex).ToString("x")
                    + " B"
            );
            Debug.WriteLine(
                "压缩数据地址/大小：0x"
                    + (offsetCompressedData).ToString("x")
                    + " / "
                    + (limit - offsetCompressedData).ToString("x")
                    + " B"
            );
            Debug.WriteLine(
                "词组索引地址/大小（解压缩后）：0x0 / " + inflatedWordsIndexLength.ToString("x") + " B"
            );
            Debug.WriteLine(
                "词组地址/大小（解压缩后）：0x"
                    + (inflatedWordsIndexLength).ToString("x")
                    + " / "
                    + inflatedWordsLength.ToString("x")
                    + " B"
            );
            Debug.WriteLine(
                "XML地址/大小（解压缩后）：0x"
                    + (inflatedWordsIndexLength + inflatedWordsLength).ToString("x")
                    + " / "
                    + inflatedXmlLength.ToString("x")
                    + " B"
            );
            Debug.WriteLine(
                "文件大小（解压缩后）："
                    + (inflatedWordsIndexLength + inflatedWordsLength + inflatedXmlLength) / 1024
                    + " KB"
            );

            byte[] inflatedFile = Inflate(fs, offsetCompressedData, deflateStreams);

            //fs.Position = offsetIndex;
            //var idxArray = new int[definitions];
            //for (int i = 0; i < definitions; i++)
            //{
            //    idxArray[i] = BinFileHelper.ReadInt32(fs);
            //}


            return Extract(
                inflatedFile,
                inflatedWordsIndexLength,
                inflatedWordsIndexLength + inflatedWordsLength
            );
        }

        #region 解压

        private byte[] Inflate(FileStream dataRawBytes, long startP, List<int> deflateStreams)
        {
            Debug.WriteLine("解压缩'" + deflateStreams.Count + "'个数据流至'" + "'。。。");
            var temp = new List<byte>();
            long startOffset = startP;
            long offset = -1;
            long lastOffset = startOffset;
            try
            {
                foreach (int offsetRelative in deflateStreams)
                {
                    offset = startOffset + offsetRelative;
                    temp.AddRange(Decompress(dataRawBytes, lastOffset, offset - lastOffset));

                    lastOffset = offset;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("解压缩失败: 0x" + (offset) + ": " + e.Message);
            }
            Debug.WriteLine("解压成功完成!");
            return temp.ToArray();
        }

        private List<byte> Decompress(FileStream data, long offset, long length)
        {
            var t = new List<byte>();
            var inflator = new Inflater();
            Stream stream = CopyStream(data, offset, (int)length);
            var in1 = new InflaterInputStream(stream, inflator, 1024 * 8);

            var buffer = new byte[1024 * 8];
            int len;
            while ((len = in1.Read(buffer, 0, 1024 * 8)) > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    t.Add(buffer[i]);
                }
            }
            return t;
        }

        private Stream CopyStream(Stream stream, long offset, int length)
        {
            stream.Position = offset;
            byte[] bs = StreamToBytes(stream, length);
            return BytesToStream(bs);
        }

        private byte[] StreamToBytes(Stream stream, int length)
        {
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// 将 byte[] 转成 Stream
        private Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        #endregion

        #region 解析

        /// <summary>
        ///     判断字典中词汇的编码和解释的编码
        /// </summary>
        /// <param name="inflatedBytes"></param>
        /// <param name="offsetWords"></param>
        /// <param name="offsetXml"></param>
        /// <param name="defTotal"></param>
        /// <param name="dataLen"></param>
        /// <returns></returns>
        //private Encoding[] DetectEncodings(byte[] inflatedBytes, int offsetWords, int offsetXml, int defTotal,
        //                                   int dataLen)
        //{
        //    return new[] { AVAIL_ENCODINGS[1], AVAIL_ENCODINGS[0] };
        //    int test = Math.Min(defTotal, 10);

        //    for (int j = 0; j < AVAIL_ENCODINGS.Length; j++)
        //    {
        //        for (int k = 0; k < AVAIL_ENCODINGS.Length; k++)
        //        {
        //            try
        //            {
        //                for (int i = 0; i < test; i++)
        //                {
        //                    ReadDefinitionData(inflatedBytes, offsetWords, offsetXml, dataLen, AVAIL_ENCODINGS[j],
        //                                       AVAIL_ENCODINGS[k], i);
        //                }
        //                Debug.WriteLine("词组编码：" + AVAIL_ENCODINGS[j]);
        //                Debug.WriteLine("XML编码：" + AVAIL_ENCODINGS[k]);
        //                return new[] {AVAIL_ENCODINGS[j], AVAIL_ENCODINGS[k]};
        //            }
        //            catch (Exception ex)
        //            {
        //                // ignore
        //                Debug.WriteLine("There are some error:" + ex.Message);
        //            }
        //        }
        //    }
        //    Debug.WriteLine("自动识别编码失败！选择UTF-16LE继续。");
        //    return new[] {AVAIL_ENCODINGS[1], AVAIL_ENCODINGS[1]};
        //}
        private readonly Regex hregex = new Regex(@"<(.[^>]*)>");

        /// <summary>
        ///     解析解压后的数据，返回词汇列表
        /// </summary>
        /// <param name="dataRawBytes"></param>
        /// <param name="offsetDefs"></param>
        /// <param name="offsetXml"></param>
        /// <returns></returns>
        private IList<string> Extract(byte[] dataRawBytes, int offsetDefs, int offsetXml)
        {
            int dataLen = 10;
            int defTotal = offsetDefs / dataLen - 1;
            CountWord = defTotal;
            var words = new string[defTotal];
#if DEBUG
            StreamWriter sw = new StreamWriter("C:\\Temp\\灵格斯.txt", false);
#endif
            //Encoding[] encodings = DetectEncodings(dataRawBytes, offsetDefs, offsetXml, defTotal, dataLen);

            //dataRawBytes.Position = (8);
            //int counter = 0;
            CurrentStatus = 0;
            for (int i = 0; i < defTotal; i++)
            {
                KeyValuePair<string, string> kv = ReadDefinitionData(
                    dataRawBytes,
                    offsetDefs,
                    offsetXml,
                    dataLen,
                    WordEncoding,
                    XmlEncoding,
                    i
                );

                string word = kv.Key;
                string xml = kv.Value;
                if (IncludeMeaning)
                {
                    words[i] = word + "\t" + (xml);
                }
                else
                {
                    words[i] = word;
                }
#if DEBUG
                sw.WriteLine(kv.Key + "\t" + kv.Value);
#endif
                CurrentStatus++;
            }

#if DEBUG
            sw.Close();
#endif
            Debug.WriteLine("成功读出" + CurrentStatus + "组数据。");
            return new List<string>(words);
        }

        /// <summary>
        ///     读取一个词汇的词和解释
        /// </summary>
        /// <param name="inflatedBytes"></param>
        /// <param name="offsetWords"></param>
        /// <param name="offsetXml"></param>
        /// <param name="dataLen"></param>
        /// <param name="wordStringDecoder"></param>
        /// <param name="xmlStringDecoder"></param>
        /// <param name="i"></param>
        /// <returns>Key为词汇，Value为解释</returns>
        private KeyValuePair<string, string> ReadDefinitionData(
            byte[] inflatedBytes,
            int offsetWords,
            int offsetXml,
            int dataLen,
            Encoding wordStringDecoder,
            Encoding xmlStringDecoder,
            int i
        )
        {
            var idxData = new int[6];
            GetIdxData(inflatedBytes, dataLen * i, idxData);
            int lastWordPos = idxData[0];
            int lastXmlPos = idxData[1];
            int flags = idxData[2];
            int refs = idxData[3];
            int currentWordOffset = idxData[4];
            int currenXmlOffset = idxData[5];

            string xml = xmlStringDecoder.GetString(
                inflatedBytes,
                offsetXml + lastXmlPos,
                currenXmlOffset - lastXmlPos
            );
            while (refs-- > 0)
            {
                int position = (offsetWords + lastWordPos);
                int ref1 = BitConverter.ToInt32(inflatedBytes, position);
                GetIdxData(inflatedBytes, dataLen * ref1, idxData);
                lastXmlPos = idxData[1];
                currenXmlOffset = idxData[5];
                if (string.IsNullOrEmpty(xml))
                {
                    xml = xmlStringDecoder.GetString(
                        inflatedBytes,
                        offsetXml + lastXmlPos,
                        currenXmlOffset - lastXmlPos
                    );
                }
                else
                {
                    xml =
                        xmlStringDecoder.GetString(
                            inflatedBytes,
                            offsetXml + lastXmlPos,
                            currenXmlOffset - lastXmlPos
                        )
                        + ", "
                        + xml;
                }
                lastWordPos += 4;
            }
            //defData[1] = xml;

            int position1 = offsetWords + lastWordPos;

            byte[] w = BinFileHelper.ReadArray(
                inflatedBytes,
                position1,
                currentWordOffset - lastWordPos
            );
            string word = wordStringDecoder.GetString(w);
            //defData[0] = word;
            return new KeyValuePair<string, string>(word, xml);
        }

        private void GetIdxData(byte[] dataRawBytes, int position, int[] wordIdxData)
        {
            wordIdxData[0] = BitConverter.ToInt32(dataRawBytes, position);
            wordIdxData[1] = BitConverter.ToInt32(dataRawBytes, position + 4);
            wordIdxData[2] = dataRawBytes[position + 8] & 0xff;
            wordIdxData[3] = dataRawBytes[position + 9] & 0xff;
            wordIdxData[4] = BitConverter.ToInt32(dataRawBytes, position + 10);
            wordIdxData[5] = BitConverter.ToInt32(dataRawBytes, position + 14);
        }

        private string RemoveHtmlTag(string html)
        {
            return hregex.Replace(html, "");
        }

        #endregion
    }
}
