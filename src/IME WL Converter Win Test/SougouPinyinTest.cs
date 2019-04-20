using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class SougouPinyinTest : BaseTest
    {
        [TestFixtureSetUp]
        public override void InitData()
        {
            importer = new SougouPinyin();
            exporter = new SougouPinyin();
        }

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [Test]
        public void TestImport()
        {
        }
    }
}