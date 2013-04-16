using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class CollectionHelper
    {
        public static string[] ToArray(IList<string> str)
        {
            var result = new string[str.Count];
            for (int i = 0; i < str.Count; i++)
            {
                result[i] = str[i];
            }
            return result;
        }

        public static string ListToString(IList<string> list, string split = "")
        {
            if (list.Count == 1)
            {
                return list[0];
            }
            return string.Join(split, ToArray(list));
        }
        public static string GetString(IEnumerable<string> list, string split, BuildType buildType)
        {
            var sb = new StringBuilder();
           

            if (list == null )
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
            else //BuildType.LeftContain
            {
                return split + str;
            }
        }

        /// <summary>
        /// Cartesian product
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        private static string Descartes(IList<IList<string>> list, int count, IList<string> result, string data)
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

        public static IList<string> Descartes(IList<IList<string>> codes)
        {
            var result = new List<string>();
            Descartes(codes, 0, result, string.Empty);
            return result;
        }
        public static IList<string> DescarteIndex1(IList<IList<string>> codes)
        {
            var result = new List<string>();
            foreach (var code in codes)
            {
                result.Add(code[0]);
            }
            return result;
        }
    }
}