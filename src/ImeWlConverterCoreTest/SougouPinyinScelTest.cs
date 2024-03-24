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
using System.IO;
using System.Net;
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    internal class SougouPinyinScelTest : BaseTest
    {
        [OneTimeSetUp]
        public override void InitData()
        {
            importer = new SougouPinyinScel();
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
                "Scel格式是二进制文件，不支持流转换"
            );
        }

        [TestCase("诗词名句大全.scel"), Description("较慢，按需启用"), Explicit()]
        public void TestImportBigScel(string filePath)
        {
            var lib = importer.Import(GetFullPath(filePath));
            Assert.Greater(lib.Count, 0);
            Assert.That(lib[0].Word, Is.Not.Null.And.Not.Empty);

            Assert.AreEqual(lib.Count, 342179);
            Assert.AreEqual(lib[0].CodeType, Studyzy.IMEWLConverter.Entities.CodeType.Pinyin);
            Assert.AreEqual(lib[0].IsEnglish, false);
            Assert.AreEqual(lib[0].PinYinString, "a'cheng'yi'wen'you'bi'duan");
            Assert.AreEqual(lib[0].Rank, 0);
            Assert.AreEqual(lib[0].SingleCode, "a");
            Assert.AreEqual(lib[0].Word, "阿秤亦闻有笔端");
        }

        [TestCase("唐诗300首【官方推荐】.scel")]
        public void TestImportSmallScel(string filePath)
        {
            var lib = importer.Import(GetFullPath(filePath));
            Assert.Greater(lib.Count, 0);
            Assert.That(lib[0].Word, Is.Not.Null.And.Not.Empty);

            Assert.AreEqual(lib.Count, 3563);
            Assert.AreEqual(lib[0].CodeType, Studyzy.IMEWLConverter.Entities.CodeType.Pinyin);
            Assert.AreEqual(lib[0].IsEnglish, false);
            Assert.AreEqual(lib[0].PinYinString, "ai'jiang'tou");
            Assert.AreEqual(lib[0].Rank, 0);
            Assert.AreEqual(lib[0].SingleCode, "ai");
            Assert.AreEqual(lib[0].Word, "哀江头");
            Assert.AreEqual(lib[0].WubiCode, null);
        }

        [TestCase("唐诗300首【官方推荐】.scel")]
        public void TestListScelInfo(string filePath)
        {
            var info = SougouPinyinScel.ReadScelInfo(GetFullPath(filePath));
            Assert.That(info, Is.Not.Null.And.Not.Empty);
            foreach (var item in info)
                TestContext.WriteLine(item.Key + ": " + item.Value);

            Assert.AreEqual(info["CountWord"], "3563");
            Assert.AreEqual(info["Name"], "唐诗300首【官方推荐】");
            Assert.AreEqual(info["Type"], "诗词歌赋");
            Assert.AreEqual(info["Info"], "包含唐诗300首的所有诗人、诗名、诗句。");
            Assert.True(info["Sample"].Contains("张九龄 侧见双翠鸟"));
        }

        [TestCase(
            "https://pinyin.sogou.com/d/dict/download_cell.php?id=4&name=%E7%BD%91%E7%BB%9C%E6%B5%81%E8%A1%8C%E6%96%B0%E8%AF%8D%E3%80%90%E5%AE%98%E6%96%B9%E6%8E%A8%E8%8D%90%E3%80%91&f=detail"
        )]
        [Description("按需使用。下载1MB，用时1秒"), Explicit()]
        public void TestLatestScelOnWeb(string url)
        {
            var filePath = Path.GetTempFileName();
            WebClient dl = new WebClient();
            dl.DownloadFile(url, filePath);
            var info = SougouPinyinScel.ReadScelInfo(GetFullPath(filePath));
            foreach (var item in info)
                TestContext.WriteLine(item.Key + ": " + item.Value);
            Assert.Greater(info["CountWord"], "10000");
            Assert.AreEqual(info["Name"], "网络流行新词【官方推荐】");
            Assert.AreEqual(info["Type"], "北京");
            Assert.AreEqual(info["Info"], "搜狗搜索自动生成的流行新词，每周更新。");
            Assert.That(info["Sample"], Is.Not.Null.And.Not.Empty);

            var lib = importer.Import(GetFullPath(filePath));
            Assert.AreEqual(info["CountWord"], lib.Count);
            Assert.That(lib[0].Word, Is.Not.Null.And.Not.Empty);
        }
    }
}
