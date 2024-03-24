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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Studyzy.IMEWLConverter.Entities
{
    public class WordLibraryStream
    {
        private readonly Encoding encoding;
        private readonly IWordLibraryExport export;
        private readonly IWordLibraryImport import;

        private readonly string path;
        private readonly StreamWriter sw;

        public WordLibraryStream(
            IWordLibraryImport import,
            IWordLibraryExport export,
            string path,
            Encoding encoding,
            StreamWriter sw
        )
        {
            this.import = import;
            this.export = export;
            this.sw = sw;
            this.path = path;
            this.encoding = encoding;
        }

        public int Count
        {
            get { return 0; }
        }

        public void ConvertWordLibrary(Predicate<WordLibrary> match)
        {
            int i = 0;
            using (var sr = new StreamReader(path, encoding))
            {
                try
                {
                    if (import is IStreamPrepare)
                    {
                        var p = import as IStreamPrepare;
                        p.Prepare();
                    }
                    if (export is IStreamPrepare)
                    {
                        var p = export as IStreamPrepare;
                        p.Prepare();
                    }
                    while (sr.Peek() != -1)
                    {
                        string line = sr.ReadLine();
                        WordLibraryList wll = import.ImportLine(line);
                        import.CurrentStatus = i++;
                        foreach (WordLibrary wl in wll)
                        {
                            if (wl != null && match(wl))
                            {
                                sw.WriteLine(export.ExportLine(wl));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw;
#endif
                }
            }
        }
    }
}
