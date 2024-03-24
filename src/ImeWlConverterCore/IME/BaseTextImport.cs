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
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    public abstract class BaseTextImport : BaseImport
    {
        public abstract WordLibraryList ImportLine(string line);
        public abstract Encoding Encoding { get; }

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path);
            return ImportText(str);
        }

        protected virtual bool IsContent(string line)
        {
            return true;
        }

        public WordLibraryList ImportText(string str)
        {
            var wlList = new WordLibraryList();
            string[] lines = str.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            CountWord = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                CurrentStatus = i;
                try
                {
                    if (IsContent(line))
                    {
                        wlList.AddWordLibraryList(ImportLine(line));
                    }
                }
                catch
                {
                    SendImportLineErrorNotice("无效的词条，解析失败：" + line);
                }
            }
            return wlList;
        }
    }
}
