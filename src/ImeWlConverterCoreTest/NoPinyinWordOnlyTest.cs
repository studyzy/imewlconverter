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

using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    public class NoPinyinWordOnlyTest : BaseTest
    {
        [OneTimeSetUp]
        public override void InitData()
        {
            importer = new NoPinyinWordOnly();
            exporter = new NoPinyinWordOnly();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected override string StringData
        {
            get { return Resource4Test.NoPinyinWordOnly; }
        }

        [Test]
        public void TestExport()
        {
            string txt = exporter.Export(WlListData)[0];
            Assert.AreEqual(txt, "深蓝测试\r\n词库转换\r\n");
        }

        [Test]
        public void TestExportLine()
        {
            string txt = exporter.ExportLine(WlData);
            Assert.AreEqual(txt, "深蓝测试");
        }

        [Test]
        public void TestImport()
        {
            WordLibraryList wl = ((IWordLibraryTextImport)importer).ImportText(StringData);
            Assert.AreEqual(wl.Count, 10);
        }

        [Test]
        public void TestImportFile()
        {
            WordLibraryList wll = importer.Import(GetFullPath("纯汉字.txt"));
            Assert.Greater(wll.Count, 0);
        }
    }
}
