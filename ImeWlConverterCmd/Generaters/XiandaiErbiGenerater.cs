using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class XiandaiErbiGenerater : ErbiGenerater
    {
        protected override int DicColumnIndex
        {
            get { return 1; }
        }

        /// <summary>
        ///     现代二笔与普通二笔不同的是，现代二笔组词的时候，每个词取2码，并没有4码长度的限制。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        protected override IList<IList<string>> GetErbiCode(string str, IList<string> py)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var codes = new List<IList<string>>();

            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    codes.Add(Get1CharCode(c, py[i]));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            return codes;
        }
    }
}