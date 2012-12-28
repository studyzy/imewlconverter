using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter
{
    internal class ConsoleRun
    {
        private readonly List<ComboBoxShowAttribute> cbxExportItems = new List<ComboBoxShowAttribute>();
        private readonly List<ComboBoxShowAttribute> cbxImportItems = new List<ComboBoxShowAttribute>();
        private readonly IDictionary<string, IWordLibraryExport> exports = new Dictionary<string, IWordLibraryExport>();
        private readonly List<string> importPaths = new List<string>();
        private readonly IDictionary<string, IWordLibraryImport> imports = new Dictionary<string, IWordLibraryImport>();
        private readonly ParsePattern pattern = new ParsePattern();
        private bool beginImportFile;
        private string codingFile;
        private string exportPath = "";
        private string format;
        private CommandType type = CommandType.Null;
        private IWordLibraryExport wordLibraryExport;
        private IWordLibraryImport wordLibraryImport;

        public ConsoleRun(string[] args)
        {
            Args = args;
            pattern.ContainPinyin = true;
            pattern.SplitString = " ";
            pattern.PinyinSplitString = ",";
            pattern.PinyinSplitType = BuildType.None;
            pattern.Sort = new List<int> {2, 1, 3};
            pattern.ContainCipin = false;
            LoadImeList();
        }

        public string[] Args { get; set; }

        public void Run()
        {
            for (int i = 0; i < Args.Length; i++)
            {
                string arg = Args[i];
                type = RunCommand(arg);
            }
            if (!string.IsNullOrEmpty(format))
            {
                if ((!(wordLibraryExport is SelfDefining)) && (!(wordLibraryImport is SelfDefining)))
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
                ((SelfDefining) wordLibraryImport).UserDefiningPattern = pattern;
            }
            if (wordLibraryExport is SelfDefining)
            {
                ((SelfDefining) wordLibraryExport).UserDefiningPattern = pattern;
            }
            if (importPaths.Count > 0 && exportPath != "")
            {
                MainBody mainBody=new MainBody();
                mainBody.Export = wordLibraryExport;
                mainBody.Import = wordLibraryImport;
                
                Console.WriteLine("转换开始...");
                //foreach (string importPath in importPaths)
                //{
                //    Console.WriteLine("开始转换文件：" + importPath);
                //    wordLibraryList.AddWordLibraryList(wordLibraryImport.Import(importPath));
                //}
                //string str = wordLibraryExport.Export(wordLibraryList);
                string str = mainBody.Convert(importPaths);
                FileOperationHelper.WriteFile(exportPath, wordLibraryExport.Encoding, str);
                Console.WriteLine("转换完成,共转换" + mainBody.Count + "个");
                return;
            }
            else
            {
                Console.WriteLine("输入 -? 可获取帮助");
            }
        }

        private CommandType RunCommand(string command)
        {
            if (command == "-help" || command == "-?")
            {
                Help();
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
                UserCodingHelper.FilePath = codingFile;
                pattern.Factory = new SelfDefiningCodeGenerater();
                beginImportFile = false;
                return CommandType.Coding;
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
                pattern.PinyinSplitString = format[3].ToString();
                pattern.SplitString = format[4].ToString();
                string t = format[5].ToString().ToLower();
                beginImportFile = false;
                if (t == "l") pattern.PinyinSplitType = BuildType.LeftContain;
                if (t == "r") pattern.PinyinSplitType = BuildType.RightContain;
                if (t == "b") pattern.PinyinSplitType = BuildType.FullContain;
                if (t == "n") pattern.PinyinSplitType = BuildType.None;
                pattern.ContainPinyin = (format[6].ToString().ToLower() == "y");
                pattern.ContainCipin = (format[8].ToString().ToLower() == "y");
                return CommandType.Format;
            }
           
            if (beginImportFile)
            {
                importPaths.Add(command);
            }
            if (type == CommandType.Export)
            {
                exportPath = command;
            }
            return CommandType.Other;
        }

        private void LoadImeList()
        {
            Assembly assembly = GetType().Assembly;
            Type[] d = assembly.GetTypes();


            foreach (Type type in d)
            {
                if (type.Namespace != null && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME"))
                {
                    object[] att = type.GetCustomAttributes(typeof (ComboBoxShowAttribute), false);
                    if (att.Length > 0)
                    {
                        var cbxa = att[0] as ComboBoxShowAttribute;
                        Debug.WriteLine(cbxa.ShortCode);
                        Debug.WriteLine(cbxa.Index);
                        if (type.GetInterface("IWordLibraryImport") != null)
                        {
                            Debug.WriteLine("Import!!!!" + type.FullName);
                            cbxImportItems.Add(cbxa);
                            imports.Add(cbxa.ShortCode, assembly.CreateInstance(type.FullName) as IWordLibraryImport);
                        }
                        if (type.GetInterface("IWordLibraryExport") != null)
                        {
                            Debug.WriteLine("Export!!!!" + type.FullName);
                            cbxExportItems.Add(cbxa);
                            exports.Add(cbxa.ShortCode, assembly.CreateInstance(type.FullName) as IWordLibraryExport);
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

        private void Help()
        {
            Console.WriteLine("-i:输入的词库类型 词库路径1 词库路径2 词库路径3 -o:输出的词库类型 输出词库路径 -c:编码文件路径");
            Console.WriteLine("输入和输出的词库类型如下：");
            ConsoleColour.SetForeGroundColour(ConsoleColour.ForeGroundColour.Green);

            foreach (ComboBoxShowAttribute comboBoxShowAttribute in cbxImportItems)
            {
                Console.WriteLine(comboBoxShowAttribute.ShortCode + "\t" + comboBoxShowAttribute.Name);
            }
           
            Console.WriteLine("");
            ConsoleColour.SetForeGroundColour(ConsoleColour.ForeGroundColour.White);
            Console.WriteLine("例如要将C:\\test.scel和C:\\a.scel的搜狗细胞词库转换为D:\\gg.txt的谷歌拼音词库，命令为：");
            ConsoleColour.SetForeGroundColour(ConsoleColour.ForeGroundColour.Blue);
            Console.WriteLine("深蓝词库转换.exe -i:" + ConstantString.SOUGOU_XIBAO_SCEL_C + " C:\\test.scel C:\\a.scel -o:" +
                              ConstantString.GOOGLE_PINYIN_C + " D:\\gg.txt");
            ConsoleColour.SetForeGroundColour(ConsoleColour.ForeGroundColour.White);
            Console.WriteLine("自定义格式的参数如下:");
            Console.WriteLine("-f:213,|byyn");
            Console.WriteLine("213 这里是设置拼音、汉字和词频的顺序，213表示1汉字2拼音3词频，必须要有3个");
            Console.WriteLine(", 这里是设置拼音之间的分隔符，用逗号分割");
            Console.WriteLine("| 这里是设置汉字拼音词频之间的分隔符，用|分割");
            Console.WriteLine("b 这里是设置拼音分隔符的位置，有lrbn四个选项，l表示左包含，r表示右包含，b表示两边都包含，n表示两边都不包含");
            Console.WriteLine("yyn 这里是设置拼音汉字词频这3个是否显示，y表示显示，b表示不显示，这里yyn表示显示拼音和汉字，不显示词频");
            Console.WriteLine("例如要将一个qpyd词库转换为自定义格式的文本词库，拼音之间逗号分割，拼音和词之间空格分割，不显示词频，同时使用自定义的编码文件code.txt命令如下：");
            ConsoleColour.SetForeGroundColour(ConsoleColour.ForeGroundColour.Blue);
            Console.WriteLine("深蓝词库转换.exe -i:qpyd D:\\a.qpyd -o:self D:\\zy.txt \"-f:213, nyyn\" -c:D:\\code.txt");
            ConsoleColour.SetForeGroundColour(ConsoleColour.ForeGroundColour.White);
        }

        #region Nested type: CommandType

        private enum CommandType
        {
            Import,
            Export,
            Help,
            Null,
            Coding,
            Format,
            Other
        }

        #endregion
    }

    /// <summary>
    /// Static class for console colour manipulation.
    /// </summary>
    internal class ConsoleColour
    {
        // constants for console streams

        #region ForeGroundColour enum

        [Flags]
        public enum ForeGroundColour
        {
            Black = 0x0000,
            Blue = 0x0001,
            Green = 0x0002,
            Cyan = 0x0003,
            Red = 0x0004,
            Magenta = 0x0005,
            Yellow = 0x0006,
            Grey = 0x0007,
            White = 0x008
        }

        #endregion

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        private ConsoleColour()
        {
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetStdHandle
            (
            int nStdHandle // input, output, or error device
            );

        [DllImport("Kernel32.dll")]
        private static extern bool SetConsoleTextAttribute
            (
            IntPtr hConsoleOutput, // handle to screen buffer
            int wAttributes // text and background colors
            );

        // class can not be created, so we can set colours
        // without a variable

        public static bool SetForeGroundColour()
        {
            // default to a white-grey
            return SetForeGroundColour(ForeGroundColour.Grey);
        }

        public static bool SetForeGroundColour(
            ForeGroundColour foreGroundColour)
        {
            // default to a bright white-grey
            return SetForeGroundColour(foreGroundColour, true);
        }

        public static bool SetForeGroundColour(
            ForeGroundColour foreGroundColour,
            bool brightColours)
        {
            // get the current console handle
            IntPtr nConsole = GetStdHandle(STD_OUTPUT_HANDLE);
            int colourMap;
            // if we want bright colours OR it with white
            if (brightColours)
                colourMap = (int) foreGroundColour |
                            (int) ForeGroundColour.White;
            else
                colourMap = (int) foreGroundColour;
            // call the api and return the result
            return SetConsoleTextAttribute(nConsole, colourMap);
        }

        // colours that can be set
    }
}