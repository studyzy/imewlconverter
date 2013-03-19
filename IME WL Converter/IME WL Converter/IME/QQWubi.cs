using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.QQ_WUBI, ConstantString.QQ_WUBI_C, 70)]
    public class QQWubi : Jidian
    {
        protected override bool IsWubi
        {
            get { return true; }
        }
        public override CodeType CodeType
        {
            get
            {
                return CodeType.Wubi;
            }
         
        }
    }
}