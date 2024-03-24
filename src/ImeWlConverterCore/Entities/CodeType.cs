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

namespace Studyzy.IMEWLConverter.Entities
{
    public enum CodeType
    {
        /// <summary>
        ///     用户自定义短语
        /// </summary>
        UserDefinePhrase,

        /// <summary>
        ///     五笔86
        /// </summary>
        Wubi,
        Wubi98,

        /// <summary>
        /// 五笔新世纪
        /// </summary>
        WubiNewAge,

        /// <summary>
        ///     郑码
        /// </summary>
        Zhengma,

        /// <summary>
        ///     仓颉
        /// </summary>
        Cangjie,

        /// <summary>
        ///     未知
        /// </summary>
        Unknown,

        /// <summary>
        ///     用户自定义
        /// </summary>
        UserDefine,

        /// <summary>
        ///     拼音
        /// </summary>
        Pinyin,

        /// <summary>
        ///     永码
        /// </summary>
        Yong,

        /// <summary>
        ///     青松二笔
        /// </summary>
        QingsongErbi,

        /// <summary>
        ///     超强二笔30键
        /// </summary>
        ChaoqiangErbi,

        /// <summary>
        ///     超强音形(二笔)
        /// </summary>
        ChaoqingYinxin,

        /// <summary>
        ///     英语
        /// </summary>
        English,

        /// <summary>
        ///     内码
        /// </summary>
        InnerCode,

        /// <summary>
        ///     现代二笔
        /// </summary>
        XiandaiErbi,

        /// <summary>
        ///     注音
        /// </summary>
        Zhuyin,

        /// <summary>
        ///     地球拼音
        /// </summary>
        TerraPinyin,

        /// <summary>
        /// 超音速写
        /// </summary>
        Chaoyin,

        /// <summary>
        ///     无编码
        /// </summary>
        NoCode
    }
}
