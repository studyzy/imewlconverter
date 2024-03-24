/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
                System.Text.Encoding.RegisterProvider(
                    System.Text.CodePagesEncodingProvider.Instance
                );

                Items.Clear();
                Items.AddRange(
                    new object[]
                    {
                        "Unicode",
                        "UTF-8",
                        "GB18030",
                        "GBK",
                        "Big5",
                        "UnicodeFFFE",
                        "ASCII"
                    }
                );
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
                    MessageBox.Show(ex.Message, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
