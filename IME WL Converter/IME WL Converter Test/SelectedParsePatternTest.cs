using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    internal class SelectedParsePatternTest
    {

        SelfDefining selfDefining = new SelfDefining();
        [Test]
        public void TestPinyinString2WL()
        {
            ParsePattern parser = new ParsePattern()
                                      {
                                          IsPinyinFormat = true,
                                          CodeSplitType = BuildType.FullContain,
                                          CodeSplitString = ",",
                                          ContainCode = true,
                                          ContainRank = true,
                                          SplitString = " ",
                                          Sort = new List<int>() {2, 1, 3}
                                      };
            var str = "深蓝 ,shen,lan, 1";
            selfDefining.UserDefiningPattern = parser;
            var wl = selfDefining.ImportLine(str)[0];
          
            Assert.AreEqual(wl.Codes[0][0], "shen");
            Assert.AreEqual(wl.Codes[1][0], "lan");
            Assert.AreEqual(wl.Rank, 1);
        }

        [Test]
        public void TestWordLibrary2String()
        {
            ParsePattern parser = new ParsePattern()
            {
                IsPinyinFormat = true,
                CodeSplitType = BuildType.FullContain,
                CodeSplitString = ",",
                ContainCode = true,
                ContainRank = true,
                SplitString = "|",
                CodeType = CodeType.Pinyin,
                Sort = new List<int>() {2, 1, 3}
            };
            WordLibrary wl = new WordLibrary() {Word = "深蓝", Rank = 123, CodeType = CodeType.Pinyin};
            wl.Codes = new IList<string>[2];
            wl.Codes[0] = new[] {"shen"};
            wl.Codes[1] = new[] {"lan"};
            selfDefining.UserDefiningPattern = parser;
            var str = selfDefining.ExportLine(wl);
            Assert.AreEqual(str, "深蓝|,shen,lan,|123");
        }
        [Test]
        public void TestGeneratePinyinThen2String()
        {
            ParsePattern parser = new ParsePattern()
            {
                IsPinyinFormat = true,
                CodeSplitType = BuildType.FullContain,
                CodeSplitString = "~",
                ContainCode = true,
                ContainRank = true,
                SplitString = "|",
                CodeType = CodeType.Pinyin,
                LineSplitString = "\r",
                Sort = new List<int>() { 2, 1, 3 }
            };
            WordLibraryList wll=new WordLibraryList();
            WordLibrary wl = new WordLibrary() { Word = "深蓝", Rank = 123, CodeType = CodeType.UserDefine };
            wl.Codes = new IList<string>[2];
            wl.Codes[0] = new[] { "sn" };
            wl.Codes[1] = new[] { "ln" };
            wll.Add(wl);
            selfDefining.UserDefiningPattern = parser;
            var str = selfDefining.Export(wll);
            Assert.AreEqual(str, "深蓝|~shen~lan~|123\r");
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
