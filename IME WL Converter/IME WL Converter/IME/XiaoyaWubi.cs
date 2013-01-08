namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.XIAOYA_WUBI, ConstantString.XIAOYA_WUBI_C, 191)]
    public class XiaoyaWubi : Jidian
    {
        protected override bool IsWubi
        {
            get { return true; }
        }
    }
}