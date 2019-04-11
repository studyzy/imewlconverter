using System;


namespace Studyzy.IMEWLConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //var cj = Helpers.DictionaryHelper.GetResourceContent("Cangjie5.txt");
            //Console.WriteLine(cj);
              var consoleRun = new ConsoleRun(args);
                consoleRun.Run();
        }
    }
}
