﻿/*
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

using Studyzy.IMEWLConverter.Helpers;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class BaiduWordRankGenerater : IWordRankGenerater
    {
        private static string API = "http://www.baidu.com/s?wd={0}";
        private static readonly Regex regex = new Regex("百度为您找到相关结果约([0-9\\,]+)个");
        public bool ForceUse { get; set; }
        public int GetRank(string word)
        {
            try
            {
                string result = HttpHelper.GetHtml(string.Format(API, word));
                if (regex.IsMatch(result))
                {
                    string num = regex.Match(result).Groups[1].Value;
                    return Convert.ToInt32(num.Replace(",", ""));
                }
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}