/*!
 * This work contains codes translated from the original work
 * by stevenlele (https://github.com/studyzy/imewlconverter/issues/204#issuecomment-2011007855)
 * translate to csharp by nopdan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     百度拼音备份词库
    /// </summary>
    [ComboBoxShow(ConstantString.BAIDU_PINYIN_BACKUP, ConstantString.BAIDU_PINYIN_BACKUP_C, 20)]
    public class BaiduPinyinBackup : BaseImport, IWordLibraryImport
    {
        #region IWordLibraryImport 成员

        public WordLibraryList Import(string path)
        {
            var wordLibraryList = new WordLibraryList();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            // FF FE
            fs.Seek(2, SeekOrigin.Begin);
            // 不清楚 <cnword> 是否在 <enword> 前面，所以标记一下
            var cnFlag = false;
            while (fs.Position < fs.Length - 4)
            {
                // 每次读取两字节
                var lineBytes = new List<byte>();
                var bytes = new byte[2];
                do
                {
                    fs.Read(bytes, 0, 2);
                    // 遇到换行符结束读取
                    if (bytes[0] == 0x0A && bytes[1] == 0x00)
                        break;
                    lineBytes.AddRange(bytes);
                } while (true);
                var decoded = Decode(lineBytes);
                var line = Encoding.Unicode.GetString(decoded);
                // 忽略英文单词
                if (cnFlag && (line == "<enword>" || line == "<sysusrword>"))
                    break;
                if (line == "<cnword>")
                {
                    cnFlag = true;
                    continue;
                }
                if (!cnFlag)
                    continue;
                // 每一行的格式
                // 百度输入法(bai|du|shu|ru|fa) 2 24 1703756731 N N
                var array = line.Split(" ");
                if (array.Length < 2)
                    continue;
                var rank = Convert.ToInt32(array[1]);
                // 用正则分离词组和拼音
                var pattern = @"([^\(]+)\((.+)\)";
                var match = Regex.Match(array[0], pattern);
                if (match.Groups.Count != 3)
                    continue;
                var word = match.Groups[1].Value;
                var py = match.Groups[2].Value;
                var pinyin = py.Split("|");

                wordLibraryList.Add(
                    new WordLibrary
                    {
                        Rank = rank,
                        Word = word,
                        PinYin = pinyin
                    }
                );
            }
            return wordLibraryList;
        }

        #endregion

        #region 解码算法

        private const uint MASK = 0x2D382324;
        private static readonly byte[] TABLE = Encoding.ASCII.GetBytes(
            "qogjOuCRNkfil5p4SQ3LAmxGKZTdesvB6z_YPahMI9t80rJyHW1DEwFbc7nUVX2-"
        );
        private static byte[] DECODE_TABLE;

        public BaiduPinyinBackup()
        {
            DECODE_TABLE = new byte[256];
            for (var i = 0; i < TABLE.Length; i++)
            {
                DECODE_TABLE[TABLE[i]] = (byte)i;
            }
        }

        public static byte[] Decode(List<byte> data)
        {
            if (data.Count % 4 != 2)
                throw new ArgumentException("Invalid data length");

            byte base64Remainder = (byte)(data[data.Count - 2] - 65);
            if (base64Remainder > 2 || data[data.Count - 1] != 0)
                throw new ArgumentException("Invalid padding");

            // 映射魔改过的 base64 编码表
            for (int i = 0; i < data.Count - 2; i++)
            {
                data[i] = DECODE_TABLE[data[i]];
            }
            // 将每 4 个字节转换为 3 个字节
            var transformed = new List<byte>(capacity: data.Count / 4 * 3);
            for (int i = 0; i < data.Count - 2; i += 4)
            {
                byte highBits = data[i + 3];
                transformed.Add((byte)(data[i] | (highBits & 0b110000) << 2));
                transformed.Add((byte)(data[i + 1] | (highBits & 0b1100) << 4));
                transformed.Add((byte)(data[i + 2] | (highBits & 0b11) << 6));
            }

            if (base64Remainder > 0)
            {
                for (int i = 0; i < 3 - base64Remainder; i++)
                {
                    if (transformed[transformed.Count - 1] != 0)
                        throw new ArgumentException("Invalid padding");
                    transformed.RemoveAt(transformed.Count - 1);
                }
            }

            var result = transformed.ToArray();
            for (int i = 0; i < result.Length; i += 4)
            {
                uint chunk;
                if (i + 4 > result.Length)
                {
                    var chunkBytes = new byte[4];
                    Array.Copy(result, i, chunkBytes, 0, result.Length - i);
                    chunk = MASK ^ BitConverter.ToUInt32(chunkBytes, 0);
                    Buffer.BlockCopy(BitConverter.GetBytes(chunk), 0, result, i, result.Length - i);
                    break;
                }
                chunk = MASK ^ BitConverter.ToUInt32(result, i);
                chunk = (chunk & 0x1FFFFFFF) << 3 | chunk >> 29;
                Buffer.BlockCopy(BitConverter.GetBytes(chunk), 0, result, i, 4);
            }
            return result;
        }

        #endregion

        #region IWordLibraryImport Members

        public override bool IsText
        {
            get { return false; }
        }

        #endregion

        public WordLibraryList ImportLine(string line)
        {
            throw new Exception("百度输入法备份格式是二进制文件，不支持流转换");
        }
    }
}
