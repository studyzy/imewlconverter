using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.XIAOYA_WUBI, ConstantString.XIAOYA_WUBI_C, 191)]
    public class XiaoyaWubi : Jidian
    {
       

        public override CodeType CodeType
        {
            get { return CodeType.Wubi; }
        }
    }
}