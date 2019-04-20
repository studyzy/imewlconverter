using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    public class BaiduShoujiTest : BaseTest
    {
        #region SetUp/Teardown

        [SetUp]
        public override void InitData()
        {
            exporter = new BaiduShouji();
            importer = new BaiduShouji();
        }

        #endregion

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [Test]
        public void TestExport()
        {
        }
    }
}