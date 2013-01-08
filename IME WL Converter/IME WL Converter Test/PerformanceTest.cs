using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    class PerformanceTest
    {
        [Ignore("需要的时候再启用")]
        [Test]
        public void TestLoadHugeNumberWL()
        {
            Debug.WriteLine("Start:"+ DateTime.Now.ToString());
            IWordLibraryImport importer=new SougouPinyinScel();
            var wls = importer.Import("Test\\诗词名句大全.scel");
            Debug.WriteLine("Load Words count:" + wls.Count);
            Debug.WriteLine("End:"+DateTime.Now.ToString());
        }
    }
}
