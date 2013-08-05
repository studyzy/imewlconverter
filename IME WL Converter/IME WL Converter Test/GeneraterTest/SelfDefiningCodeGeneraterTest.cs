using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    class SelfDefiningCodeGeneraterTest
    {
        [Test]
        public void TestGenerateCode()
        {
            SelfDefiningCodeGenerater generater=new SelfDefiningCodeGenerater();
            generater.MappingDictionary=new Dictionary<char, string>();
            generater.MappingDictionary.Add('深', "shen");
            generater.MappingDictionary.Add('蓝', "lan");
            generater.MutiWordCodeFormat = @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            var result = generater.GetCodeOfString("深蓝");
            Assert.AreEqual(result[0],"shla"); 
        }
        [Test]
        public void TestGeneratePinyinFormatCode()
        {
            SelfDefiningCodeGenerater generater = new SelfDefiningCodeGenerater();
            generater.MappingDictionary = new Dictionary<char, string>();
            generater.MappingDictionary.Add('深', "ipws");
            generater.MappingDictionary.Add('蓝', "ajtl");
            generater.MutiWordCodeFormat = @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            generater.Is1Char1Code = true;
            var result = generater.GetCodeOfString("深蓝");
            Assert.AreEqual(result[0], "ipws");
            Assert.AreEqual(result[1], "ajtl");
            //var codes = generater.GetCodeOfString("蓝深", ",");
            //Assert.AreEqual(codes[0], "ajtl,ipws");

        }
    }
}
