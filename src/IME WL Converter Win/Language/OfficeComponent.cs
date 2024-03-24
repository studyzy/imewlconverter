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
using System.Reflection;
using Microsoft.Office.Interop.Word;

namespace Studyzy.IMEWLConverter.Language
{
    internal class OfficeComponent : IChineseConverter, IDisposable
    {
        #region IChineseConverter Members

        public string ToChs(string cht)
        {
            var doc = new Document();
            doc.Content.Text = cht;
            doc.Content.TCSCConverter(
                WdTCSCConverterDirection.wdTCSCConverterDirectionTCSC,
                true,
                true
            );
            string des = doc.Content.Text;
            object saveChanges = false;
            object originalFormat = Missing.Value;
            object routeDocument = Missing.Value;
            doc.Close(ref saveChanges, ref originalFormat, ref routeDocument);
            GC.Collect();
            return des;
        }

        public string ToCht(string chs)
        {
            var doc = new Document();
            doc.Content.Text = chs;
            doc.Content.TCSCConverter(
                WdTCSCConverterDirection.wdTCSCConverterDirectionSCTC,
                true,
                true
            );
            string des = doc.Content.Text;
            object saveChanges = false;
            object originalFormat = Missing.Value;
            object routeDocument = Missing.Value;
            doc.Close(ref saveChanges, ref originalFormat, ref routeDocument);
            GC.Collect();
            return des;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //object saveChange = 0;
            //object originalFormat = Missing.Value;
            //object routeDocument = Missing.Value;
            //appWord.Quit(ref saveChange, ref originalFormat, ref routeDocument);
            //doc = null;
            //appWord = null;
            GC.Collect(); //进程资源释放
        }

        #endregion

        //public void Init()
        //{
        //    //appWord = new Microsoft.Office.Interop.Word.Application();
        //    object template = Missing.Value;
        //    object newTemplate = Missing.Value;
        //    object docType = Missing.Value;
        //    object visible = true;
        //    //doc = appWord.Documents.Add(ref template, ref newTemplate, ref docType, ref visible);
        //    doc=new Document();
        //}

        //private Document doc;
        //private _Application appWord;
    }
}
