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
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test;

[TestFixture]
internal class PinyinJiaJiaTest : BaseTest
{
    [OneTimeSetUp]
    public override void InitData()
    {
        importer = new PinyinJiaJia();
        exporter = new PinyinJiaJia();
    }

    protected override string StringData => Resource4Test.PinyinJiajia;

    [Test]
    public void ExportLine()
    {
        var txt = exporter.ExportLine(WlData);
        Assert.That("深shen蓝lan测ce试shi", Is.EqualTo(txt));
    }

    [Test]
    public void ImportNoPinyin()
    {
        var wl = importer.ImportLine("深蓝测试");
        Assert.That(1, Is.EqualTo(wl.Count));
        Assert.That("shen'lan'ce'shi", Is.EqualTo(wl[0].PinYinString));
    }

    [Test]
    public void ImportWithPinyinFull()
    {
        var wl = importer.ImportLine("深shen蓝lan居ju");
        Assert.That(1, Is.EqualTo(wl.Count));
        Assert.That("shen'lan'ju", Is.EqualTo(wl[0].PinYinString));
        Assert.That("深蓝居", Is.EqualTo(wl[0].Word));
    }

    [Test]
    public void ImportWithPinyinPart()
    {
        var wl = ((IWordLibraryTextImport)importer).ImportText(StringData);
        Assert.That(10, Is.EqualTo(wl.Count));
        Assert.That("ren'min'hen'xing", Is.EqualTo(wl[0].PinYinString));
        Assert.That("人民很行", Is.EqualTo(wl[0].Word));
        Assert.That("ren'min'yin'hang", Is.EqualTo(wl[1].PinYinString));
        Assert.That("人民银行", Is.EqualTo(wl[1].Word));
        Assert.That("dong'li'wu'xian", Is.EqualTo(wl[2].PinYinString));
        Assert.That("栋力无限", Is.EqualTo(wl[2].Word));
    }
}
