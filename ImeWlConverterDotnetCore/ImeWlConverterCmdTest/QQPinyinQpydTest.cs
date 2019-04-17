using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class QQPinyinQpydTest : BaseTest
    {
        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [SetUp]
        public override void InitData()
        {
            importer = new QQPinyinQpyd();
        }

        [TestCase("成语.qpyd")]
        public void TestParseQypd(string file)
        {
            WordLibraryList wll = importer.Import(GetFullPath(file));
            Assert.Greater(wll.Count, 0);
        }
    }
}