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
using NUnit.Framework;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter.Test
{
    [TestFixture]
    public class BaiduShoujiTest : BaseTest
    {
        #region SetUp/Teardown

        [OneTimeSetUp]
        public override void InitData()
        {
            exporter = new BaiduShouji();
            importer = new BaiduShouji();
        }

        #endregion

        protected override string StringData
        {
            get { throw new NotImplementedException(); }
        }

        [Test]
        public void TestExport() { }
    }
}
