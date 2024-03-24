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
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    public class BaseImport
    {
        public BaseImport()
        {
            DefaultRank = 0;
            CodeType = CodeType.Pinyin;
        }

        public event Action<string> ImportLineErrorNotice;

        /// <summary>
        /// 输入法编码类型
        /// </summary>
        public virtual CodeType CodeType { get; set; }

        /// <summary>
        /// 默认词频，主要用于词频丢失的情况下生成词频
        /// </summary>
        public virtual int DefaultRank { get; set; }

        /// <summary>
        /// 词条总数
        /// </summary>
        public virtual int CountWord { get; set; }

        /// <summary>
        /// 当前处理了多少条
        /// </summary>
        public virtual int CurrentStatus { get; set; }

        //public virtual Form ImportConfigForm { get; private set; }
        /// <summary>
        /// 该输入法词库是不是文本词库
        /// </summary>
        public virtual bool IsText
        {
            get { return true; }
        }

        protected void SendImportLineErrorNotice(string msg)
        {
            ImportLineErrorNotice?.Invoke(msg);
        }

        public event Action<string> ExportErrorNotice;

        protected void SendExportErrorNotice(string msg)
        {
            ExportErrorNotice?.Invoke(msg);
        }
    }
}
