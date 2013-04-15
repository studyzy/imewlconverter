using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public  static class Global
    {
        public static ParsePattern ImportSelfDefiningPattern { get; set; }
        public static string MappingTablePath { get; set; }
        public static ParsePattern ExportSelfDefiningPattern { get; set; }
    }
}
