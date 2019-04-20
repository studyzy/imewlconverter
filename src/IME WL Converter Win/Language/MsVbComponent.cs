using Microsoft.VisualBasic;

namespace Studyzy.IMEWLConverter.Language
{
    internal class MsVbComponent : IChineseConverter
    {
        #region IChineseConverter Members

        public string ToChs(string cht)
        {
            return Strings.StrConv(cht, VbStrConv.SimplifiedChinese, 0);
        }

        public string ToCht(string chs)
        {
            return Strings.StrConv(chs, VbStrConv.TraditionalChinese, 0);
        }

        #endregion

        public void Init()
        {
        }
    }
}