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
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME;

[ComboBoxShow(ConstantString.QQ_PINYIN_QPYD, ConstantString.QQ_PINYIN_QPYD_C, 60)]
public class QQPinyinQpyd : BaseImport, IWordLibraryImport
{
    #region IWordLibraryImport Members

    public override bool IsText => false;

    #endregion

    public WordLibraryList Import(string path)
    {
        var wll = new WordLibraryList();
        var txt = ParseQpyd(path);
        foreach (var line in txt.Split('\n'))
            if (line != "")
                wll.AddWordLibraryList(ImportLine(line));

        return wll;
    }

    public WordLibraryList ImportLine(string line)
    {
        var sp = line.Split('\t');
        var word = sp[0];
        var py = sp[1];
        var count = 1;
        var wl = new WordLibrary();
        wl.Word = word;
        wl.Rank = count;
        wl.PinYin = py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries);
        var wll = new WordLibraryList();
        if (!string.IsNullOrEmpty(py)) wll.Add(wl);
        return wll;
    }

    private string ParseQpyd(string qqydFile)
    {
        var fs = new FileStream(qqydFile, FileMode.Open, FileAccess.Read);
        fs.Position = 0x38;
        var startAddressByte = new byte[4];
        fs.Read(startAddressByte, 0, 4);
        var startAddress = BitConverter.ToInt32(startAddressByte, 0);
        fs.Position = 0x44;
        var wordCount = BinFileHelper.ReadInt32(fs);
        CountWord = wordCount;
        CurrentStatus = 0;

        fs.Position = startAddress;
        var zipStream = new InflaterInputStream(fs);

        var bufferSize = 2048; //缓冲区大小
        var readCount = 0; //读入缓冲区的实际字节
        var buffer = new byte[bufferSize];
        var byteList = new List<byte>();
        readCount = zipStream.Read(buffer, 0, bufferSize);
        while (readCount > 0)
        {
            for (var i = 0; i < readCount; i++) byteList.Add(buffer[i]);
            readCount = zipStream.Read(buffer, 0, bufferSize);
        }

        zipStream.Close();
        zipStream.Dispose();
        fs.Close();

        var byteArray = byteList.ToArray();

        var unzippedDictStartAddr = -1;
        var idx = 0;
        var sb = new StringBuilder();
        while (unzippedDictStartAddr == -1 || idx < unzippedDictStartAddr)
        {
            // read word

            var pinyinStartAddr = BitConverter.ToInt32(byteArray, idx + 0x6);
            var pinyinLength = BitConverter.ToInt32(byteArray, idx + 0x0) & 0xff;
            var wordStartAddr = pinyinStartAddr + pinyinLength;
            var wordLength = BitConverter.ToInt32(byteArray, idx + 0x1) & 0xff;
            if (unzippedDictStartAddr == -1)
            {
                unzippedDictStartAddr = pinyinStartAddr;
                Debug.WriteLine("词库地址（解压后）：0x" + unzippedDictStartAddr.ToString("0x") + "\n");
            }

            var pinyin = Encoding.UTF8.GetString(byteArray, pinyinStartAddr, pinyinLength);
            var word = Encoding.Unicode.GetString(byteArray, wordStartAddr, wordLength);
            sb.Append(word + "\t" + pinyin + "\n");
            Debug.WriteLine(word + "\t" + pinyin);
            CurrentStatus++;
            // step up
            idx += 0xa;
        }

        return sb.ToString();
    }
}
