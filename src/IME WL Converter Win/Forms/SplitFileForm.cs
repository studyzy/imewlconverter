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
using System.IO;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter
{
    public partial class SplitFileForm : Form
    {
        public SplitFileForm()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txbFilePath.Text = openFileDialog1.FileName;
            }
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            if (txbFilePath.Text == "")
            {
                MessageBox.Show("请先选择要分割的文件", "分割", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(txbFilePath.Text))
            {
                MessageBox.Show(
                    txbFilePath.Text + "，该文件不存在",
                    "分割",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            rtbLogs.Clear();
            if (rbtnSplitByLine.Checked)
            {
                SplitFileByLine((int)numdMaxLine.Value);
            }
            else if (rbtnSplitBySize.Checked)
            {
                SplitFileBySize((int)numdMaxSize.Value);
            }
            else
            {
                SplitFileByLength((int)numdMaxLength.Value);
            }
            MessageBox.Show("恭喜你，文件分割完成!", "分割", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SplitFileByLine(int maxLine)
        {
            Encoding encoding = FileOperationHelper.GetEncodingType(txbFilePath.Text);

            string str = FileOperationHelper.ReadFile(txbFilePath.Text, encoding);

            string splitLineChar = "\r\n";
            if (str.IndexOf(splitLineChar) < 0)
            {
                if (str.IndexOf('\r') > 0)
                {
                    splitLineChar = "\r";
                }
                else if (str.IndexOf('\n') > 0)
                {
                    splitLineChar = "\n";
                }
                else
                {
                    MessageBox.Show("不能找到行分隔符", "分割", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            string[] list = str.Split(
                new[] { splitLineChar },
                StringSplitOptions.RemoveEmptyEntries
            );

            var fileContent = new StringBuilder();
            int fileIndex = 1;
            for (int i = 0; i < list.Length; i++)
            {
                fileContent.Append(list[i]);
                fileContent.Append(splitLineChar);
                if ((i + 1) % maxLine == 0 || i == list.Length - 1)
                {
                    if (i != 0)
                    {
                        string newFile = GetWriteFilePath(fileIndex++);
                        FileOperationHelper.WriteFile(newFile, encoding, fileContent.ToString());
                        rtbLogs.AppendText(newFile + "\r\n");
                        fileContent = new StringBuilder();
                    }
                }
            }
        }

        private void SplitFileBySize(int maxSize)
        {
            Encoding encoding = FileOperationHelper.GetEncodingType(txbFilePath.Text);

            int fileIndex = 1;
            int size = (maxSize - 10) * 1024; //10K的Buffer
            var inFile = new FileStream(txbFilePath.Text, FileMode.Open, FileAccess.Read);

            do
            {
                string newFile = GetWriteFilePath(fileIndex++);
                var outFile = new FileStream(newFile, FileMode.OpenOrCreate, FileAccess.Write);
                if (fileIndex != 2) //不是第一个文件，那么就要写文件头
                {
                    FileOperationHelper.WriteFileHeader(outFile, encoding);
                }
                int data = 0;
                var buffer = new byte[size];
                if ((data = inFile.Read(buffer, 0, size)) > 0)
                {
                    outFile.Write(buffer, 0, data);
                    bool hasContent = true;
                    do
                    {
                        int b = inFile.ReadByte();
                        if (b == 0xA || b == 0xD)
                        {
                            ReadToNextLine(inFile);

                            hasContent = false;
                        }
                        if (b != -1) //文件已经读完
                        {
                            outFile.WriteByte((byte)b);
                        }
                        else
                        {
                            hasContent = false;
                        }
                    } while (hasContent);
                }
                outFile.Close();
                rtbLogs.AppendText(newFile + "\r\n");
            } while (inFile.Position != inFile.Length);
            inFile.Close();
        }

        private bool ReadToNextLine(FileStream fs)
        {
            do
            {
                int b = fs.ReadByte();

                if (b == -1)
                {
                    return false;
                }
                if (b != 0xA && b != 0xD && b != 0)
                {
                    fs.Position--;
                    return true;
                }
            } while (true);
        }

        private void SplitFileByLength(int length)
        {
            //Encoding encoding = null;
            length = length - 100; //100个字的Buffer
            //string str = FileOperationHelper.ReadFileContent(txbFilePath.Text, ref encoding, Encoding.UTF8);

            Encoding encoding = FileOperationHelper.GetEncodingType(txbFilePath.Text);
            string str = FileOperationHelper.ReadFile(txbFilePath.Text, encoding);
            int fileIndex = 1;
            do
            {
                if (str.Length == 0)
                {
                    break;
                }
                string content = str.Substring(0, Math.Min(str.Length, length));
                str = str.Substring(content.Length);

                int i = Math.Min(str.IndexOf('\r'), str.IndexOf('\n'));
                if (i != -1)
                {
                    content += str.Substring(0, i + 2);
                    str = str.Substring(i + 2);
                }
                string newFile = GetWriteFilePath(fileIndex++);
                FileOperationHelper.WriteFile(newFile, encoding, content);
                rtbLogs.AppendText(newFile + "\r\n");
            } while (true);
        }

        private string GetWriteFilePath(int i)
        {
            string path = txbFilePath.Text;
            return Path.GetDirectoryName(path)
                + "\\"
                + Path.GetFileNameWithoutExtension(path)
                + i.ToString("00")
                + Path.GetExtension(path);
        }
    }
}
