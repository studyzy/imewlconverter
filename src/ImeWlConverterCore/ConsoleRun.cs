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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter
{
    public delegate void ShowHelp(List<ComboBoxShowAttribute> cbxImportItems);

    public class ConsoleRun
    {
        private readonly List<ComboBoxShowAttribute> cbxExportItems =
            new List<ComboBoxShowAttribute>();
        private readonly List<ComboBoxShowAttribute> cbxImportItems =
            new List<ComboBoxShowAttribute>();
        private readonly IDictionary<string, IWordLibraryExport> exports =
            new Dictionary<string, IWordLibraryExport>();
        private readonly List<string> importPaths = new List<string>();
        private readonly IDictionary<string, IWordLibraryImport> imports =
            new Dictionary<string, IWordLibraryImport>();
        private readonly ParsePattern pattern = new ParsePattern();
        private bool beginImportFile;
        private string codingFile;
        private string exportPath = "";
        private string format;
        private CommandType type = CommandType.Null;
        private Encoding wordEncoding = Encoding.UTF8;
        private IWordLibraryExport wordLibraryExport;
        private IWordLibraryImport wordLibraryImport;
        private Encoding xmlEncoding;
        private ShowHelp showHelp;
        private IWordRankGenerater wordRankGenerater = new DefaultWordRankGenerater();
        private IList<ISingleFilter> filters = new List<ISingleFilter>();

        [RequiresUnreferencedCode("Calls LoadImeList()")]
        public ConsoleRun(string[] args, ShowHelp showHelp)
        {
            Args = args;
            this.showHelp = showHelp;
            pattern.ContainCode = true;
            pattern.SplitString = " ";
            pattern.CodeSplitString = ",";
            pattern.CodeSplitType = BuildType.None;
            pattern.Sort = new List<int> { 2, 1, 3 };
            pattern.ContainRank = false;
            LoadImeList();
        }

        public string[] Args { get; set; }

        public void Run()
        {
            if (Args.Length == 0)
            {
                Console.WriteLine("输入 -h 可获取帮助");
                return;
            }
            for (int i = 0; i < Args.Length; i++)
            {
                string arg = Args[i];
                type = RunCommand(arg);
            }
            if (!string.IsNullOrEmpty(format))
            {
                if (
                    (!(wordLibraryExport is SelfDefining)) && (!(wordLibraryImport is SelfDefining))
                )
                {
                    Console.WriteLine("-f参数用于自定义格式时设置格式样式用，导入导出词库格式均不是自定义格式，该参数无效！");
                    return;
                }
            }
            if (!string.IsNullOrEmpty(codingFile))
            {
                if (!(wordLibraryExport is SelfDefining))
                {
                    Console.WriteLine("-f参数用于自定义格式输出时设置编码用，导出词库格式不是自定义格式，该参数无效！");
                    return;
                }
            }
            if (wordLibraryImport is SelfDefining)
            {
                ((SelfDefining)wordLibraryImport).UserDefiningPattern = pattern;
            }
            if (wordLibraryExport is SelfDefining)
            {
                ((SelfDefining)wordLibraryExport).UserDefiningPattern = pattern;
            }
            if (wordLibraryExport is Rime)
            {
                ((Rime)wordLibraryExport).CodeType = pattern.CodeType;
                ((Rime)wordLibraryExport).OS = pattern.OS;
            }
            if (wordLibraryImport is LingoesLd2)
            {
                var ld2Import = ((LingoesLd2)wordLibraryImport);
                ld2Import.WordEncoding = wordEncoding;
                if (xmlEncoding != null)
                {
                    ld2Import.XmlEncoding = xmlEncoding;
                    ld2Import.IncludeMeaning = true;
                }
            }
            if (importPaths.Count > 0 && exportPath != "")
            {
                var mainBody = new MainBody();
                mainBody.Export = wordLibraryExport;
                mainBody.Import = wordLibraryImport;
                mainBody.SelectedWordRankGenerater = this.wordRankGenerater;
                mainBody.Filters = this.filters;
                mainBody.ProcessNotice += MainBody_ProcessNotice;
                Console.WriteLine("转换开始...");
                //foreach (string importPath in importPaths)
                //{
                //    Console.WriteLine("开始转换文件：" + importPath);
                //    wordLibraryList.AddWordLibraryList(wordLibraryImport.Import(importPath));
                //}
                //string str = wordLibraryExport.Export(wordLibraryList);
                if (exportPath.EndsWith("*"))
                {
                    mainBody.Convert(importPaths, exportPath.Substring(0, exportPath.Length - 1));
                }
                else
                {
                    string str = mainBody.Convert(importPaths);
                    FileOperationHelper.WriteFile(exportPath, wordLibraryExport.Encoding, str);
                }
                Console.WriteLine("转换完成,共转换" + mainBody.Count + "个");
            }
            Console.WriteLine("输入 -h 可获取帮助");
        }

        private void MainBody_ProcessNotice(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t" + message);
        }

        private CommandType RunCommand(string command)
        {
            if (command == "--help" || command == "-h")
            {
                showHelp(this.cbxImportItems);
                return CommandType.Help;
            }
            if (command == "--version" || command == "-v")
            {
                Console.WriteLine("Version:" + Assembly.GetExecutingAssembly().GetName().Version);
                return CommandType.Help;
            }
            if (command.StartsWith("-i:"))
            {
                wordLibraryImport = GetImportInterface(command.Substring(3));
                beginImportFile = true;
                return CommandType.Import;
            }

            if (command.StartsWith("-o:"))
            {
                wordLibraryExport = GetExportInterface(command.Substring(3));
                beginImportFile = false;
                return CommandType.Export;
            }
            if (command.StartsWith("-c:")) //code
            {
                codingFile = command.Substring(3);
                pattern.MappingTablePath = codingFile;
                pattern.IsPinyinFormat = false;
                beginImportFile = false;
                return CommandType.Coding;
            }
            if (command.StartsWith("-ft:")) //filter
            {
                var filterStrs = command.Substring(4);
                Regex lenRegex = new Regex(@"len:(\d+)-(\d+)");
                Regex rankRegex = new Regex(@"rank:(\d+)-(\d+)");
                Regex rmRegex = new Regex(@"rm:(\w+)");
                foreach (var filterStr in filterStrs.Split('|'))
                {
                    if (lenRegex.IsMatch(filterStr))
                    {
                        var match = lenRegex.Match(filterStr);
                        var from = Convert.ToInt32(match.Groups[1].Value);
                        var to = Convert.ToInt32(match.Groups[2].Value);
                        var numberFilter = new LengthFilter() { MinLength = from, MaxLength = to };
                        this.filters.Add(numberFilter);
                    }
                    else if (rankRegex.IsMatch(filterStr))
                    {
                        var match = rankRegex.Match(filterStr);
                        var from = Convert.ToInt32(match.Groups[1].Value);
                        var to = Convert.ToInt32(match.Groups[2].Value);
                        var rFilter = new RankFilter() { MinLength = from, MaxLength = to };
                        this.filters.Add(rFilter);
                    }
                    else if (rmRegex.IsMatch(filterStr))
                    {
                        var match = rmRegex.Match(filterStr);
                        var rmType = match.Groups[1].Value;
                        ISingleFilter filter;
                        switch (rmType)
                        {
                            case "eng":
                                filter = new EnglishFilter();
                                break;
                            case "num":
                                filter = new NumberFilter();
                                break;
                            case "space":
                                filter = new SpaceFilter();
                                break;
                            case "pun":
                                filter = new EnglishPunctuationFilter();
                                break;
                            default:
                                throw new ArgumentException("Unsupport filter type:" + rmType);
                        }
                        this.filters.Add(filter);
                    }
                }
                return CommandType.Coding;
            }
            if (command.StartsWith("-ct:")) //code type
            {
                var codeType = command.Substring(4).ToLower();
                switch (codeType)
                {
                    case "pinyin":
                        pattern.CodeType = CodeType.Pinyin;
                        break;
                    case "wubi":
                        pattern.CodeType = CodeType.Wubi;
                        break;
                    case "zhengma":
                        pattern.CodeType = CodeType.Zhengma;
                        break;
                    case "cangjie":
                        pattern.CodeType = CodeType.Cangjie;
                        break;
                    case "zhuyin":
                        pattern.CodeType = CodeType.TerraPinyin;
                        break;
                    default:
                        pattern.CodeType = CodeType.Pinyin;
                        break;
                }
                return CommandType.CodeType;
            }
            if (command.StartsWith("-r:")) //Rank
            {
                var rankType = command.Substring(3).ToLower();
                switch (rankType)
                {
                    case "baidu":
                        this.wordRankGenerater = new BaiduWordRankGenerater();
                        break;
                    case "google":
                        this.wordRankGenerater = new GoogleWordRankGenerater();
                        break;

                    default:

                        {
                            var rankNumber = Convert.ToInt32(rankType);
                            var gen = new DefaultWordRankGenerater();
                            gen.ForceUse = true;
                            gen.Rank = rankNumber;
                            this.wordRankGenerater = gen;
                        }
                        break;
                }
                return CommandType.CodeType;
            }
            if (command.StartsWith("-os:")) //code type
            {
                var os = command.Substring(4).ToLower();
                switch (os)
                {
                    case "windows":
                        pattern.OS = OperationSystem.Windows;
                        break;
                    case "mac":
                    case "macos":
                        pattern.OS = OperationSystem.MacOS;
                        break;
                    case "linux":
                    case "unix":
                        pattern.OS = OperationSystem.Linux;
                        break;
                    default:
                        pattern.OS = OperationSystem.Windows;
                        break;
                }
                return CommandType.OS;
            }
            if (command.StartsWith("-ld2:")) //ld2 encoding
            {
                string ecodes = command.Substring(5);
                string[] arr = ecodes.Split(',');

                wordEncoding = Encoding.GetEncoding(arr[0]);
                if (arr.Length > 1)
                {
                    xmlEncoding = Encoding.GetEncoding(arr[1]);
                }

                return CommandType.Encoding;
            }
            if (command.StartsWith("-f:")) //format
            {
                format = command.Substring(3);
                beginImportFile = false;
                var sort = new List<int>();
                for (int i = 0; i < 3; i++)
                {
                    char c = format[i];
                    sort.Add(Convert.ToInt32(c));
                }
                pattern.Sort = sort;
                pattern.CodeSplitString = format[3].ToString();
                pattern.SplitString = format[4].ToString();
                string t = format[5].ToString().ToLower();
                beginImportFile = false;
                if (t == "l")
                    pattern.CodeSplitType = BuildType.LeftContain;
                if (t == "r")
                    pattern.CodeSplitType = BuildType.RightContain;
                if (t == "b")
                    pattern.CodeSplitType = BuildType.FullContain;
                if (t == "n")
                    pattern.CodeSplitType = BuildType.None;
                pattern.ContainCode = (format[6].ToString().ToLower() == "y");
                pattern.ContainRank = (format[8].ToString().ToLower() == "y");
                return CommandType.Format;
            }

            if (beginImportFile)
            {
                importPaths.AddRange(FileOperationHelper.GetFilesPath(command));
            }
            if (type == CommandType.Export)
            {
                exportPath = command;
            }
            return CommandType.Other;
        }

        [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
        private void LoadImeList()
        {
            Assembly assembly = GetType().Assembly;
            Type[] d = assembly.GetTypes();

            foreach (Type type in d)
            {
                if (
                    type.Namespace != null
                    && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME")
                )
                {
                    object[] att = type.GetCustomAttributes(typeof(ComboBoxShowAttribute), false);
                    if (att.Length > 0)
                    {
                        var cbxa = att[0] as ComboBoxShowAttribute;
                        Debug.WriteLine(cbxa.ShortCode);
                        Debug.WriteLine(cbxa.Index);
                        if (type.GetInterface("IWordLibraryImport") != null)
                        {
                            Debug.WriteLine("Import!!!!" + type.FullName);
                            cbxImportItems.Add(cbxa);
                            imports.Add(
                                cbxa.ShortCode,
                                assembly.CreateInstance(type.FullName) as IWordLibraryImport
                            );
                        }
                        if (type.GetInterface("IWordLibraryExport") != null)
                        {
                            Debug.WriteLine("Export!!!!" + type.FullName);
                            cbxExportItems.Add(cbxa);
                            exports.Add(
                                cbxa.ShortCode,
                                assembly.CreateInstance(type.FullName) as IWordLibraryExport
                            );
                        }
                    }
                }
            }
            cbxImportItems.Sort((a, b) => a.Index - b.Index);
            cbxExportItems.Sort((a, b) => a.Index - b.Index);
        }

        private IWordLibraryExport GetExportInterface(string str)
        {
            try
            {
                return exports[str];
            }
            catch
            {
                throw new ArgumentException("导出词库的输入法错误");
            }
        }

        private IWordLibraryImport GetImportInterface(string str)
        {
            try
            {
                return imports[str];
            }
            catch
            {
                throw new ArgumentException("导入词库的输入法错误");
            }
        }

        #region Nested type: CommandType

        private enum CommandType
        {
            Import,
            Export,
            Help,
            Null,

            //编码映射文件
            Coding,

            //编码类型
            CodeType,
            Format,
            Encoding,
            OS,
            Other
        }

        #endregion
    }
}
