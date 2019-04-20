using System;
using System.Collections.Generic;
using System.Linq;
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
            generater.MappingDictionary=new Dictionary<char, IList<string>>();
            generater.MappingDictionary.Add('深',new []{ "shen"});
            generater.MappingDictionary.Add('蓝',new []{ "lan"});
            generater.Is1Char1Code = false;
            
            generater.MutiWordCodeFormat = @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            var result = generater.GetCodeOfString("深蓝").GetTop1Code();
            Assert.AreEqual(result,"shla");
            result = generater.GetCodeOfString("深深蓝").GetTop1Code();
            Assert.AreEqual(result, "ssla");
            result = generater.GetCodeOfString("深蓝深蓝").GetTop1Code();
            Assert.AreEqual(result, "slsl"); 
        }
        [Test]
        public void TestGeneratePinyinFormatCode()
        {
            SelfDefiningCodeGenerater generater = new SelfDefiningCodeGenerater();
            generater.MappingDictionary = new Dictionary<char,IList<  string>>();
            generater.MappingDictionary.Add('深',new []{ "ipws"});
            generater.MappingDictionary.Add('蓝', new []{"ajtl"});
         
            generater.Is1Char1Code = true;
            var result = generater.GetCodeOfString("深蓝").ToCodeString(",");
            Assert.AreEqual(result[0], "ipws,ajtl");
        
            //var codes = generater.GetCodeOfString("蓝深", ",");
            //Assert.AreEqual(codes[0], "ajtl,ipws");

        }

        [Test]
        public void TestGenerateMutiPinyinFormatCode()
        {
            SelfDefiningCodeGenerater generater = new SelfDefiningCodeGenerater();
            generater.MappingDictionary = new Dictionary<char, IList<string>>();
            generater.MappingDictionary.Add('深', new[] { "ipws", "ebcd" });
            generater.MappingDictionary.Add('蓝', new[] { "ajtl" });

            generater.Is1Char1Code = true;
            var result = generater.GetCodeOfString("深蓝").ToCodeString(",");
            Assert.Contains("ipws,ajtl", result.ToArray());

            //var codes = generater.GetCodeOfString("蓝深", ",");
            //Assert.AreEqual(codes[0], "ajtl,ipws");

        }
    }
}
