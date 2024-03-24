/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    class ErbiTest
    {
        private IWordCodeGenerater generater;

        [OneTimeSetUp]
        public void SetUp()
        {
            generater = new QingsongErbiGenerater();
        }

        [TestCase("中国人民", "zgrm")]
        [TestCase("中华人民共和国", "zhrg")]
        public void TestOneWord(string c, string code)
        {
            var codes = generater.GetCodeOfString(c);
            foreach (var code1 in codes)
            {
                Debug.WriteLine(code1[0]);
                if (code == code1[0])
                {
                    Assert.Pass("Pass");
                    return;
                }
            }
            Assert.Fail("not matched code," + c);
        }

        [Test, Description("数据量太大，手动启动"), Explicit()]
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

        private bool IsContain(IList<string> str, string code)
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
