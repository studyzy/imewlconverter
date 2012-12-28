using System;
using System.Reflection;
using Microsoft.Office.Interop.Word;

namespace Studyzy.IMEWLConverter.Language
{
    internal class OfficeComponent : IChineseConverter, IDisposable
    {
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

        #region IChineseConverter Members

        public string ToChs(string cht)
        {
            var doc = new Document();
            doc.Content.Text = cht;
            doc.Content.TCSCConverter(WdTCSCConverterDirection.wdTCSCConverterDirectionTCSC, true, true);
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
            doc.Content.TCSCConverter(WdTCSCConverterDirection.wdTCSCConverterDirectionSCTC, true, true);
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
    }
}