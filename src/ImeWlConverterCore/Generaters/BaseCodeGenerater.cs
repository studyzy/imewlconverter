using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Generaters
{
    public abstract class BaseCodeGenerater
    {
        public virtual void GetCodeOfWordLibrary(WordLibrary wl)
        {
            wl.Codes = GetCodeOfString(wl.Word);
        }

        public abstract Code GetCodeOfString(string str);
  
    }
}
