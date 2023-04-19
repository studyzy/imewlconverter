﻿/*
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
using System;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class GooglePinyinTest : BaseTest
    {
        #region SetUp/Teardown

        [OneTimeSetUp]
        public override void InitData()
        {
            exporter = new GooglePinyin();
            importer = new GooglePinyin();
        }

        #endregion

        protected override string StringData
        {
            get { return Resource4Test.GooglePinyin; }
        }

        [Test]
        public void TestExport()
        {
            var txt = exporter.Export(WlListData)[0];
            Assert.IsTrue(txt.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length == 2);
        }

        [Test]
        public void TestExportLine()
        {
            string txt = exporter.ExportLine(WlData);
            Assert.AreEqual(txt, "深蓝测试\t10\tshen lan ce shi");
        }

        [Test]
        public void TestImport()
        {
            WordLibraryList list = ((IWordLibraryTextImport)importer).ImportText(StringData);
            Assert.IsNotNull(list);
            Assert.AreEqual(list.Count, 10);
        }
    }
}