using NUnit.Framework;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImeWlConverterCoreTest
{
    public class Win10MsPinyinSelfStudyTest
    {
        [Test]
        public void TestExport1()
        {
            WordLibraryList wl = new WordLibraryList();
            var wl1 = new WordLibrary() { Word="曾毅曾诚",PinYin= new string[] { "zeng", "yi", "zeng", "cheng" },CodeType=CodeType.Pinyin};
            wl.Add(wl1);
          
            var export = new Win10MsPinyinSelfStudy();
            export.ExportFilePath = "c:\\Temp\\win10selfstudy5.dat";
            var filePath =export.Export(wl);
            Debug.WriteLine(filePath[0]);
        }
    }
}
