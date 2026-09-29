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

using Xunit;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.CodeData;
using ImeWlConverter.Core.CodeGeneration.Generators;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest;

public class ZhuyinTest
{
    private readonly ICodeGenerator generator;

    public ZhuyinTest()
    {
        var resources = new EmbeddedResourceProvider();
        generator = new ZhuyinCodeGenerator(
            new TerraPinyinCodeGenerator(
                new PinyinCodeGenerator(
                    new PinyinTable(new CodeTableLibrary(resources)),
                    resources),
                new PinyinTable(new CodeTableLibrary(resources))),
            new ZhuyinTable(resources));
    }

    [Fact]
    public void TestGetOneWordPinyin()
    {
    }

    [Theory]
    [InlineData("曾毅", "ㄗㄥ,ㄧˋ")]
    [InlineData("北京吃饭", "ㄅㄟˇ,ㄐㄧㄥ,ㄔ,ㄈㄢˋ")]
    [InlineData("煤矿", "ㄇㄟˊ,ㄎㄨㄤˋ")]
    [InlineData("故乡", "ㄍㄨˋ,ㄒㄧㄤ")]
    public void TestGetLongWordsPinyin(string str, string py)
    {
        var result = generator.GenerateCode(str);
        var primaryCode = result.GetPrimaryCode(",");
        Assert.Equal(py, primaryCode);
    }
}
