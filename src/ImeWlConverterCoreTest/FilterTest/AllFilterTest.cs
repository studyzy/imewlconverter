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
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;

namespace Studyzy.IMEWLConverter.Test.FilterTest
{
    class AllFilterTest
    {
        [TestCase("123.456", true)]
        [TestCase("abc!efg?", true)]
        [TestCase("1《深蓝词库转换》", false)]
        [TestCase("2大家，好", false)]
        [TestCase("3转换成功。", false)]
        public void ChinesePunctuationFilterTest(string word, bool isKeep)
        {
            var wl = new WordLibrary();
            wl.Word = word;
            ChinesePunctuationFilter filter = new ChinesePunctuationFilter();
            Assert.AreEqual(filter.IsKeep(wl), isKeep);
        }

        [TestCase("深蓝", true)]
        [TestCase("深 蓝", false)]
        [TestCase(" 深蓝", false)]
        [TestCase("深蓝 ", false)]
        public void SpaceFilterTest(string word, bool isKeep)
        {
            var wl = new WordLibrary();
            wl.Word = word;
            SpaceFilter filter = new SpaceFilter();
            Assert.AreEqual(filter.IsKeep(wl), isKeep);
        }
    }
}
