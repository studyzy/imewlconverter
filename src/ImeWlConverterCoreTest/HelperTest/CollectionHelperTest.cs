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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Test.HelperTest;

internal class CollectionHelperTest
{
    [Test]
    public void TestCartesianProduct1()
    {
        IList<IList<string>> list = new List<IList<string>>();
        list.Add(new List<string> { "a" });
        list.Add(new List<string> { "b", "c", "d" });
        list.Add(new List<string> { "e", "f" });
        var result = CollectionHelper.CartesianProduct(list, ",");
        var array = result.ToArray();
        Assert.That(array, Does.Contain("a,b,e"));
        Assert.That(array, Does.Contain("a,b,f"));
        Assert.That(array, Does.Contain("a,c,e"));
        Assert.That(array, Does.Contain("a,c,f"));
        Assert.That(array, Does.Contain("a,d,e"));
        Assert.That(array, Does.Contain("a,d,f"));
        Assert.That(6, Is.EqualTo(result.Count));
    }

    [Test]
    public void TestCartesianProduct2()
    {
        IList<IList<string>> list = new List<IList<string>>();
        list.Add(new List<string> { "a" });
        list.Add(new List<string> { "b" });
        list.Add(new List<string> { "e" });
        var result = CollectionHelper.CartesianProduct(list, ",");
        var array = result.ToArray();
        Assert.That(array, Does.Contain("a,b,e"));

        Assert.That(1, Is.EqualTo(result.Count));
    }
}
