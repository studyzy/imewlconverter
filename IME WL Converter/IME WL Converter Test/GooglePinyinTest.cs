using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class GooglePinyinTest : BaseTest
    {
        #region Setup/Teardown

        [SetUp]
        public override void InitData()
        {
            exporter = new GooglePinyin();
            importer = new GooglePinyin();
        }

        #endregion

        protected override string StringData
        {
            get { return Resource4Test.GooglePinyin; }
        }

        [Test]
        public void TestExport()
        {
            var txt = exporter.Export(WlListData)[0];
            Assert.IsTrue(txt.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries).Length == 2);
        }

        [Test]
        public void TestExportLine()
        {
            string txt = exporter.ExportLine(WlData);
            Assert.AreEqual(txt, "深蓝测试\t10\tshen lan ce shi");
        }

        [Test]
        public void TestImport()
        {
            WordLibraryList list = ((IWordLibraryTextImport) importer).ImportText(StringData);
            Assert.IsNotNull(list);
            Assert.AreEqual(list.Count, 10);
        }
    }
}