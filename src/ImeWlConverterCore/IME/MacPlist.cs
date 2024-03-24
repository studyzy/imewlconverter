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
using System.Text;
using System.Xml;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.MAC_PLIST, ConstantString.MAC_PLIST_C, 150)]
    public class MacPlist : BaseImport, IWordLibraryTextImport, IWordLibraryExport
    {
        private string Header =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"><plist version=\"1.0\"><array>";
        private string Footer = "</array></plist>";

        #region IWordLibraryExport 成员

        public string ExportLine(WordLibrary wl)
        {
            var format =
                "<dict><key>phrase</key><string>{0}</string><key>shortcut</key><string>{1}</string></dict>";
            try
            {
                string py = wl.GetPinYinString("", BuildType.None);
                if (string.IsNullOrEmpty(py))
                {
                    return "";
                }
                return string.Format(format, wl.Word, py);
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
