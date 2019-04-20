using System;

namespace Studyzy.IMEWLConverter
{
    public class ComboBoxShowAttribute : Attribute
    {
        public ComboBoxShowAttribute(string name, string shortCode, int index)
        {
            Name = name;
            Index = index;
            ShortCode = shortCode;
        }

        public string ShortCode { get; set; }

        public string Name { get; set; }
        public int Index { get; set; }
    }
}