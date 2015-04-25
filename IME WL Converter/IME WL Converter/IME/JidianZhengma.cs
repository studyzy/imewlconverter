using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     极点的词库格式为“编码 词语 词语 词语”\r\n
    /// </summary>
    [ComboBoxShow(ConstantString.JIDIAN_ZHENGMA, ConstantString.JIDIAN_ZHENGMA_C, 190)]
    public class JidianZhengma : Jidian, IWordLibraryTextImport, IWordLibraryExport
    {
        #region IWordLibraryExport 成员

        //private readonly IWordCodeGenerater factory = new ZhengmaGenerater();

        public override string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();
            sb.Append(wl.SingleCode);
            //sb.Append(factory.GetCodeOfString(wl.Word)[0]);
            sb.Append(" ");
            sb.Append(wl.Word);

            return sb.ToString();
        }

        public override CodeType CodeType
        {
            get { return CodeType.Zhengma; }
        }

        #endregion
    }
}