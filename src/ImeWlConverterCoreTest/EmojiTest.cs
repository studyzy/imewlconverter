using NUnit.Framework;
using Studyzy.IMEWLConverter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImeWlConverterCoreTest
{
    class EmojiTest
    {
        [Test]
        public void TestGenerateEmojiFile()
        {
            //var sw = FileOperationHelper.WriteFile("emoji.txt", Encoding.Unicode);

            var b = Encoding.UTF8.GetBytes("😀");
            Console.WriteLine("Hex:{0:x}", b);
        }
    }
}
