using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
