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

using System.Diagnostics;
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Test.GeneraterTest
{
    class TerraPinyinTest
    {
        private IWordCodeGenerater generater = new TerraPinyinGenerater();

        [Test]
        public void TestPinyin2TerraPinyin()
        {
            WordLibrary wl = new WordLibrary()
            {
                Word = "深蓝",
                Rank = 123,
                PinYin = new[] { "shen", "lan" },
                CodeType = CodeType.Pinyin
            };
            generater.GetCodeOfWordLibrary(wl);
            foreach (var py in wl.Codes)
            {
                Debug.WriteLine(py);
            }
        }

        [TestCase("曾经", "ceng2 jin1")]
        [TestCase("曾毅", "zeng1 yi4")]
        [TestCase("音乐", "yin1 yue4")]
        [TestCase("快乐", "kuai4 le4")]
        public void TestChar2TerraPinyin(string word, string pinyin)
        {
            WordLibrary wl = new WordLibrary()
            {
                Word = word,
                Rank = 123,
                CodeType = CodeType.NoCode
            };
            generater.GetCodeOfWordLibrary(wl);
            foreach (var py in wl.Codes.ToCodeString(" "))
            {
                Debug.WriteLine(py);
            }
        }
    }
}
