using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.IME
{
    public class BaseImport
    {
        public BaseImport()
        {
            DefaultRank = 1;
            CodeType = CodeType.Pinyin;
        }

        public virtual CodeType CodeType { get; set; }
        public virtual int DefaultRank { get; set; }
        public virtual int CountWord { get; set; }
        public virtual int CurrentStatus { get; set; }

        public virtual bool IsText
        {
            get { return true; }
        }
    }
}