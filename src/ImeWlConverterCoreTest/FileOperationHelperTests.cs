using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    public class FileOperationHelperTests
    {
        [Test]
        public void WriteReadFile_Roundtrip()
        {
            var tempDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "temp_tests");
            Directory.CreateDirectory(tempDir);
            var path = Path.Combine(tempDir, "rw_test.txt");
            var expected = "测试内容 - FileOperationHelper roundtrip.\n第二行。";

            try
            {
                var ok = FileOperationHelper.WriteFile(path, Encoding.UTF8, expected);
                Assert.That(ok, Is.True, "WriteFile should return true");

                var actual = FileOperationHelper.ReadFile(path, Encoding.UTF8);
                Assert.That(actual, Is.EqualTo(expected));
            }
            finally
            {
                try { File.Delete(path); } catch { }
                try { Directory.Delete(tempDir); } catch { }
            }
        }

        [Test]
        public void ZipFile_Unzip_PreservesContent()
        {
            var tempDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "temp_zip_tests");
            Directory.CreateDirectory(tempDir);
            var inputFile = Path.Combine(tempDir, "input.txt");
            var zipFile = Path.Combine(tempDir, "out.zip");
            var unzipDir = Path.Combine(tempDir, "unzipped");
            var content = "Zip test content: 这是一个测试。";

            try
            {
                FileOperationHelper.WriteFile(inputFile, Encoding.UTF8, content);

                var zipped = FileOperationHelper.ZipFile(inputFile, zipFile);
                Assert.That(zipped, Is.True, "ZipFile should succeed");
                Assert.That(File.Exists(zipFile), Is.True, "Zip file should exist");

                var unzipped = FileOperationHelper.UnZip(zipFile, unzipDir);
                Assert.That(unzipped, Is.True, "UnZip should succeed");

                // The extracted file should have the same name as the input
                var extracted = Path.Combine(unzipDir, Path.GetFileName(inputFile));
                Assert.That(File.Exists(extracted), Is.True, "Extracted file should exist");
                var actual = FileOperationHelper.ReadFile(extracted, Encoding.UTF8);
                Assert.That(actual, Is.EqualTo(content));
            }
            finally
            {
                try { File.Delete(inputFile); } catch { }
                try { File.Delete(zipFile); } catch { }
                try { Directory.Delete(unzipDir, true); } catch { }
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
