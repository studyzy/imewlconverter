using System;
using System.Text;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter
{
    public class EncodingComboBox : ComboBox
    {
        private readonly bool isInit;

        public EncodingComboBox()
        {
            if (!isInit)
            {
                Items.Clear();
                Items.AddRange(new object[]
                {
                    "Unicode",
                    "UTF-8",
                    "GB18030",
                    "GBK",
                    "Big5",
                    "UnicodeFFFE",
                    "ASCII"
                });
                Text = "UTF-8";
                isInit = true;
            }
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
                    Text = "UTF-8";
                }
                else if (value == Encoding.BigEndianUnicode)
                {
                    Text = "UnicodeFFFE";
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