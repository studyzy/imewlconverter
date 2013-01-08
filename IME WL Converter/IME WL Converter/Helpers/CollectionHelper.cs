using System.Collections.Generic;

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
    }
}