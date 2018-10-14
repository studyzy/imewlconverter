using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class SougouPinyinBinTest : BaseTest
    {
        [TestFixtureSetUp]
        public override void InitData()
        {
            importer = new SougouPinyinBinFromPython();
        }

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [TestCase("sougoubak.bin")]
        public void TestParseBinFile(string filePath)
        {
            WordLibraryList lib = importer.Import(GetFullPath(filePath));
            Assert.Greater(lib.Count, 0);
        }
    }
}