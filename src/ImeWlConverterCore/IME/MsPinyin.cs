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
using System.Xml;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.MS_PINYIN, ConstantString.MS_PINYIN_C, 135)]
    public class MsPinyin : BaseImport, IWordLibraryExport, IWordLibraryTextImport
    {
        #region IWordLibraryExport 成员

        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append("<ns1:DictionaryEntry>\r\n");
            sb.Append("<ns1:InputString>" + GetPinyinWithTone(wl) + "</ns1:InputString>\r\n");
            sb.Append("<ns1:OutputString>" + wl.Word + "</ns1:OutputString>\r\n");
            sb.Append("<ns1:Exist>1</ns1:Exist>\r\n");
            sb.Append("</ns1:DictionaryEntry>");

            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            sb.Append(
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n<ns1:Dictionary xmlns:ns1=\"http://www.microsoft.com/ime/dctx\">"
            );
            sb.Append(
                @"<ns1:DictionaryHeader>
    <ns1:DictionaryGUID>{"
                    + Guid.NewGuid()
                    + @"}</ns1:DictionaryGUID>
    <ns1:DictionaryLanguage>zh-cn</ns1:DictionaryLanguage>
    <ns1:FormatVersion>0</ns1:FormatVersion>
    <ns1:DictionaryVersion>1</ns1:DictionaryVersion>
    <ns1:DictionaryInfo Language=""zh-cn"">
      <ns1:ShortName>深蓝词库</ns1:ShortName>
      <ns1:LongName>深蓝词库转换而成</ns1:LongName>
      <ns1:Description>Dictionary for IME</ns1:Description>
      <ns1:Copyright>深蓝词库转换</ns1:Copyright>
      <ns1:CommentHeader1>CommentTitle1</ns1:CommentHeader1>
      <ns1:CommentHeader2>CommentTitle1</ns1:CommentHeader2>
      <ns1:CommentHeader3>CommentTitle1</ns1:CommentHeader3>
    </ns1:DictionaryInfo>
    <ns1:DictionaryInfo Language=""en-us"">
      <ns1:ShortName>Shenlan</ns1:ShortName>
      <ns1:LongName>Shenlan</ns1:LongName>
      <ns1:Description>Shenlan</ns1:Description>
      <ns1:Copyright>Shenlan</ns1:Copyright>
      <ns1:CommentHeader1>CommentTitle1</ns1:CommentHeader1>
      <ns1:CommentHeader2>CommentTitle1</ns1:CommentHeader2>
      <ns1:CommentHeader3>CommentTitle1</ns1:CommentHeader3>
    </ns1:DictionaryInfo>
    <ns1:ContentCategory>Genral</ns1:ContentCategory>
    <ns1:DictionaryType>Conversion</ns1:DictionaryType>
    <ns1:SourceURL>
    </ns1:SourceURL>
    <ns1:CommentInsertion>true</ns1:CommentInsertion>
    <ns1:IconID>25</ns1:IconID>
  </ns1:DictionaryHeader>
"
            );
            for (int i = 0; i < wlList.Count; i++)
            {
                try
                {
                    sb.Append(ExportLine(wlList[i]));
                    sb.Append("\r\n");
                }
                catch { }
            }
            sb.Append("</ns1:Dictionary>");
            return new List<string>() { sb.ToString() };
        }

        private string GetPinyinWithTone(WordLibrary wl)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wl.Word.Length; i++)
            {
                char c = wl.Word[i];
                string py = wl.PinYin[i];
                string pinyin = PinyinHelper.AddToneToPinyin(c, py);
                if (pinyin == null)
                {
                    throw new Exception("找不到字[" + c + "]的拼音");
                }
                sb.Append(pinyin);
                if (i != wl.Word.Length - 1)
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        #endregion

        #region IWordLibraryImport 成员

        public WordLibraryList Import(string path)
        {
            string str = FileOperationHelper.ReadFile(path, Encoding);
            return ImportText(str);
        }

        public WordLibraryList ImportText(string str)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(str);
            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("ns1", "http://www.microsoft.com/ime/dctx");
            var wlList = new WordLibraryList();
            XmlNodeList xns = xmlDoc.SelectNodes(
                "//ns1:Dictionary/ns1:DictionaryEntry",
                namespaceManager
            );
            CountWord = xns.Count;
            for (int i = 0; i < xns.Count; i++)
            {
                XmlNode xn = xns[i];
                string py = xn.SelectSingleNode("ns1:InputString", namespaceManager).InnerText;
                string word = xn.SelectSingleNode("ns1:OutputString", namespaceManager).InnerText;
                var wl = new WordLibrary();
                wl.Word = word;
                wl.Rank = 1;
                wl.PinYin = py.Split(
                    new[] { ' ', '1', '2', '3', '4' },
                    StringSplitOptions.RemoveEmptyEntries
                );
                CurrentStatus = i;
                wlList.Add(wl);
            }

            return wlList;
        }

        public WordLibraryList ImportLine(string line)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
