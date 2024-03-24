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
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public interface IWordLibraryImport
    {
        event Action<string> ImportLineErrorNotice;
        int CountWord { get; set; }
        int CurrentStatus { get; set; }
        bool IsText { get; }
        CodeType CodeType { get; }

        //int DefaultRank { get; set; }
        WordLibraryList Import(string path);
        WordLibraryList ImportLine(string str);
        //Form ImportConfigForm { get; }
    }

    public interface IWordLibraryTextImport : IWordLibraryImport
    {
        Encoding Encoding { get; }
        WordLibraryList ImportText(string text);
    }

    public interface IWordLibraryExport
    {
        event Action<string> ExportErrorNotice;
        Encoding Encoding { get; }
        CodeType CodeType { get; }

        /// <summary>
        /// 导出成多个文件
        /// </summary>
        /// <param name="wlList"></param>
        /// <returns></returns>
        IList<string> Export(WordLibraryList wlList);
        string ExportLine(WordLibrary wl);
    }

    public interface IMultiCodeType
    {
        CodeType CodeType { get; }
    }

    public interface IStreamPrepare
    {
        void Prepare();
    }
}
