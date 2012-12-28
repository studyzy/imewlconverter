using System;
using System.Diagnostics;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class Ld2ParseTest : BaseTest
    {
        [TestCase("g.ld2")]
        [TestCase("i.ld2")]
        public void TestParseLd2(string file)
        {
            string ld2File = GetFullPath(file);

            WordLibraryList reult = importer.Import(ld2File);

            Assert.IsNotNull(reult);
            Assert.Greater(reult.Count, 0);
            //foreach (WordLibrary wordLibrary in reult)
            //{
            //    Debug.WriteLine(wordLibrary);
            //}
        }

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [TestFixtureSetUp]
        public override void InitData()
        {
            importer = new LingoesLd2();
        }
    }
}