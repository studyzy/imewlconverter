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
using Xunit;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Formats.SougouScel;

namespace Studyzy.IMEWLConverter.Test;

public class SougouPinyinScelTest : BaseTest
{
    public SougouPinyinScelTest()
    {
        importer = new SougouScelImporter();
    }

    protected override string StringData => throw new NotImplementedException();

    [Theory(Skip = "Large file test, run manually")]
    [InlineData("诗词名句大全.scel")]
    [Trait("Category", "Explicit")]
    public void TestImportBigScel(string filePath)
    {
        var result = ImportFromFile(GetFullPath(filePath));
        var lib = result.Entries;
        Assert.True(lib.Count > 0);
        Assert.False(string.IsNullOrEmpty(lib[0].Word));

        Assert.Equal(342179, lib.Count);
        Assert.Equal(CodeType.Pinyin, lib[0].CodeType);
        Assert.False(lib[0].IsEnglish);
        Assert.Equal("a'cheng'yi'wen'you'bi'duan", lib[0].Code?.GetPrimaryCode("'"));
        Assert.Equal(0, lib[0].Rank);
        Assert.Equal("阿秤亦闻有笔端", lib[0].Word);
    }

    [Theory]
    [InlineData("唐诗300首【官方推荐】.scel")]
    public void TestImportSmallScel(string filePath)
    {
        var result = ImportFromFile(GetFullPath(filePath));
        var lib = result.Entries;
        Assert.True(lib.Count > 0);
        Assert.False(string.IsNullOrEmpty(lib[0].Word));

        Assert.Equal(3563, lib.Count);
        Assert.Equal(CodeType.Pinyin, lib[0].CodeType);
        Assert.False(lib[0].IsEnglish);
        Assert.Equal("ai'jiang'tou", lib[0].Code?.GetPrimaryCode("'"));
        Assert.Equal(0, lib[0].Rank);
        Assert.Equal("哀江头", lib[0].Word);
    }
}
