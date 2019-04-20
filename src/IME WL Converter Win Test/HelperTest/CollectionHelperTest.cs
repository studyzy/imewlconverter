using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest
{
    class CollectionHelperTest
    {
        [Test]
        public void TestCartesianProduct1()
        {
            IList<IList<string>> list=new List<IList<string>>();
            list.Add(new List<string>(){"a"});
            list.Add(new List<string>() { "b","c","d" });
            list.Add(new List<string>() { "e","f" });
            var result = CollectionHelper.CartesianProduct(list, ",");
            var array = result.ToArray();
            Assert.Contains("a,b,e", array);
            Assert.Contains("a,b,f", array);
            Assert.Contains("a,c,e", array);
            Assert.Contains("a,c,f", array);
            Assert.Contains("a,d,e", array);
            Assert.Contains("a,d,f", array);
            Assert.AreEqual(result.Count,6);
        }
        [Test]
        public void TestCartesianProduct2()
        {
            IList<IList<string>> list = new List<IList<string>>();
            list.Add(new List<string>() { "a" });
            list.Add(new List<string>() { "b"});
            list.Add(new List<string>() { "e" });
            var result = CollectionHelper.CartesianProduct(list, ",");
            var array = result.ToArray();
            Assert.Contains("a,b,e", array);
          
            Assert.AreEqual(result.Count, 1);
        }
    }
}
