using System.Collections.Generic;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class Wubi98Generater : Wubi86Generater
    {
        public override string GetDefaultCodeOfChar(char str)
        {
            return DictionaryHelper.GetCode(str).Wubi98;
        }


        public override IList<string> GetCodeOfChar(char str)
        {
            return new List<string> {DictionaryHelper.GetCode(str).Wubi98};
        }
    }
}