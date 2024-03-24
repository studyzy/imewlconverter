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

using System.Runtime.InteropServices;
using System.Text;

namespace Studyzy.IMEWLConverter.Language
{
    public class SystemKernel : IChineseConverter
    {
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        #region IChineseConverter Members

        public string ToChs(string cht)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding gb2312 = Encoding.GetEncoding(936);
            byte[] src = gb2312.GetBytes(cht);
            var dest = new byte[src.Length];
            LCMapString(0x0804, LCMAP_SIMPLIFIED_CHINESE, src, -1, dest, src.Length);

            //LCMapString(0x0804, LCMAP_TRADITIONAL_CHINESE, src, -1, dest, src.Length);
            return gb2312.GetString(dest);
        }

        public string ToCht(string chs)
        {
            Encoding gb2312 = Encoding.GetEncoding(936);
            byte[] src = gb2312.GetBytes(chs);
            var dest = new byte[src.Length];
            //LCMapString(0x0804, LCMAP_SIMPLIFIED_CHINESE, src, -1, dest, src.Length);

            LCMapString(0x0804, LCMAP_TRADITIONAL_CHINESE, src, -1, dest, src.Length);
            return gb2312.GetString(dest);
        }

        #endregion

        [DllImport("kernel32.dll", EntryPoint = "LCMapStringA")]
        public static extern int LCMapString(
            int Locale,
            int dwMapFlags,
            byte[] lpSrcStr,
            int cchSrc,
            byte[] lpDestStr,
            int cchDest
        );

        public void Init() { }
    }
}
