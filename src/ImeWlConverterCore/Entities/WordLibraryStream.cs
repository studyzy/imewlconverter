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

namespace Studyzy.IMEWLConverter.Entities;

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
        this.import = import ?? throw new ArgumentNullException(nameof(import));
        this.export = export ?? throw new ArgumentNullException(nameof(export));
        this.sw = sw ?? throw new ArgumentNullException(nameof(sw));
        this.path = path ?? throw new ArgumentNullException(nameof(path));
        this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public int Count => 0;

    public void ConvertWordLibrary(Predicate<WordLibrary> match)
    {
        var i = 0;
        using (var sr = new StreamReader(path, encoding))
        {
            try
            {
                if (import is IStreamPrepare pImport)
                {
                    pImport.Prepare();
                }

                if (export is IStreamPrepare pExport)
                {
                    pExport.Prepare();
                }

                while (sr.Peek() != -1)
                {
                    var line = sr.ReadLine();
                    if (line == null) break;
                    var wll = import.ImportLine(line);
                    import.CurrentStatus = i++;
                    if (wll == null) continue;
                    foreach (var wl in wll)
                        if (wl != null && match(wl))
                            sw.WriteLine(export.ExportLine(wl));
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
