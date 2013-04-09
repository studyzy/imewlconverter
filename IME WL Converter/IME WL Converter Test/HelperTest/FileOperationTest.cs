using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest
{
    class FileOperationTest
    {
        [TestCase("Test\\u8nobomzy.txt","UTF-8")]
        [TestCase("Test\\luna_pinyin_export.txt", "UTF-8")]
        [TestCase("Test\\gbzy.txt", "GB18030")]
        [TestCase("Test\\QQPinyin.txt", "Unicode")]
        public void TestGetFileEncoding(string path,string encoding)
        {
            var e = FileOperationHelper.GetEncodingType(path);
            Assert.AreEqual(e.ToString(),Encoding.GetEncoding(encoding).ToString());
            var txt = FileOperationHelper.ReadFile(path);
            Debug.WriteLine(txt);
        }
        [Test]
        public void TestWriteFile()
        {
            string path = "WriteTest.txt";
            string content = "Hello Word!";
            Assert.IsTrue(FileOperationHelper.WriteFile(path, Encoding.UTF8, content));
            Assert.IsTrue(File.Exists(path));
            File.Delete(path);
        }
    }
}
