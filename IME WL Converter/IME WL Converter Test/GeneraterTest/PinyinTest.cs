using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Studyzy.IMEWLConverter.Generaters;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    [TestFixture]
    class PinyinTest
    {
        private IWordCodeGenerater generater;
        [TestFixtureSetUp]
        public void Setup()
        {
            generater = new PinyinGenerater();
        }
        [Test]
        public void TestGetOneWordPinyin()
        {
            
        }
        [TestCase("曾毅","zeng yi")]
        [TestCase("音乐", "yin yue")]
        [TestCase("快乐", "kuai le")]
        [TestCase("银行", "yin hang")]
        [TestCase("行走", "xing zou")]
        public void TestGetLongWordsPinyin(string str,string py)
        {
            var result = generater.GetCodeOfString(str);
            Assert.Contains(py,result.ToCodeString(" ").ToArray());
        }
    }
}
