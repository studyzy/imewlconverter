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

        public static ushort ReadUInt16(Stream fs)
        {
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            ushort s = BitConverter.ToUInt16(temp, 0);
            return s;
        }

        public static uint ReadUInt32(Stream fs)
        {
            var temp = new byte[4];
            fs.Read(temp, 0, 4);
            uint s = BitConverter.ToUInt32(temp, 0);
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
