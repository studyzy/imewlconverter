using System.Collections.Generic;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class Wubi98Generater : Wubi86Generater
    {
        public override string GetDefaultCodeOfChar(char str)
        {
            return DictionaryHelper.GetCode(str).Wubi98;
        }

        public override IList<string> GetAllCodesOfChar(char str)
        {
            return new List<string> {DictionaryHelper.GetCode(str).Wubi98};
        }
    }
}