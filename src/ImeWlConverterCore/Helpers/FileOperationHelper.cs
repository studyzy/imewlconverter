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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using UtfUnknown;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class FileOperationHelper
    {
        public static string GetCurrentFolderPath()
        {
            var type = (new ConstantString()).GetType();

            return Path.GetDirectoryName(type.Assembly.Location);
        }

        /// <summary>
        ///     根据词库的格式或内容判断源词库的类型
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string AutoMatchSourceWLType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            if (ext == ".scel")
            {
                return ConstantString.SOUGOU_XIBAO_SCEL;
            }
            if (ext == ".qcel")
            {
                return ConstantString.QQ_PINYIN_QCEL;
            }
            if (ext == ".bak")
            {
                return ConstantString.TOUCH_PAL;
            }
            if (ext == ".uwl")
            {
                return ConstantString.ZIGUANG_PINYIN_UWL;
            }
            if (ext == ".bin")
            {
                return ConstantString.SOUGOU_PINYIN_BIN;
            }
            if (ext == ".dat")
            {
                return ConstantString.WIN10_MS_PINYIN;
            }
            if (ext == ".bcd")
            {
                return ConstantString.BAIDU_BCD;
            }
            if (ext == ".bdict")
            {
                return ConstantString.BAIDU_BDICT;
            }
            if (ext == ".qpyd")
            {
                return ConstantString.QQ_PINYIN_QPYD;
            }
            if (ext == ".ld2")
            {
                return ConstantString.LINGOES_LD2;
            }
            if (ext == ".zip")
            {
                return ConstantString.GBOARD;
            }
            if (ext == ".mb")
            {
                return ConstantString.JIDIAN_MBDICT;
            }
            if (Directory.Exists(filePath))
            {
                return "";
            }

            string example = "";
            Encoding code = GetEncodingType(filePath);
            using (var sr = new StreamReader(filePath, code))
            {
                for (int i = 0; i < 5; i++)
                {
                    example = sr.ReadLine();
                }
                sr.Close();
            }
            if (example == null)
            {
                example = "";
            }
            var reg = new Regex(@"^('[a-z]+)+\s[\u4E00-\u9FA5]+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.SOUGOU_PINYIN;
            }
            reg = new Regex(@"^([a-z]+')+[a-z]+\,[\u4E00-\u9FA5]+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.FIT;
            }

            reg = new Regex(@"^[a-z']+\s[\u4E00-\u9FA5]+\s\d+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.QQ_PINYIN;
            }
            //reg = new Regex(@"^[\u4E00-\u9FA5]+$");
            //if (reg.IsMatch(example))
            //{
            //    return ConstantString.WORD_ONLY;
            //}//用户“不再梦想”建议删除该功能，因为加加词库也可能是纯汉字，会形成误判。
            reg = new Regex(@"^([\u4E00-\u9FA5]+[a-z]+)+([\u4E00-\u9FA5]+[a-z]*)*$");
            if (reg.IsMatch(example))
            {
                return ConstantString.PINYIN_JIAJIA;
            }
            reg = new Regex(@"^[\u4E00-\u9FA5]+\t[a-z']+\t\d+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.ZIGUANG_PINYIN;
            }
            reg = new Regex(@"^[\u4E00-\u9FA5]+\t\d+[a-z\s]+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.GOOGLE_PINYIN;
            }
            reg = new Regex(@"^[\u4E00-\u9FA5]+\s[a-z\|]+\s\d+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.BAIDU_SHOUJI;
            }
            reg = new Regex(@"^[a-z]{1,4}\s[\u4E00-\u9FA5]+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.JIDIAN;
            }
            reg = new Regex(@"^[a-z']+\s[\u4E00-\u9FA5]+$");
            if (reg.IsMatch(example))
            {
                return ConstantString.SINA_PINYIN;
            }

            return "";
        }

        /// <summary>
        ///     自动判断文字编码，然后进行读取
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }
            Encoding c = GetEncodingType(path);
            return ReadFile(path, c);
            //string ext = Path.GetExtension(path);
            //if (ext == ".scel") //搜狗细胞词库
            //{
            //    return SougouPinyinScel.ReadScel(path);
            //}
            //else //文本
            //{
            //    //using (var sr = new StreamReader(path, GetType(path),true))
            //    //{
            //    //    return sr.ReadToEnd();
            //    //}
            //    Encoding c = Encoding.GetEncoding("GBK");
            //    return ReadFileContent(path, ref c, Encoding.GetEncoding("GBK"));
            //}
        }

        public static string ReadFile(string path, Encoding encoding)
        {
            if (!File.Exists(path))
            {
                return "";
            }
            using (var sr = new StreamReader(path, encoding))
            {
                string txt = sr.ReadToEnd();
                sr.Close();
                return txt;
            }
        }

        /// <summary>
        ///     将一个字符串写入文件，采用覆盖的方式
        /// </summary>
        /// <param name="path"></param>
        /// <param name="coding"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool WriteFile(string path, Encoding coding, string content)
        {
            try
            {
                using (var sw = new StreamWriter(path, false, coding))
                {
                    sw.Write(content);
                    sw.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static StreamWriter WriteFile(string path, Encoding coding)
        {
            var sw = new StreamWriter(path, false, coding);

            return sw;
        }

        /// <summary>
        ///     写一行文本到文件，追加的方式
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool WriteFileLine(string path, string line)
        {
            try
            {
                using (var sw = new StreamWriter(path, true))
                {
                    sw.WriteLine(line);
                    sw.Close();
                }
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
            var sw = new StreamWriter(path, false, coding);
            return sw;
        }

        public static Encoding GetEncodingType(string fileName)
        {
            DetectionResult result = CharsetDetector.DetectFromFile(fileName);
            DetectionDetail resultDetected = result.Detected;
            if (resultDetected.Confidence < 0.7)
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
            Encoding encoding = resultDetected.Encoding;
            return encoding;
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
        /// <param name="input"></param>
        /// <returns></returns>
        public static IList<string> GetFilesPath(string input)
        {
            var result = new List<string>();

            foreach (var path in input.Split('|'))
            {
                result.AddRange(GetFilesPathFor1(path.Trim()));
            }
            return result;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sFullName"></param>
        /// <returns></returns>
        public static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            if (File.Exists(sFullName))
                lSize = new FileInfo(sFullName).Length;
            return lSize;
        }

        /// <summary>
        /// 给定一个文件夹或者正则表达式，返回文件路径列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static IList<string> GetFilesPathFor1(string input)
        {
            if (input.Contains("*")) //正则匹配模式
            {
                var dic = Path.GetDirectoryName(input);
                var filen = Path.GetFileName(input);
                return Directory.GetFiles(dic, filen, SearchOption.AllDirectories);
            }
            if (Directory.Exists(input)) //这是一个文件夹
            {
                return Directory.GetFiles(input, "*.*", SearchOption.AllDirectories);
            }
            if (File.Exists(input)) //文件
            {
                return new List<string>() { input };
            }
            return new List<string>();
        }

        //public static bool IsUnicode(Encoding encoding)
        //{
        //    int codepage = encoding.CodePage;
        //    // return true if codepage is any UTF codepage
        //    return codepage == 65001 || codepage == 65000 || codepage == 1200 || codepage == 1201;
        //}

        //public static string ReadFileContent(string fileName, ref Encoding encoding, Encoding defaultEncoding)
        //{
        //    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        //    {
        //        using (StreamReader reader = OpenStream(fs, encoding, defaultEncoding))
        //        {
        //            encoding = reader.CurrentEncoding;
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}

        //public static StreamReader OpenStream(FileStream fs, Encoding suggestedEncoding, Encoding defaultEncoding)
        //{
        //    if (fs.Length > 3)
        //    {
        //        // the autodetection of StreamReader is not capable of detecting the difference
        //        // between ISO-8859-1 and UTF-8 without BOM.
        //        int firstByte = fs.ReadByte();
        //        int secondByte = fs.ReadByte();
        //        switch ((firstByte << 8) | secondByte)
        //        {
        //            case 0x0000: // either UTF-32 Big Endian or a binary file; use StreamReader
        //            case 0xfffe: // Unicode BOM (UTF-16 LE or UTF-32 LE)
        //            case 0xfeff: // UTF-16 BE BOM
        //            case 0xefbb: // start of UTF-8 BOM
        //                // StreamReader autodetection works
        //                fs.Position = 0;
        //                return new StreamReader(fs);
        //            default:
        //                return AutoDetect(fs, (byte) firstByte, (byte) secondByte, defaultEncoding);
        //        }
        //    }
        //    else
        //    {
        //        if (suggestedEncoding != null)
        //        {
        //            return new StreamReader(fs, suggestedEncoding);
        //        }
        //        else
        //        {
        //            return new StreamReader(fs);
        //        }
        //    }
        //}

        //private static StreamReader AutoDetect(FileStream fs, byte firstByte, byte secondByte, Encoding defaultEncoding)
        //{
        //    var max = (int) Math.Min(fs.Length, 500000); // look at max. 500 KB
        //    const int ASCII = 0;
        //    const int Error = 1;
        //    const int UTF8 = 2;
        //    const int UTF8Sequence = 3;
        //    int state = ASCII;
        //    int sequenceLength = 0;
        //    byte b;
        //    for (int i = 0; i < max; i++)
        //    {
        //        if (i == 0)
        //        {
        //            b = firstByte;
        //        }
        //        else if (i == 1)
        //        {
        //            b = secondByte;
        //        }
        //        else
        //        {
        //            b = (byte) fs.ReadByte();
        //        }
        //        if (b < 0x80)
        //        {
        //            // normal ASCII character
        //            if (state == UTF8Sequence)
        //            {
        //                state = Error;
        //                break;
        //            }
        //        }
        //        else if (b < 0xc0)
        //        {
        //            // 10xxxxxx : continues UTF8 byte sequence
        //            if (state == UTF8Sequence)
        //            {
        //                --sequenceLength;
        //                if (sequenceLength < 0)
        //                {
        //                    state = Error;
        //                    break;
        //                }
        //                else if (sequenceLength == 0)
        //                {
        //                    state = UTF8;
        //                }
        //            }
        //            else
        //            {
        //                state = Error;
        //                break;
        //            }
        //        }
        //        else if (b >= 0xc2 && b < 0xf5)
        //        {
        //            // beginning of byte sequence
        //            if (state == UTF8 || state == ASCII)
        //            {
        //                state = UTF8Sequence;
        //                if (b < 0xe0)
        //                {
        //                    sequenceLength = 1; // one more byte following
        //                }
        //                else if (b < 0xf0)
        //                {
        //                    sequenceLength = 2; // two more bytes following
        //                }
        //                else
        //                {
        //                    sequenceLength = 3; // three more bytes following
        //                }
        //            }
        //            else
        //            {
        //                state = Error;
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            // 0xc0, 0xc1, 0xf5 to 0xff are invalid in UTF-8 (see RFC 3629)
        //            state = Error;
        //            break;
        //        }
        //    }
        //    fs.Position = 0;
        //    switch (state)
        //    {
        //        case ASCII:
        //        case Error:
        //            // when the file seems to be ASCII or non-UTF8,
        //            // we read it using the user-specified encoding so it is saved again
        //            // using that encoding.
        //            if (IsUnicode(defaultEncoding))
        //            {
        //                // the file is not Unicode, so don't read it using Unicode even if the
        //                // user has choosen Unicode as the default encoding.

        //                // If we don't do this, SD will end up always adding a Byte Order Mark
        //                // to ASCII files.
        //                defaultEncoding = Encoding.GetEncoding("GBK"); // use system encoding instead
        //            }
        //            return new StreamReader(fs, defaultEncoding);
        //        default:
        //            return new StreamReader(fs);
        //    }
        //}
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileToZip">要压缩的文件全名</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <returns>压缩结果</returns>
        public static bool ZipFile(string fileToZip, string zipedFile)
        {
            bool result = true;
            ZipOutputStream zipStream = null;
            FileStream fs = null;
            ZipEntry ent = null;

            if (!File.Exists(fileToZip))
                return false;

            try
            {
                fs = File.OpenRead(fileToZip);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                fs = File.Create(zipedFile);
                zipStream = new ZipOutputStream(fs);
                //if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
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
                if (ent != null)
                {
                    ent = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            //GC.Collect();
            //GC.Collect(1);

            return result;
        }

        /// <summary>
        /// 解压功能(解压压缩文件到指定目录)
        /// </summary>
        /// <param name="fileToUnZip">待解压的文件</param>
        /// <param name="zipedFolder">指定解压目标目录</param>
        /// <returns>解压结果</returns>
        public static bool UnZip(string fileToUnZip, string zipedFolder)
        {
            bool result = true;
            FileStream fs = null;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            string fileName;

            if (!File.Exists(fileToUnZip))
                return false;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            try
            {
                zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));
                //if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = Path.Combine(zipedFolder, ent.Name);
                        fileName = fileName.Replace('/', '\\'); //change by Mr.HopeGi

                        if (fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }

                        //fs = File.Create(fileName);
                        //int size = 2048;
                        //byte[] data = new byte[size];
                        //while (true)
                        //{
                        //    size = zipStream.Read(data, 0, data.Length);
                        //    if (size > 0)
                        //        fs.Write(data, 0, data.Length);
                        //    else
                        //        break;
                        //}
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            byte[] buffer = new byte[10240];
                            int size = zipStream.Read(buffer, 0, buffer.Length);
                            while (size > 0)
                            {
                                streamWriter.Write(buffer, 0, size);
                                size = zipStream.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
                //GC.Collect();
                //GC.Collect(1);
            }
            return result;
        }
    }
}
