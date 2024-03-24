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

using System.Linq;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    [TestFixture]
    class ZhuyinTest
    {
        private IWordCodeGenerater generater;

        [OneTimeSetUp]
        public void SetUp()
        {
            generater = new ZhuyinGenerater();
        }

        [Test]
        public void TestGetOneWordPinyin() { }

        [TestCase("曾毅", "ㄗㄥ,ㄧˋ")]
        [TestCase("北京吃饭", "ㄅㄟˇ,ㄐㄧㄥ,ㄔ,ㄈㄢˋ")]
        [TestCase("煤矿", "ㄇㄟˊ,ㄎㄨㄤˋ")]
        [TestCase("故乡", "ㄍㄨˋ,ㄒㄧㄤ")]
        public void TestGetLongWordsPinyin(string str, string py)
        {
            var result = generater.GetCodeOfString(str).ToCodeString(",");
            Assert.Contains(py, result.ToArray());
        }
    }
}
