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
using Xunit;
using ImeWlConverter.Formats.BaiduBdict;

namespace Studyzy.IMEWLConverter.Test;

public class BaiduBdictTest : BaseTest
{
    public BaiduBdictTest()
    {
        importer = new BaiduBdictImporter();
    }

    protected override string StringData => throw new NotImplementedException();

    [Theory]
    [InlineData("movie.bdict", 60000)] // 新版头部（0x60 处有词条区结束偏移）
    [InlineData("travel.bdict", 300)] // 旧版头部（0x60 为 0，用 0x44 长度推算），旅游词库 302 条
    [InlineData("百度官网.bdict", 9)] // 百度官网下载，编程词汇 9 条
    public void TestImport(string file, int minCount)
    {
        var result = ImportFromFile(GetFullPath(file));
        Assert.NotNull(result.Entries);
        Assert.True(result.Entries.Count >= minCount, $"{file} 仅导入 {result.Entries.Count} 条（预期 >= {minCount}）");
    }
}
