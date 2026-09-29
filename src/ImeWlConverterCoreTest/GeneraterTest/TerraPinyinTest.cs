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
using ImeWlConverter.Core.CodeGeneration.Generators;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest;

public class TerraPinyinTest
{
    private readonly ICodeGenerator generator = TestCodeData.CreateTerraGenerator();

    [Fact]
    public void TestPinyin2TerraPinyin()
    {
        var result = generator.GenerateCode("深蓝");
        Assert.NotNull(result);
        Assert.Equal(2, result.Segments.Count);
    }

    [Theory]
    [InlineData("曾经", "ceng2 jing1")]
    [InlineData("曾毅", "zeng1 yi4")]
    [InlineData("音乐", "yin1 yue4")]
    [InlineData("快乐", "kuai4 le4")]
    public void TestChar2TerraPinyin(string word, string pinyin)
    {
        var result = generator.GenerateCode(word);
        var primaryCode = result.GetPrimaryCode(" ");
        Assert.Equal(pinyin, primaryCode);
    }
}
