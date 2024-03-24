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
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     用户自定义的短语
    /// </summary>
    [ComboBoxShow(ConstantString.USER_PHRASE, ConstantString.USER_PHRASE_C, 110)]
    public class UserDefinePhrase : BaseImport, IWordLibraryExport //, IWordLibraryTextImport
    {
        public UserDefinePhrase()
        {
            PhraseFormat = "{1},{2}={0}"; //默认搜狗自定义短语的格式
            DefaultRank = 1;
        }

        /// <summary>
        ///     短语的格式{0}是短语{1}是编码{2}是排列的位置
        /// </summary>
        public string PhraseFormat { get; set; }

        public override CodeType CodeType { get; set; }

        /// <summary>
        /// 拼音编码时，是否只使用拼音首字母
        /// </summary>
        public bool IsShortCode { get; set; }

        public Encoding Encoding => Encoding.UTF8;

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            foreach (WordLibrary wordLibrary in wlList)
            {
                sb.Append(ExportLine(wordLibrary));
                sb.Append("\r\n");
            }
            return new List<string>() { sb.ToString() };
        }

        public string ExportLine(WordLibrary wl)
        {
            var codes = wl.Codes;
            if (IsShortCode)
            {
                codes = new Code();
                foreach (var c in wl.Codes)
                {
                    codes.Add(new List<string>() { c[0][0].ToString() });
                }
            }
            return string.Format(
                PhraseFormat,
                wl.Word,
                CollectionHelper.Descartes(codes)[0],
                wl.Rank == 0 ? DefaultRank : wl.Rank
            );
        }

        public WordLibraryList ImportText(string text)
        {
            throw new NotImplementedException();
        }

        public WordLibraryList Import(string path)
        {
            throw new NotImplementedException();
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException();
        }
    }
}
