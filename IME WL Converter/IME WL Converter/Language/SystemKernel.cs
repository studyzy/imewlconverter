using System.Runtime.InteropServices;
using System.Text;

namespace Studyzy.IMEWLConverter.Language
{
    internal class SystemKernel : IChineseConverter
    {
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        #region IChineseConverter Members

        public string ToChs(string cht)
        {
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
        public static extern int LCMapString(int Locale, int dwMapFlags, byte[] lpSrcStr, int cchSrc, byte[] lpDestStr,
                                             int cchDest);

        public void Init()
        {
        }
    }
}