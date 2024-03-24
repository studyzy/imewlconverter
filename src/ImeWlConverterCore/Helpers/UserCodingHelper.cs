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

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class UserCodingHelper
    {
        //private static string filePath = "";

        //private static IDictionary<char, string> dictionary = new Dictionary<char, string>();

        //public static string FilePath
        //{
        //    get { return filePath; }
        //    set
        //    {
        //        filePath = value;
        //        dictionary = GetCodingDict(FileOperationHelper.ReadFile(filePath));
        //    }
        //}
        //public static IDictionary<char,string> MappingDictionary
        //{
        //    get { return dictionary; }
        //    set { dictionary = value; }
        //}

        //public static string GetCharCoding(char c, string codingFilePath = null)
        //{
        //    if (codingFilePath != null && codingFilePath != filePath)
        //    {
        //        dictionary = GetCodingDict(FileOperationHelper.ReadFile(codingFilePath));
        //        filePath = codingFilePath;
        //    }
        //    if (dictionary.ContainsKey(c))
        //    {
        //        return dictionary[c];
        //    }
        //    else
        //    {
        //        throw new ArgumentOutOfRangeException("从编码文件中找不到字[" + c + "]对应的编码");
        //    }
        //}

        public static IDictionary<char, IList<string>> GetCodingDict(
            string filePath,
            Encoding encoding
        )
        {
            string codingContent = FileOperationHelper.ReadFile(filePath, encoding);
            var dic = new Dictionary<char, IList<string>>();
            foreach (
                string line in codingContent.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                )
            )
            {
                string[] l = line.Split('\t');
                if (l.Length != 2)
                {
                    throw new Exception("无效的自定义编码格式：" + line);
                }
                char c = l[0][0];
                string code = l[1];
                if (!dic.ContainsKey(c))
                {
                    dic.Add(c, new List<string> { code });
                }
                else
                {
                    dic[c].Add(code);
                }
            }
            return dic;
        }

        //static Regex wcRegex = new Regex(@"[\u4E00-\u9FA5]+,\w+");
        //private static bool IsWordAndCode(string line)
        //{
        //    return wcRegex.IsMatch(line);
        //}
    }
}
