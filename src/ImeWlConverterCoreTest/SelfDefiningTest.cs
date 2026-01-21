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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test;

[TestFixture]
public class SelfDefiningTest
{
    //private SelfDefining selfDefining;
    //[OneTimeSetUp]
    //public void Init()
    //{
    //    selfDefining=new SelfDefining();
    //}
    [Test]
    public void TestPinyinString2WL()
    {
        var parser = new ParsePattern
        {
            IsPinyinFormat = true,
            CodeSplitType = BuildType.FullContain,
            CodeSplitString = ",",
            ContainCode = true,
            ContainRank = true,
            SplitString = " ",
            Sort = new List<int> { 2, 1, 3 }
        };
        var str = "深蓝 ,shen,lan, 1";
        var selfDefining = new SelfDefining();
        selfDefining.UserDefiningPattern = parser;
        var wl = selfDefining.ImportLine(str)[0];

        Assert.That("shen", Is.EqualTo(wl.Codes[0][0]));
        Assert.That("lan", Is.EqualTo(wl.Codes[1][0]));
        Assert.That(1, Is.EqualTo(wl.Rank));
    }

    [Test]
    public void TestWordLibrary2String()
    {
        var parser = new ParsePattern
        {
            IsPinyinFormat = true,
            CodeSplitType = BuildType.FullContain,
            CodeSplitString = ",",
            ContainCode = true,
            ContainRank = true,
            SplitString = "|",
            CodeType = CodeType.Pinyin,
            Sort = new List<int> { 2, 1, 3 }
        };
        var wl = new WordLibrary
        {
            Word = "深蓝",
            Rank = 123,
            CodeType = CodeType.Pinyin
        };
        wl.Codes = new Code();
        wl.Codes.Add(new[] { "shen" });
        wl.Codes.Add(new[] { "lan" });
        var selfDefining = new SelfDefining();
        selfDefining.UserDefiningPattern = parser;
        var str = selfDefining.ExportLine(wl);
        Assert.That("深蓝|,shen,lan,|123", Is.EqualTo(str));
    }

    [Test]
    public void TestGeneratePinyinThen2String()
    {
        var parser = new ParsePattern
        {
            IsPinyinFormat = true,
            CodeSplitType = BuildType.FullContain,
            CodeSplitString = "~",
            ContainCode = true,
            ContainRank = true,
            SplitString = "|",
            CodeType = CodeType.Pinyin,
            LineSplitString = "\r",
            Sort = new List<int> { 2, 1, 3 }
        };
        var wll = new WordLibraryList();
        var wl = new WordLibrary
        {
            Word = "深蓝",
            Rank = 123,
            CodeType = CodeType.UserDefine
        };
        wl.Codes = new Code();
        wl.Codes.Add(new[] { "sn" });
        wl.Codes.Add(new[] { "ln" });
        wll.Add(wl);
        var selfDefining = new SelfDefining();
        selfDefining.UserDefiningPattern = parser;
        var str = selfDefining.Export(wll);
        Assert.That("深蓝|~sn~ln~|123\r", Is.EqualTo(str[0]));
    }

    private WordLibrary WlData
    {
        get
        {
            return new WordLibrary
            {
                Rank = 10,
                PinYin = new[] { "shen", "lan", "ce", "shi" },
                Word = "深蓝测试",
                CodeType = CodeType.Pinyin
            };
        }
    }

    [Test]
    public void TestExportPinyinDifferentFormatWL()
    {
        var p = new ParsePattern();
        p.Sort = new List<int> { 3, 2, 1 };
        p.SplitString = "$";
        p.ContainRank = false;
        p.CodeSplitString = "_";
        p.CodeSplitType = BuildType.None;
        p.IsPinyinFormat = true;
        p.CodeType = CodeType.Pinyin;
        var selfDefining = new SelfDefining();

        selfDefining.UserDefiningPattern = p;
        Console.WriteLine("CodeType:" + selfDefining.UserDefiningPattern.CodeType);
        var str1 = selfDefining.Export(new WordLibraryList { WlData });
        Console.WriteLine(str1[0]);
        Assert.That("深蓝测试$shen_lan_ce_shi\r\n", Is.EqualTo(str1[0]));
    }

    [Test]
    public void TestExportExtCodeWL()
    {
        var selfDefining = new SelfDefining();
        selfDefining.UserDefiningPattern = InitPattern();
        selfDefining.UserDefiningPattern.MappingTablePath = "./Test/array30.txt";
        var str = selfDefining.Export(new WordLibraryList { WlData });
        Debug.WriteLine(str);
        //Assert.That(str, Is.Not.Null.And.Not.Empty);
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
        var str = "深蓝词库转换测试代码";
        var list = new WordLibraryList();
        var ts = "";
        foreach (var c in str)
        {
            ts += c;
            list.Add(
                new WordLibrary
                {
                    Rank = 10,
                    IsEnglish = false,
                    Word = ts
                }
            );
        }

        var selfDefining = new SelfDefining();
        selfDefining.UserDefiningPattern = InitPattern();
        selfDefining.UserDefiningPattern.MappingTablePath = "./Test/array30.txt";
        var x = selfDefining.Export(list);
        Debug.WriteLine(x);
        Assert.That(str, Is.Not.Null);
    }

    [Test]
    public void TestImportTxt()
    {
        var txt = "深藍 shen,lan 12345";
        var pp = new ParsePattern();
        pp.Sort = new List<int> { 2, 1, 3 };
        pp.IsPinyinFormat = true;
        pp.CodeType = CodeType.Pinyin;
        pp.CodeSplitString = ",";
        pp.SplitString = " ";
        pp.CodeSplitType = BuildType.None;
        var selfDefining = new SelfDefining();
        selfDefining.UserDefiningPattern = pp;

        var x = selfDefining.ImportLine(txt);
        Debug.WriteLine(x[0].ToString());
        Assert.That("深藍", Is.EqualTo(x[0].Word));
    }

    private ParsePattern InitPattern()
    {
        var pp = new ParsePattern();
        pp.MutiWordCodeFormat =
            @"code_e2=p11+p12+p21+p22
code_e3=p11+p21+p31+p32
code_a4=p11+p21+p31+n11";
        pp.IsPinyinFormat = false;
        pp.ContainRank = false;
        pp.SplitString = " ";
        pp.ContainCode = true;
        pp.TextEncoding = Encoding.UTF8;
        pp.CodeType = CodeType.UserDefine;
        return pp;
    }
}
