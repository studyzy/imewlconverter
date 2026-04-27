using System;
using System.IO;

namespace Studyzy.IMEWLConverter.Helpers
{
    /// <summary>
    /// Compatibility extension methods for Stream to provide ReadExactly on frameworks that lack it.
    /// If running on a runtime that already provides Stream.ReadExactly as an instance method, the instance method will be used
    /// and this extension will not be invoked.
    /// </summary>
    internal static class StreamExtensions
    {
        public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            var read = 0;
            while (read < count)
            {
                var r = stream.Read(buffer, offset + read, count - read);
                if (r == 0) throw new EndOfStreamException("Unable to read the required number of bytes from the stream.");
                read += r;
            }
        }
    }
}
