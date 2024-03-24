/*
 *   Copyright © 2022 yfdyh000

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

using System;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class QQPinyinQcelTest : BaseTest
    {
        [OneTimeSetUp]
        public override void InitData()
        {
            importer = new QQPinyinQcel();
        }

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [TestCase]
        public void TestImportLine()
        {
            Assert.Catch(
                () =>
                {
                    importer.ImportLine("test");
                },
                "Qcel格式是二进制文件，不支持流转换"
            );
        }

        [TestCase("星际战甲.qcel")]
        public void TestImportQcelWithAlphabet(string filePath)
        {
            var lib = importer.Import(GetFullPath(filePath));
            Assert.Greater(lib.Count, 0);

            Assert.AreEqual(lib.Count, 4675);
            Assert.AreEqual(lib[0].CodeType, Studyzy.IMEWLConverter.Entities.CodeType.Pinyin);
            Assert.AreEqual(lib[2].PinYinString, "a'ka'ta");
            Assert.AreEqual(lib[3].PinYinString, "a'ka'ta'r'i'v'wai'guan");
            Assert.AreEqual(lib[0].Rank, 0);
            Assert.AreEqual(lib[4670].SingleCode, "zuo");
            Assert.AreEqual(lib[2].Word, "阿卡塔");
            Assert.AreEqual(lib[3].Word, "阿卡塔riv外观");
        }

        [TestCase("星际战甲.qcel")]
        public void TestListQcelInfo(string filePath)
        {
            var info = QQPinyinQcel.ReadQcelInfo(GetFullPath(filePath));
            Assert.That(info, Is.Not.Null.And.Not.Empty);
            foreach (var item in info)
                TestContext.WriteLine(item.Key + ": " + item.Value);

            Assert.AreEqual(info["CountWord"], "4675");
            Assert.AreEqual(info["Name"], "星际战甲warframe国际服");
            Assert.AreEqual(info["Type"], "射击游戏");
            Assert.True(info["Info"].Contains("词条来源是灰机wiki-warframe中文维基的中英文对照表"));
            Assert.True(info["Sample"].Contains("肿瘤 三叶坚韧 狂风猛踢 寒冰之力"));
        }
    }
}
