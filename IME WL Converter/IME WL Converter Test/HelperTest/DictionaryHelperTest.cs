using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest
{
    class DictionaryHelperTest
    {
        [TestCase('曾', "uljf")]
        public void TestGetCharCode(char c,string code)
        {
            var codes = DictionaryHelper.GetCode(c);
            Assert.AreEqual(codes.Wubi86,code);
        }
    }
}
