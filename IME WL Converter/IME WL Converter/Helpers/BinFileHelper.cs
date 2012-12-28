using System;
using System.IO;

namespace Studyzy.IMEWLConverter.Helpers
{
    internal static class BinFileHelper
    {
        public static short ReadInt16(Stream fs)
        {
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            short s = BitConverter.ToInt16(temp, 0);
            return s;
        }

        public static int ReadInt32(Stream fs)
        {
            var temp = new byte[4];
            fs.Read(temp, 0, 4);
            int s = BitConverter.ToInt32(temp, 0);
            return s;
        }

        public static long ReadInt64(Stream fs)
        {
            var temp = new byte[8];
            fs.Read(temp, 0, 8);
            long s = BitConverter.ToInt64(temp, 0);
            return s;
        }

        public static byte[] ReadArray(Stream fs, int count)
        {
            var bytes = new byte[count];
            fs.Read(bytes, 0, count);
            return bytes;
        }

        public static byte[] ReadArray(byte[] fs, int position, int count)
        {
            var bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = fs[position + i];
            }

            return bytes;
        }
    }
}