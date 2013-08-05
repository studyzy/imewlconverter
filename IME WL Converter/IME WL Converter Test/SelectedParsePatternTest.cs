using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Test
{
    internal class SelectedParsePatternTest
    {
        [Test]
        public void TestPinyinString2WL()
        {
            ParsePattern parser = new ParsePattern()
                                      {
                                          IsPinyin = true,
                                          CodeSplitType = BuildType.FullContain,
                                          CodeSplitString = ",",
                                          ContainCode = true,
                                          ContainRank = true,
                                          SplitString = " ",
                                          Sort = new List<int>() {2, 1, 3}
                                      };
            var wl = parser.BuildWordLibrary("深蓝 ,shen,lan, 1");
            Assert.AreEqual(wl.PinYin[0], "shen");
            Assert.AreEqual(wl.PinYin[1], "lan");
            Assert.AreEqual(wl.Count, 1);
        }
//        [Test]
//        public void TestCodeString2WL()
//        {
//            ParsePattern parser = new ParsePattern()
//            {
//                IsPinyin = false,
//                IsPinyinFormat = false,
//                MutiWordCodeFormat = @"code_e2=p11+p12+p21+p22
//code_e3=p11+p21+p31+p32
//code_a4=p11+p21+p31+n11",
//                ContainCode = true,
//                ContainRank = false,
//                SplitString = " ",
//                Sort = new List<int>() { 2, 1 ,3}
//            };
//            parser.MappingTable=new Dictionary<char, string>();
//            parser.MappingTable.Add('深',"shen");
//            parser.MappingTable.Add('蓝',"lan");
//            var wl = parser.BuildWlString("深蓝");
//            Assert.AreEqual(wl, "深蓝 shla");
            
            
//        }
    }
}
