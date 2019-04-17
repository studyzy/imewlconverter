using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class RimeTest : BaseTest
    {
        [SetUp]
        public override void InitData()
        {
            exporter = new Rime();
            importer = new Rime();
        }

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [TestCase("luna_pinyin_export.txt")]
        public void TestImport(string path)
        {
            WordLibraryList wl = importer.Import(GetFullPath(path));
            Assert.Greater(wl.Count, 0);
        }
    }
}