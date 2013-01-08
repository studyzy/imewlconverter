using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class QQPinyinTest : BaseTest
    {
        [TestFixtureSetUp]
        public override void InitData()
        {
            exporter = new QQPinyin();
            importer = new QQPinyin();
        }

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [Test]
        public void TestImport()
        {
            WordLibraryList wll = importer.Import(GetFullPath("QQPinyin.txt"));
            Assert.Greater(wll.Count, 0);
        }
    }
}