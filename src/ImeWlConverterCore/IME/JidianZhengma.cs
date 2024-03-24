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

using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     极点的词库格式为“编码 词语 词语 词语”\r\n
    /// </summary>
    [ComboBoxShow(ConstantString.JIDIAN_ZHENGMA, ConstantString.JIDIAN_ZHENGMA_C, 190)]
    public class JidianZhengma : Jidian, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        //private readonly IWordCodeGenerater factory = new ZhengmaGenerater();

        //public override string ExportLine(WordLibrary wl)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append(wl.SingleCode);
        //    //sb.Append(factory.GetCodeOfString(wl.Word)[0]);
        //    sb.Append(" ");
        //    sb.Append(wl.Word);

        //    return sb.ToString();
        //}

        public override CodeType CodeType
        {
            get { return CodeType.Zhengma; }
        }

        #endregion
    }
}
