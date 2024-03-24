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

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class CollectionHelper
    {
        //public static string[] ToArray(IList<string> str)
        //{
        //    var result = new string[str.Count];
        //    for (int i = 0; i < str.Count; i++)
        //    {
        //        result[i] = str[i];
        //    }
        //    return result;
        //}

        //public static string ListToString(IList<string> list, string split = "")
        //{
        //    if (list.Count == 1)
        //    {
        //        return list[0];
        //    }
        //    return string.Join(split, ToArray(list));
        //}

        public static string GetString(IEnumerable<string> list, string split, BuildType buildType)
        {
            var sb = new StringBuilder();

            if (list == null)
            {
                return "";
            }

            foreach (string s in list)
            {
                sb.Append(s + split);
            }
            if (sb.Length == 0)
            {
                return "";
            }
            if (buildType == BuildType.RightContain)
            {
                return sb.ToString();
            }
            if (buildType == BuildType.FullContain)
            {
                return split + sb;
            }
            string str = sb.ToString();
            if (split.Length > 0)
            {
                str = str.Remove(sb.Length - 1);
            }
            if (buildType == BuildType.None)
            {
                return str;
            }
            return split + str;
        }

        /// <summary>
        ///     Cartesian product
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        private static string Descartes(
            IList<IList<string>> list,
            int count,
            IList<string> result,
            string data
        )
        {
            string temp = data;
            //获取当前数组
            IList<string> astr = list[count];
            //循环当前数组
            foreach (string item in astr)
            {
                if (count + 1 < list.Count)
                {
                    temp += Descartes(list, count + 1, result, data + item);
                }
                else
                {
                    result.Add(data + item);
                }
            }
            return temp;
        }

        /// <summary>
        ///     多音字情况下，做笛卡尔积
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static IList<string> Descartes(IList<IList<string>> codes)
        {
            var result = new List<string>();
            Descartes(codes, 0, result, string.Empty);
            return result;
        }

        /// <summary>
        ///     只取每个字的第一个编码，返回这些编码的List
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static IList<string> DescarteIndex1(IList<IList<string>> codes)
        {
            var result = new List<string>();
            foreach (var code in codes)
            {
                result.Add(code[0]);
            }
            return result;
        }

        public static IList<string> CartesianProduct(IList<IList<string>> codes, String split)
        {
            int count = 1;
            foreach (var code in codes)
            {
                count *= code.Count;
            }
            var result = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var line = new string[codes.Count];
                for (int j = 0; j < codes.Count; j++)
                {
                    line[j] = codes[j][i % codes[j].Count];
                }
                result.Add(String.Join(split, line));
            }
            return result;
        }

        public static IList<string> CartesianProduct(
            IList<IList<string>> codes,
            String split,
            BuildType buildType
        )
        {
            IList<string> list = CartesianProduct(codes, split);
            if (buildType == BuildType.None)
                return list;
            var result = new List<string>();
            foreach (string line in list)
            {
                string newline = line;
                if (buildType == BuildType.FullContain || buildType == BuildType.LeftContain)
                {
                    newline = split + newline;
                }
                if (buildType == BuildType.FullContain || buildType == BuildType.RightContain)
                {
                    newline = newline + split;
                }
                result.Add(newline);
            }
            return result;
        }
    }
}
