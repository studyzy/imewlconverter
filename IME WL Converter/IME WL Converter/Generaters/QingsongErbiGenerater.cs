using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
  
    public class QingsongErbiGenerater : ErbiGenerater
    {
        protected override int DicColumnIndex
        {
            get { return 4; }
        }
       
      
    }
}