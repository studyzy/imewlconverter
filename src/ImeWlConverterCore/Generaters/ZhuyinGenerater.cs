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

using System.Collections.Generic;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    /*
     * 声母：
ㄅ	b
ㄆ	p
ㄇ	m
ㄈ	f
ㄉ	d
ㄊ	t
ㄋ	n
ㄌ	l
ㄍ	g
ㄎ	k
ㄏ	h
ㄐ	j
ㄑ	q
ㄒ	x
ㄓ	zhi / zh
ㄔ	chi / ch
ㄕ	shi / sh
ㄖ	ri / r
ㄗ	zi /z
ㄘ	ci /c
ㄙ	si / s
    韵尾：
ㄚ	a
ㄛ	o
ㄜ	e
ㄝ	ê
ㄞ	ai
ㄟ	ei
ㄠ	ao
ㄡ	ou
ㄢ	an
ㄣ	en
ㄤ	ang
ㄥ	eng
ㄦ	er
ㄧ	yi / i
ㄧ	yin /in
ㄧ	ying / ing
ㄨ	wu / u
ㄨ	wen / un
ㄨ	weng / ong
ㄩ	yu / u / ü
ㄩ	yun / un
ㄩ	yong / iong
     *
    注音符号和汉语拼音大致能直接转换，但部分例子略有差异，其重点如下：

    “ㄜ”对应于汉语拼音中的e，而“ㄝ”则对应于汉语拼音中的ê。可是除了独用外，其他情况ê都改写成“e”，所以使辨识上不及注音符号的“ㄝ”来得容易。在汉语拼音的写法中，“ㄝ”永远是复韵母的最后一个，这点可以用来根据汉语拼音辨别两者。另一个特殊情况是“ㄦ”汉语拼音写作“er”，但作韵尾时仅写“r”。
    当既可作声母又可作韵母的“丨”、“ㄨ”不作声母时，“ㄨㄥ”在汉语拼音中写作“ong”，“丨ㄡ”写作“iu”，“ㄨㄟ”写作“ui”，“ㄩㄥ”写作“iong”，“丨ㄥ”写作“ing”，“ㄨㄣ”写作“un”。
    单成音节的“丨”、“ㄨ”、“ㄩ”分别写作“yi”、“wu”、“yu”。“ㄩ”在很多情况下写成“u”，详见汉语拼音方案。
    在注音符号，“ㄓㄔㄕㄖㄗㄘㄙ”等声母可以单独成音节。但汉语拼音必须添加韵母“i”方可组成音节。故这些音节在汉语拼音中写作“zhi chi shi ri zi ci si”。附注：zhi 在旧式注音需加空韵“ㄭ”注成“ㄓㄭ”，新式中则不需，注成“ㄓ”，故汉语拼音以类似旧式的拼法，zh 要加空韵 i。
    Mapping参见：http://home.ust.hk/~lbxwang/cats/guoyuzhuyin.html
     */

    /// <summary>
    ///     注音是在地球拼音的基础上，通过注音Mapping表，转化为注音符号
    /// </summary>
    public class ZhuyinGenerater : TerraPinyinGenerater
    {
        public override IList<string> GetAllCodesOfChar(char str)
        {
            var result = new List<string>();
            foreach (string code in base.GetAllCodesOfChar(str))
            {
                result.Add(ZhuyinHelper.GetZhuyin(code));
            }
            return result;
        }

        public override Code GetCodeOfString(string str)
        {
            var result = new List<IList<string>>();

            foreach (var row in base.GetCodeOfString(str))
            {
                var zyrow = new List<string>();
                foreach (var py in row)
                {
                    zyrow.Add(ZhuyinHelper.GetZhuyin(py));
                }

                result.Add(zyrow);
            }
            return new Code(result);
        }

        //public override void GetCodeOfWordLibrary(WordLibrary wl)
        //{
        //    base.GetCodeOfWordLibrary(wl);
        //    for (int i = 0; i < wl.Codes.Count; i++)
        //    {
        //        var row = wl.Codes[i];
        //        for (int j = 0; j < row.Count; j++)
        //        {
        //            string s = row[j];
        //            string zy = ZhuyinHelper.GetZhuyin(s);
        //            wl.Codes[i][j] = zy;
        //        }
        //    }
        //}
    }
}
