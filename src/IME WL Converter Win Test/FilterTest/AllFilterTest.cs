using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;

namespace Studyzy.IMEWLConverter.Test.FilterTest
{
    class AllFilterTest
    {
        [TestCase("123.456",true)]
        [TestCase("abc!efg?", true)]
        [TestCase("1《深蓝词库转换》", false)]
        [TestCase("2大家，好", false)]
        [TestCase("3转换成功。", false)]
        public void ChinesePunctuationFilterTest(string word, bool isKeep)
        {
            var wl = new WordLibrary();
            wl.Word = word;
            ChinesePunctuationFilter filter = new ChinesePunctuationFilter();
            Assert.AreEqual(filter.IsKeep(wl), isKeep);
        }

        [TestCase("深蓝",true)]
        [TestCase("深 蓝", false)]
        [TestCase(" 深蓝", false)]
        [TestCase("深蓝 ", false)]
        public void SpaceFilterTest(string word,bool isKeep)
        {
            var wl = new WordLibrary();
            wl.Word = word;
            SpaceFilter filter=new SpaceFilter();
            Assert.AreEqual(filter.IsKeep(wl), isKeep);
        }
    }
}
