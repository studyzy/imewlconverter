using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.MAC_PLIST, ConstantString.MAC_PLIST_C, 150)]
    public class MacPlist : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        private string Header="<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"><plist version=\"1.0\"><array>";
        private string Footer="</array></plist>";
        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var format="<dict><key>phrase</key><string>{0}</string><key>shortcut</key><string>{1}</string></dict>";
            try
            {
                string py = wl.GetPinYinString("", BuildType.None);
                if (string.IsNullOrEmpty(py))
                {
                    return "";
                }
                return string.Format(format,wl.Word,py);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return "";
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            if (wlList.Count == 0)
            {
                return new List<string>();
            }
            var sb = new StringBuilder();
            sb.Append(Header);
            for (int i = 0; i < wlList.Count; i++)
            {
                string line = ExportLine(wlList[i]);
                if (line != "")
                {
                    sb.Append(line);
                }
            }
            sb.Append(Footer);
            return new List<string>() { sb.ToString() };
        }

        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion

//<dict><key>phrase</key><string>马上到！</string><key>shortcut</key><string>msd</string></dict>

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        public WordLibraryList ImportText(string str)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);
           
            var wlList = new WordLibraryList();
            XmlNodeList xns = xmlDoc.SelectNodes("//plist/array/dict");
            CountWord = xns.Count;
            for (int i = 0; i < xns.Count; i++)
            {
                XmlNode xn = xns[i];
                var nodes = xn.SelectNodes("string");
                
                var wl = new WordLibrary();
                wl.Word = nodes[0].InnerText;
                wl.Rank = 1;
                wl.SetPinyinString(nodes[1].InnerText);
                CurrentStatus = i;
                wlList.Add(wl);
            }

            return wlList;
        }


        public WordLibraryList ImportLine(string line)
        {
            throw new NotImplementedException();
        }
    }
}