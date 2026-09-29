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

using System.IO;
using System.Text;
using Xunit;
using ImeWlConverter.Formats.PinyinJiaJia;

namespace Studyzy.IMEWLConverter.Test;

public class PinyinJiaJiaTest : BaseTest
{
    public PinyinJiaJiaTest()
    {
        importer = new PinyinJiaJiaImporter();
        exporter = new PinyinJiaJiaExporter();
    }

    protected override string StringData => Resource4Test.PinyinJiajia;

    [Fact]
    public void ImportWithPinyinFull()
    {
        var text = "深shen蓝lan居ju";
        var bytes = Encoding.Unicode.GetBytes(text);
        using var ms = new MemoryStream(bytes);
        var result = importer!.ImportAsync(ms).GetAwaiter().GetResult();
        Assert.Single(result.Entries);
        Assert.Equal("深蓝居", result.Entries[0].Word);
    }

    [Fact]
    public void ImportFromResource()
    {
        var bytes = Encoding.Unicode.GetBytes(StringData);
        using var ms = new MemoryStream(bytes);
        var result = importer!.ImportAsync(ms).GetAwaiter().GetResult();
        Assert.True(result.Entries.Count >= 8);
    }
}
