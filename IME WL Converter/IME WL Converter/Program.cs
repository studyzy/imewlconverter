using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    internal static class Program
    {

        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            PlatformID platform = Environment.OSVersion.Platform;
            if (platform == PlatformID.Unix || platform == PlatformID.MacOSX)
            {
                var consoleRun = new ConsoleRun(args);
                consoleRun.Run();
                return;
            }
#if DEBUG

            //Application.Run(new SelfDefiningConfigForm());
            //return;
#endif
            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                var consoleRun = new ConsoleRun(args);
                consoleRun.Run();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}