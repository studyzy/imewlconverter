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
using System.Linq;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters;

public class PinyinGenerater : BaseCodeGenerater, IWordCodeGenerater
{
    private static Dictionary<string, List<string>> mutiPinYinWord;

    private void InitMutiPinYinWord()
    {
        if (mutiPinYinWord == null)
        {
            var wlList = new Dictionary<string, List<string>>();
            var lines = GetMutiPinyin()
                .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Split(' ');
                if (line.Length < 2) continue;

                var py = line[0];
                var word = line[1];

                var pinyin = new List<string>(
                    py.Split(new[] { '\'' }, StringSplitOptions.RemoveEmptyEntries)
                );
                wlList.TryAdd(word, pinyin);
            }

            mutiPinYinWord = wlList;
        }
    }

    private string GetMutiPinyin()
    {
        return DictionaryHelper.GetResourceContent("WordPinyin.txt");
    }

    /// <summary>
    ///     一个词中是否有多音字注音
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private bool IsInWordPinYin(string word)
    {
        InitMutiPinYinWord();
        foreach (var key in mutiPinYinWord.Keys)
            if (word.Contains(key))
                return true;

        return false;
    }

    /// <summary>
    ///     产生一个词中多音字的拼音,没有的就空着
    ///     使用贪婪匹配算法,优先匹配较长的词组,避免重复标注
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private List<string> GenerateMutiWordPinYin(string word)
    {
        InitMutiPinYinWord();
        var pinyin = new string[word.Length];
        var matched = new bool[word.Length]; // Track which positions have been matched

        // Sort keys by length (descending) to match longer words first
        var sortedKeys = mutiPinYinWord.Keys.OrderByDescending(k => k.Length).ToList();

        foreach (var key in sortedKeys)
        {
            // Find all occurrences of this key in the word
            var index = 0;
            while ((index = word.IndexOf(key, index)) != -1)
            {
                // Check if any position in this range has already been matched
                var canMatch = true;
                for (var i = 0; i < key.Length; i++)
                {
                    if (matched[index + i])
                    {
                        canMatch = false;
                        break;
                    }
                }

                // If no overlap, apply the match
                if (canMatch)
                {
                    for (var i = 0; i < mutiPinYinWord[key].Count; i++)
                    {
                        pinyin[index + i] = mutiPinYinWord[key][i];
                        matched[index + i] = true;
                    }
                }

                index++; // Move to next position to find other occurrences
            }
        }

        return new List<string>(pinyin);
    }

    #region IWordCodeGenerater Members

    public override void GetCodeOfWordLibrary(WordLibrary wl)
    {
        if (wl.CodeType == CodeType.Pinyin) return;
        if (wl.CodeType == CodeType.TerraPinyin) //要去掉音调
        {
            for (var i = 0; i < wl.Codes.Count; i++)
            {
                var row = wl.Codes[i];
                for (var j = 0; j < row.Count; j++)
                {
                    var s = row[j];
                    var py = s.Remove(s.Length - 1); //remove tone
                    wl.Codes[i][j] = py;
                }
            }

            return;
        }

        //不是拼音，就调用GetCode生成拼音
        var code = GetCodeOfString(wl.Word);
        wl.Codes = code;
        wl.CodeType = CodeType.Pinyin;
    }

    /// <summary>
    ///     获得一个词的拼音
    ///     如果这个词不包含多音字，那么直接使用其拼音
    ///     如果包含多音字，则找对应的注音词，根据注音词进行注音
    ///     没有找到注音词的，使用默认拼音
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public override Code GetCodeOfString(string str)
    {
        if (IsInWordPinYin(str))
        {
            var pyList = GenerateMutiWordPinYin(str);
            for (var i = 0; i < str.Length; i++)
                if (pyList[i] == null)
                    pyList[i] = PinyinHelper.GetDefaultPinyin(str[i]);

            return new Code(pyList, true);
        }

        try
        {
            return new Code(PinyinHelper.GetDefaultPinyin(str), true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

    public virtual IList<string> GetAllCodesOfChar(char str)
    {
        return PinyinHelper.GetPinYinOfChar(str);
    }

    /// <summary>
    ///     因为使用了注音的方式，所以避免了多音字，一个词也只有一个音
    /// </summary>
    public virtual bool Is1CharMutiCode => false;

    public virtual bool Is1Char1Code => true;

    #endregion
}
