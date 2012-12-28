using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class BaiduBdictTest : BaseTest
    {
        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [TestFixtureSetUp]
        public override void InitData()
        {
            importer = new BaiduPinyinBdict();
        }

        [TestCase("movie.bdict")]
        [TestCase("travel.bdict")]
        public void TestImport(string file)
        {
            WordLibraryList wlList = importer.Import(GetFullPath(file));
            Assert.IsNotNull(wlList);
            Assert.Greater(wlList.Count, 0);
        }
    }
}