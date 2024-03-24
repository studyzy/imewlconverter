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
using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Test
{
    public abstract class BaseTest
    {
        /// <summary>
        /// 深蓝测试
        /// </summary>
        protected WordLibrary WlData = new WordLibrary
        {
            Rank = 10,
            PinYin = new[] { "shen", "lan", "ce", "shi" },
            Word = "深蓝测试"
        };

        protected IWordLibraryExport exporter;
        protected IWordLibraryImport importer;
        protected abstract string StringData { get; }

        /// <summary>
        /// 深蓝测试
        /// 词库转换
        /// </summary>
        protected WordLibraryList WlListData
        {
            get
            {
                var wordLibrary = new WordLibrary
                {
                    Rank = 80,
                    PinYin = new[] { "ci", "ku", "zhuan", "huan" },
                    Word = "词库转换"
                };
                return new WordLibraryList { WlData, wordLibrary };
            }
        }

        public abstract void InitData();

        protected string GetFullPath(string fileName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Test", fileName);
        }
    }
}
