using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.LINGOES_LD2, ConstantString.LINGOES_LD2_C, 200)]
    public class LingoesLd2 : BaseImport, IWordLibraryImport
    {
        private readonly Encoding[] AVAIL_ENCODINGS = new[]
            {
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.BigEndianUnicode,
                Encoding.GetEncoding("euc-jp")
            };

        private readonly Regex regex = new Regex("[\u4E00-\u9FA5]+");

        #region IWordLibraryImport Members

        public override bool IsText
        {
            get { return false; }
        }

        #endregion

        public WordLibraryList Import(string path)
        {
            IWordCodeGenerater pinyinFactory = new PinyinGenerater();
            IList<string> words = Parse(path);
            var wll = new WordLibraryList();
            foreach (string word in words)
            {
                var wl = new WordLibrary();
                if (IsChinese(word)) //是中文就要进行注音
                {
                    IList<string> list = pinyinFactory.GetCodeOfString(word);
                    wl.PinYin = CollectionHelper.ToArray(list);
                }
                else
                {
                    wl.IsEnglish = true;
                }
                wl.Word = word;
                wl.Count = DefaultRank;
                wll.Add(wl);
            }
            return wll;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException();
        }

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
                Debug.WriteLine("版本：" + BinFileHelper.ReadInt16(fs) + "." + BinFileHelper.ReadInt16(fs));
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
                    else if (fs.Length > offsetWithInfo - 0x1C)
                    {
                        return ReadDictionary(fs, offsetWithInfo);
                    }
                    else
                    {
                        Debug.WriteLine("文件不包含字典数据。网上字典？");
                    }
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
            int definitions = (offsetCompressedDataHeader - offsetIndex)/4;
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

            Debug.WriteLine("索引地址/大小：0x" + offsetIndex.ToString("x") + " / "
                            + (offsetCompressedDataHeader - offsetIndex).ToString("x") + " B");
            Debug.WriteLine("压缩数据地址/大小：0x" + (offsetCompressedData).ToString("x") + " / "
                            + (limit - offsetCompressedData).ToString("x") + " B");
            Debug.WriteLine("词组索引地址/大小（解压缩后）：0x0 / " + inflatedWordsIndexLength.ToString("x") + " B");
            Debug.WriteLine("词组地址/大小（解压缩后）：0x" + (inflatedWordsIndexLength).ToString("x") + " / "
                            + inflatedWordsLength.ToString("x") + " B");
            Debug.WriteLine("XML地址/大小（解压缩后）：0x" + (inflatedWordsIndexLength + inflatedWordsLength).ToString("x")
                            + " / " + inflatedXmlLength.ToString("x") + " B");
            Debug.WriteLine("文件大小（解压缩后）：" + (inflatedWordsIndexLength + inflatedWordsLength + inflatedXmlLength)/1024
                            + " KB");

            byte[] inflatedFile = Inflate(fs, offsetCompressedData, deflateStreams);


            //String indexFile = ld2File + ".idx";
            //String extractedFile = ld2File + ".words";
            //String extractedXmlFile = ld2File + ".xml";
            //String extractedOutputFile = ld2File + ".output";

            fs.Position = offsetIndex;
            var idxArray = new int[definitions];
            for (int i = 0; i < definitions; i++)
            {
                idxArray[i] = BinFileHelper.ReadInt32(fs);
            }


            return Extract(inflatedFile, idxArray, inflatedWordsIndexLength,
                           inflatedWordsIndexLength + inflatedWordsLength);
        }


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
            Stream stream = CopyStream(data, offset, (int) length);
            var in1 = new InflaterInputStream(stream, inflator, 1024*8);

            var buffer = new byte[1024*8];
            int len;
            while ((len = in1.Read(buffer, 0, 1024*8)) > 0)
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

        private IList<string> Extract(byte[] inflatedFile, int[] idxArray, int offsetDefs, int offsetXml)
        {
            //Debug.WriteLine("写入'" + extractedOutputFile + "'。。。");

            //StreamWriter indexWriter = new StreamWriter(indexFile);
            //StreamWriter defsWriter = new StreamWriter(extractedWordsFile);
            //StreamWriter xmlWriter = new StreamWriter(extractedXmlFile);
            //StreamWriter outputWriter = new StreamWriter(extractedOutputFile);
            // read inflated data


            byte[] dataRawBytes = inflatedFile;

            int dataLen = 10;
            int defTotal = offsetDefs/dataLen - 1;
            CountWord = defTotal;
            var words = new string[defTotal];
            var idxData = new int[6];
            var defData = new String[2];

            Encoding[] encodings = DetectEncodings(dataRawBytes, offsetDefs, offsetXml, defTotal, dataLen, idxData,
                                                   defData);

            //dataRawBytes.Position = (8);
            //int counter = 0;
            CurrentStatus = 0;
            for (int i = 0; i < defTotal; i++)
            {
                ReadDefinitionData(dataRawBytes, offsetDefs, offsetXml, dataLen, encodings[0], encodings[1], idxData,
                                   defData, i);

                words[i] = defData[0];
                string xml = defData[1];
                //defsWriter.Write(defData[0]);
                //defsWriter.Write("\n");

                //xmlWriter.Write(defData[1]);
                //xmlWriter.Write("\n");

                //outputWriter.Write(defData[0]);
                //outputWriter.Write("=");
                //outputWriter.Write(defData[1]);
                //outputWriter.Write("\n");

                //Debug.WriteLine(defData[0] + " = " + defData[1]);
                CurrentStatus++;
            }

            //for (int i = 0; i < idxArray.Length; i++)
            //{
            //    int idx = idxArray[i];
            //    indexWriter.Write(words[idx]);
            //    indexWriter.Write(", ");
            //    indexWriter.Write((idx));
            //    indexWriter.Write("\n");
            //}
            //indexWriter.Close();
            //defsWriter.Close();
            //xmlWriter.Close();
            //outputWriter.Close();
            Debug.WriteLine("成功读出" + CurrentStatus + "组数据。");
            return new List<string>(words);
        }

        private void ReadDefinitionData(byte[] inflatedBytes, int offsetWords,
                                        int offsetXml, int dataLen, Encoding wordStringDecoder,
                                        Encoding xmlStringDecoder, int[] idxData, string[] defData, int i)
        {
            GetIdxData(inflatedBytes, dataLen*i, idxData);
            int lastWordPos = idxData[0];
            int lastXmlPos = idxData[1];
            int flags = idxData[2];
            int refs = idxData[3];
            int currentWordOffset = idxData[4];
            int currenXmlOffset = idxData[5];


            string xml = xmlStringDecoder.GetString(inflatedBytes, offsetXml + lastXmlPos, currenXmlOffset - lastXmlPos);
            while (refs-- > 0)
            {
                int position = (offsetWords + lastWordPos);
                int ref1 = BitConverter.ToInt32(inflatedBytes, position);
                GetIdxData(inflatedBytes, dataLen*ref1, idxData);
                lastXmlPos = idxData[1];
                currenXmlOffset = idxData[5];
                if (string.IsNullOrEmpty(xml))
                {
                    xml = xmlStringDecoder.GetString(inflatedBytes, offsetXml + lastXmlPos, currenXmlOffset - lastXmlPos);
                }
                else
                {
                    xml =
                        xmlStringDecoder.GetString(inflatedBytes, offsetXml + lastXmlPos, currenXmlOffset - lastXmlPos) +
                        ", " + xml;
                }
                lastWordPos += 4;
            }
            defData[1] = xml;

            int position1 = offsetWords + lastWordPos;

            byte[] w = BinFileHelper.ReadArray(inflatedBytes, position1, currentWordOffset - lastWordPos);
            string word = wordStringDecoder.GetString(w);
            defData[0] = word;
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


        private Encoding[] DetectEncodings(byte[] inflatedBytes, int offsetWords, int offsetXml, int defTotal,
                                           int dataLen, int[] idxData, String[] defData)
        {
            int test = Math.Min(defTotal, 10);

            for (int j = 0; j < AVAIL_ENCODINGS.Length; j++)
            {
                for (int k = 0; k < AVAIL_ENCODINGS.Length; k++)
                {
                    try
                    {
                        for (int i = 0; i < test; i++)
                        {
                            ReadDefinitionData(inflatedBytes, offsetWords, offsetXml, dataLen, AVAIL_ENCODINGS[j],
                                               AVAIL_ENCODINGS[k], idxData, defData, i);
                        }
                        Debug.WriteLine("词组编码：" + AVAIL_ENCODINGS[j]);
                        Debug.WriteLine("XML编码：" + AVAIL_ENCODINGS[k]);
                        return new[] {AVAIL_ENCODINGS[j], AVAIL_ENCODINGS[k]};
                    }
                    catch (Exception ex)
                    {
                        // ignore
                        Debug.WriteLine("There are some error:" + ex.Message);
                    }
                }
            }
            Debug.WriteLine("自动识别编码失败！选择UTF-16LE继续。");
            return new[] {AVAIL_ENCODINGS[1], AVAIL_ENCODINGS[1]};
        }
    }
}