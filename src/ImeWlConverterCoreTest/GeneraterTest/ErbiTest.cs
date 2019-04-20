using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    class ErbiTest
    {
        private IWordCodeGenerater generater;
        [SetUp]
        public void SetUp()
        {
            generater = new QingsongErbiGenerater();
        }
        [TestCase("中国人民", "zgrm")]
        [TestCase("中华人民共和国", "zhrg")]
        public void TestOneWord(string c, string code)
        {
            var codes=generater.GetCodeOfString(c);
            foreach (var code1 in codes)
            {
                Debug.WriteLine(code1[0]);
                if (code == code1[0])
                {
                    Assert.Pass("Pass");
                    return;
                }
            }
            Assert.Fail("not matched code,"+c);
            
        }
        [Ignore("数据量太大，手动启动")]
        [Test]
        public void BatchTest()
        {
            //var txt = FileOperationHelper.ReadFile("erbi.txt");
            //foreach (var line in txt.Split('\n'))
            //{
            //    var arr = line.Split(' ');
            //    var code = arr[0];
            //    for (var i = 1; i < arr.Length; i++)
            //    {
            //        var word = arr[i];
            //        var codes = generater.GetCodeOfString(word);
            //        try
            //        {
            //            if (!IsContain(codes, code))
            //            {
            //                Debug.WriteLine("Not Match:" + word + "\t" + code + " mycode:" +
            //                                CollectionHelper.ListToString(codes, " "));
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine("Error:"+word+";"+ex.Message);
            //        }
            //    }
            //}
        }
        private bool IsContain(IList<string> str,string code )
        {
            foreach (var s in str)
            {
                if (s == code)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
