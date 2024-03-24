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

using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class PinyinJiaJiaTest : BaseTest
    {
        #region SetUp/Teardown

        [OneTimeSetUp]
        public override void InitData()
        {
            importer = new PinyinJiaJia();
            exporter = new PinyinJiaJia();
        }

        #endregion

        protected override string StringData
        {
            get { return Resource4Test.PinyinJiajia; }
        }

        [Test]
        public void ExportLine()
        {
            string txt = exporter.ExportLine(WlData);
            Assert.AreEqual(txt, "深shen蓝lan测ce试shi");
        }

        [Test]
        public void ImportNoPinyin()
        {
            WordLibraryList wl = importer.ImportLine("深蓝测试");
            Assert.AreEqual(wl.Count, 1);
            Assert.AreEqual(wl[0].PinYinString, "shen'lan'ce'shi");
        }

        [Test]
        public void ImportWithPinyinFull()
        {
            WordLibraryList wl = importer.ImportLine("深shen蓝lan居ju");
            Assert.AreEqual(wl.Count, 1);
            Assert.AreEqual(wl[0].PinYinString, "shen'lan'ju");
            Assert.AreEqual(wl[0].Word, "深蓝居");
        }

        [Test]
        public void ImportWithPinyinPart()
        {
            WordLibraryList wl = ((IWordLibraryTextImport)importer).ImportText(StringData);
            Assert.AreEqual(wl.Count, 10);
            Assert.AreEqual(wl[0].PinYinString, "ren'min'hen'xing");
            Assert.AreEqual(wl[0].Word, "人民很行");
            Assert.AreEqual(wl[1].PinYinString, "ren'min'yin'hang");
            Assert.AreEqual(wl[1].Word, "人民银行");
            Assert.AreEqual(wl[2].PinYinString, "dong'li'wu'xian");
            Assert.AreEqual(wl[2].Word, "栋力无限");
        }
    }
}
