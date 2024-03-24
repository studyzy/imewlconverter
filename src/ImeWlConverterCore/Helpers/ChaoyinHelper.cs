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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Studyzy.IMEWLConverter.Helpers
{
    /// <summary>
    /// 根据拼音，超音速写输入法编码生成
    /// </summary>
    public static class ChaoyinHelper
    {
        /*
编码规则：声母+韵母

二字词录入规则
1、如果两个字均为声母本身发音时
声+声+分号键（三码）
例如：知识（LK;）司机（wd;）
依次录入首字声母音（首编码）+次字首编码+分号键
2、如果第一个字是声母本身发音，第二个字是声韵拼合时

声+（声韵）（三码）
例如：舞蹈（wjb）特征（tlt）
依次录入首字母（首编码）及次字（全码）
3、如果第一个字是声韵拼合，第二个字是声母本身分音时
（声韵）+声+分号键（四码）
例如：主席（lvu;）老师（sbk;)
依次录入首字（全码）及次字声母
4、如果两个字均为正常声韵拼合
（声韵）+（声韵）（四码）
例如：总理（yusf）秘书（rfkv）
依次录入首字（全码）次字（全码）


三字词录入规则

1、如果末字为声母本身发音
声+声+声+上引号键（四码）
例如：经理室（dsk'）计算机（dwd'）
依次录入首字（首编码）及次字（首编码）和第三字（首编码）加上引号键

2、如果末字为正常声韵拼合
声+声+（声韵）+分号键（五码）
例如：成功率（bhsn;）现代化（ujgf;）
依次录入首字（首编码）及次字（首编码）和第三字（全码）加分号键


四字词录入规则

1、末字为声母本身发音时
声+声+声+（末字声母双击）（五码）
例如：实事求是（kkmkk）生日快乐（kcess）
依次录入首字、次字、第三字（首编码），末字声母双击
2、末字为正常声韵拼合
声+声+声+（声韵）（五码）
例如：工作顺利（hyksf）科学技术（eudkv）
依次录入首字、次字、第三字（首编码）末字（全码）


五字词

声+声+声+声+声（五码）
例如：为人民服务（vcrov）  中央电视台（lfjkt）

多字词

声+声+声+声+末字（首编码）（五码）

 */

        private static readonly Regex regex = new Regex(@"^[a-zA-Z]+\d$");

        private static IList<string> ShenmuY = new List<string>();

        private static IDictionary<string, string> pinyinCodeMapping;

        private static IDictionary<string, string> PinyinCodeMapping
        {
            get
            {
                if (pinyinCodeMapping == null)
                {
                    pinyinCodeMapping = new Dictionary<string, string>();
                    //todo
                    var lines = DictionaryHelper
                        .GetResourceContent("ChaoyinCodeMapping.txt")
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var array = line.Split('\t');
                        pinyinCodeMapping.Add(array[0], array[1]);
                        if (array[2] == "Y")
                        {
                            ShenmuY.Add(array[0]);
                        }
                    }
                }
                return pinyinCodeMapping;
            }
        }

        /// <summary>
        /// 获得一个拼音对应的超音编码
        /// </summary>
        /// <param name="pinyin"></param>
        /// <returns></returns>
        public static string GetChaoyin(string pinyin)
        {
            if (string.IsNullOrEmpty(pinyin))
            {
                throw new Exception("找不到拼音");
            }
            int yindiao = 10;
            if (regex.IsMatch(pinyin)) //包含音调
            {
                yindiao = Convert.ToInt32(pinyin[pinyin.Length - 1].ToString());
                pinyin = pinyin.Substring(0, pinyin.Length - 1);
            }

            if (!PinyinCodeMapping.ContainsKey(pinyin))
            {
                Debug.WriteLine("Can not find Chaoyin code by pinyin=" + pinyin);
                return null;
            }
            string zy = PinyinCodeMapping[pinyin];
            Debug.WriteLine("Pinyin:" + pinyin + ",Chaoyin:" + zy);
            return zy;
        }

        /// <summary>
        /// 获得一个词语的超音编码
        /// </summary>
        /// <param name="pinyins"></param>
        /// <returns></returns>
        public static string GetChaoyin(IList<string> pinyins)
        {
            StringBuilder result = new StringBuilder();
            if (pinyins.Count == 1)
            {
                return GetChaoyin(pinyins[0]);
            }
            else if (pinyins.Count == 2)
            {
                result.Append(PinyinCodeMapping[pinyins[0]]);
                result.Append(PinyinCodeMapping[pinyins[1]]);
                if (ShenmuY.Contains(pinyins[1]))
                {
                    result.Append(";");
                }
            }
            else if (pinyins.Count == 3)
            {
                /*1、如果末字为声母本身发音
声+声+声+上引号键（四码）
例如：经理室（dsk'）计算机（dwd'）
依次录入首字（首编码）及次字（首编码）和第三字（首编码）加上引号键

2、如果末字为正常声韵拼合
声+声+（声韵）+分号键（五码）
例如：成功率（bhsn;）现代化（ujgf;）
依次录入首字（首编码）及次字（首编码）和第三字（全码）加分号键
*/
                result.Append(PinyinCodeMapping[pinyins[0]][0]);
                result.Append(PinyinCodeMapping[pinyins[1]][0]);
                result.Append(PinyinCodeMapping[pinyins[2]]);
                if (ShenmuY.Contains(pinyins[2]))
                {
                    result.Append("'");
                }
                else
                {
                    result.Append(";");
                }
            }
            else if (pinyins.Count == 4)
            {
                /*1、末字为声母本身发音时
声+声+声+（末字声母双击）（五码）
例如：实事求是（kkmkk）生日快乐（kcess）
依次录入首字、次字、第三字（首编码），末字声母双击
2、末字为正常声韵拼合
声+声+声+（声韵）（五码）
例如：工作顺利（hyksf）科学技术（eudkv）
依次录入首字、次字、第三字（首编码）末字（全码）*/
                result.Append(PinyinCodeMapping[pinyins[0]][0]);
                result.Append(PinyinCodeMapping[pinyins[1]][0]);
                result.Append(PinyinCodeMapping[pinyins[2]][0]);
                if (ShenmuY.Contains(pinyins[3]))
                {
                    result.Append(PinyinCodeMapping[pinyins[3]][0]);
                    result.Append(PinyinCodeMapping[pinyins[3]][0]);
                }
                else
                {
                    result.Append(PinyinCodeMapping[pinyins[3]]);
                }
            }
            else if (pinyins.Count == 5)
            {
                //声+声+声+声+声（五码）
                result.Append(PinyinCodeMapping[pinyins[0]][0]);
                result.Append(PinyinCodeMapping[pinyins[1]][0]);
                result.Append(PinyinCodeMapping[pinyins[2]][0]);
                result.Append(PinyinCodeMapping[pinyins[3]][0]);
                result.Append(PinyinCodeMapping[pinyins[4]][0]);
            }
            else
            {
                //声+声+声+声+末字（首编码）（五码）
                result.Append(PinyinCodeMapping[pinyins[0]][0]);
                result.Append(PinyinCodeMapping[pinyins[1]][0]);
                result.Append(PinyinCodeMapping[pinyins[2]][0]);
                result.Append(PinyinCodeMapping[pinyins[3]][0]);
                result.Append(PinyinCodeMapping[pinyins[pinyins.Count - 1]][0]);
            }

            return result.ToString();
        }

        private static string GetYindiaoZhuyin(int yindiao)
        {
            switch (yindiao)
            {
                case 1:
                    return "";
                case 2:
                    return "q";
                case 3:
                    return "z";
                case 4:
                    return "p";
                case 5:
                    return "";
                default:
                    return "";
            }
        }

        private static int GetYindiaoPinyin(char yindiao)
        {
            switch (yindiao)
            {
                case 'q':
                    return 2;
                case 'z':
                    return 3;
                case 'p':
                    return 4;

                default:
                    return 1;
            }
        }
    }
}
