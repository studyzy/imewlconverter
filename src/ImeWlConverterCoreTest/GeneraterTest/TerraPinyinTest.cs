using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    class TerraPinyinTest
    {
        private IWordCodeGenerater generater=new TerraPinyinGenerater();
        [Test]
        public void TestPinyin2TerraPinyin()
        {
            WordLibrary wl=new WordLibrary(){Word = "深蓝",Rank = 123,PinYin = new []{"shen","lan"},CodeType = CodeType.Pinyin};
             generater.GetCodeOfWordLibrary(wl);
            foreach (var py in wl.Codes)
            {
                Debug.WriteLine(py);
            }
          
        }
        [TestCase("曾经","ceng2 jin1")]
        [TestCase("曾毅","zeng1 yi4")]
        [TestCase("音乐", "yin1 yue4")]
        [TestCase("快乐", "kuai4 le4")]
        public void TestChar2TerraPinyin(string word,string pinyin)
        {
            WordLibrary wl = new WordLibrary() { Word =word, Rank = 123, CodeType = CodeType.NoCode };
            generater.GetCodeOfWordLibrary(wl);
            foreach (var py in wl.Codes.ToCodeString(" "))
            {
                Debug.WriteLine(py);
            }

        }
    }
}
