using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public class EncodingComboBox:ComboBox
    {

        public EncodingComboBox()
         {
             Items.Clear();
             this.Items.AddRange(new object[] {
            "Unicode",
            "UTF-8",
            "GB18030",
            "GBK",
            "Big5",
            "BigEndianUnicode",
            "ASCII"});
             Text = "UTF-8";
         }
        public Encoding SelectedEncoding
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return Encoding.Default;
                }
                try
                {
                    return Encoding.GetEncoding(Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return Encoding.Default;
                }
            }
            set
            {
                if (value.HeaderName == Encoding.UTF8.HeaderName)
                {
                    this.Text = "UTF-8";
                }
                else if (value == Encoding.BigEndianUnicode)
                {
                    Text = "BigEndianUnicode";
                }
                else if (value == Encoding.Unicode)
                {
                    Text = "Unicode";
                }
                else if (value == Encoding.ASCII)
                {
                    Text = "ASCII";
                }
                else if (value == Encoding.GetEncoding("GB18030"))
                {
                    Text = "GB18030";
                }
                else if (value == Encoding.GetEncoding("GBK"))
                {
                    Text = "GBK";
                }
                else if (value == Encoding.GetEncoding("Big5"))
                {
                    Text = "Big5";
                }
                else
                {
                    Text = value.HeaderName.ToUpper();
                }
            }
        }
    }
}
