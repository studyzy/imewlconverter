using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest
{
    class HttpHelperTest
    {
        [TestCase("http://www.baidu.com", "百度一下")]
        public void TestWriteFile(string url,string keyword)
        {
            var html = HttpHelper.GetHtml(url);
            Assert.IsTrue(html.Contains(keyword));
        }
    }
}
