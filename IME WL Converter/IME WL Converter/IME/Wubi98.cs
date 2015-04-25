using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     搜狗五笔的词库格式为“五笔编码 词语”\r\n
    /// </summary>
    [ComboBoxShow(ConstantString.WUBI98, ConstantString.WUBI98_C, 220)]
    public class Wubi98 : Wubi86
    {
        public override CodeType CodeType
        {
            get { return CodeType.Wubi98; }
        }
    }
}