using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    public class SelfDefiningTest
    {
        private SelfDefining export;
        [TestFixtureSetUp]
        public void Init()
        {
            export=new SelfDefining();
        }

        private WordLibrary WlData = new WordLibrary { Count = 10, PinYin = new[] { "shen", "lan", "ce", "shi" }, Word = "深蓝测试" };
        [Test]
        public void TestExportPinyinWL()
        {
            export.UserDefiningPattern = InitPattern();
            var str = export.Export(new WordLibraryList() {WlData});
            Debug.WriteLine(str);
            Assert.IsNotNullOrEmpty(str);
        }
        [Test]
        public void TestExportPinyinDifferentFormatWL()
        {
            var p = InitPattern();
            p.Sort=new List<int>(){3,2,1};
            p.SplitString = "$";
            p.CodeSplitString = "_";
            p.CodeSplitType= BuildType.None;
            p.IsPinyinFormat = true;
            export.UserDefiningPattern = p;
            var str = export.Export(new WordLibraryList() { WlData });
            Debug.WriteLine(str);
            Assert.AreEqual(str, "深蓝测试$shen_lan_ce_shi\r\n");
        }
        [Test]
        public void TestExportExtCodeWL()
        {
            export.UserDefiningPattern = InitPattern();
            export.UserDefiningPattern.MappingTablePath = "Test\\array30.txt";
            var str = export.Export(new WordLibraryList() { WlData });
            Debug.WriteLine(str);
            Assert.IsNotNullOrEmpty(str);
        }
        //[Test]
        //public void TestWLWithoutPinyinExportException()
        //{
        //    export.UserDefiningPattern = InitPattern();
        //    var str = export.Export(new WordLibraryList() { new WordLibrary { Count = 10, Word = "深蓝测试" } });
        //    Debug.WriteLine(str);
        //    Assert.IsNullOrEmpty(str);
        //}
        [Test]
        public void TestExportExtCodeLots()
        {
            string str="深蓝词库转换测试代码";
            var list = new WordLibraryList();
            var ts = "";
            foreach (var c in str)
            {
                ts += c;
                list.Add(new WordLibrary() {Count = 10, IsEnglish = false, Word = ts});
            }


            export.UserDefiningPattern = InitPattern();
            export.UserDefiningPattern.MappingTablePath = "Test\\array30.txt";
            var x = export.Export(list);
            Debug.WriteLine(x);
            Assert.IsNotNullOrEmpty(str);
        }
        private ParsePattern InitPattern()
        {
            ParsePattern pp=new ParsePattern();
            pp.MutiWordCodeFormat = @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
            pp.IsPinyinFormat = false;
            pp.ContainRank = false;
            pp.SplitString = " ";
            pp.ContainCode = true;
            return pp;
        }
     
    }
}
