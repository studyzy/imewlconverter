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
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest
{
    class FileOperationTest
    {
        [TestCase("Test/u8nobomzy.txt", "UTF-8")]
        [TestCase("Test/luna_pinyin_export.txt", "UTF-8")]
        [TestCase("Test/gbzy.txt", "GB18030")]
        [TestCase("Test/QQPinyin.txt", "Unicode")]
        public void TestGetFileEncoding(string path, string encoding)
        {
            path = GetFullPath(path);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var e = FileOperationHelper.GetEncodingType(path);
            Assert.AreEqual(e.EncodingName, Encoding.GetEncoding(encoding).EncodingName);
            var txt = FileOperationHelper.ReadFile(path);
            Debug.WriteLine(txt);
        }

        [Test]
        public void TestCodePagesEncodingProviderRequired()
        {
            Assert.Catch(
                Type.GetType("System.ArgumentException"),
                () => Encoding.GetEncoding("GB2312").ToString()
            );
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Assert.AreEqual(
                "Chinese Simplified (GB2312)",
                Encoding.GetEncoding("GB2312").EncodingName
            );
        }

        [Test]
        public void TestWriteFile()
        {
            string path = GetFullPath("WriteTest.txt");
            string content = "Hello Word!";
            Assert.IsTrue(FileOperationHelper.WriteFile(path, Encoding.UTF8, content));
            Assert.IsTrue(File.Exists(path));
            File.Delete(path);
        }

        protected static string GetFullPath(string fileName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, fileName);
        }
    }
}
