using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter
{
    /// <summary>
    /// 这个程序主要是为注音准备注音词库的。输入的都是文本的搜狗词库格式的词库，对每个词的拼音进行分析，
    /// 如果这个词的每个字的拼音使用的是默认注音，那么就忽略这个词，
    /// 如果不是，那么就判断每个字的注音是否合理，不合理的注音直接忽略，合理的，那么就把这个词记录下来。
    /// 接下来对留下的词进行处理，遍历每个词，在其他词中找是否存在该词，如果存在，则把该词从长的词中去掉，
    /// 然后看长词的读音是否还是多音，如果不是，那么长词就被删除，如果还有，那么长词保留。
    /// </summary>
    public partial class CreatePinyinWLForm : Form
    {
        private string lastProcessString = "";
        private string processString = "";

        public CreatePinyinWLForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtWLFilePath.Text = openFileDialog1.FileName;
            }
        }

        private void LoadData()
        {
            //try
            //{
            string path = GetText();
            if (path == "")
            {
                MessageBox.Show("请选择一个搜狗词库的文件");
                return;
            }
            string str = FileOperationHelper.ReadFile(path);
            string[] lines = str.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            int count = lines.Length;
            var wls = new Dictionary<string, string>();
            for (int i = 0; i < count; i++)
            {
                string line = lines[i];
                if (line[0] == ';') //说明
                {
                    continue;
                }
                string[] hzpy = line.Split(' ');
                string py = hzpy[0];
                string hz = hzpy[1];
                if (NeedSave(hz, py))
                {
                    //多音字做如下处理
                    if (!wls.ContainsKey(hz))
                    {
                        wls.Add(hz, py);
                    }
                }
                processString = i + "/" + count;
            }
            ShowTextMessage("开始载入现有的注音库");
            string pylibString = FileOperationHelper.ReadFile(ConstantString.PinyinLibPath);

            lines = pylibString.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] hzpy = line.Split(' ');
                string py = hzpy[0];
                string hz = hzpy[1];

                if (!wls.ContainsKey(hz))
                {
                    wls.Add(hz, py);
                }
                processString = i + "/" + count;
            }


            ShowTextMessage("载入全部完成,开始去除重复");
            Dictionary<string, string> rst = RemoveDuplicateWords(wls);


            ShowTextMessage("去除重复完成，开始写入文件");
            StreamWriter sw = FileOperationHelper.WriteFile(ConstantString.PinyinLibPath, Encoding.Unicode); //清空注音库文件
            foreach (string key in rst.Keys)
            {
                string line = rst[key] + " " + key;
                FileOperationHelper.WriteFileLine(sw, line);
            }
            sw.Close();
            toolStripStatusLabel1.Text = "完成!";
            ShowTextMessage("完成!");
            timer1.Stop();
            MessageBox.Show("完成!");
            //}
            //catch (Exception ex)
            //{
            //    ShowTextErrorMessage(ex.Message);
            //}
        }

        private string GetText()
        {
            if (txtWLFilePath.InvokeRequired)
            {
                GetTextD d = GetText;
                return (string) txtWLFilePath.Invoke(d);
            }
            else
            {
                return txtWLFilePath.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
            var thread = new Thread(LoadData);
            thread.Start();
        }

        /// <summary>
        /// 这个单词的拼音是否需要保存(如果完全是默认注音就不保存，否则如果拼音合理，就应该保存)
        /// </summary>
        /// <param name="word"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        private bool NeedSave(string word, List<string> py)
        {
            try
            {
                if (!PinyinHelper.ValidatePinyin(word, py))
                {
                    return false;
                }
                for (int i = 0; i < word.Length; i++)
                {
                    char c = word[i];
                    if (PinyinHelper.GetDefaultPinyin(c) != py[i])
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool NeedSave(string word, string py)
        {
            string[] pylist = py.Split(new[] {"'"}, StringSplitOptions.RemoveEmptyEntries);
            return NeedSave(word, new List<string>(pylist));
        }

        /// <summary>
        /// 去处掉重复的注音词汇
        /// </summary>
        /// <param name="input"></param>
        private Dictionary<string, string> RemoveDuplicateWords(Dictionary<string, string> input)
        {
            var wl = new Dictionary<string, string>();
            foreach (string key in input.Keys)
            {
                if (key.Length > 1 && key.Length <= 5)
                {
                    wl.Add(key, input[key]);
                }
            }
            var rst = new Dictionary<string, string>(wl);
            int count = wl.Count;
            int i = 0;
            foreach (string key in wl.Keys)
            {
                processString = (i++) + "/" + count;


                foreach (string p in wl.Keys)
                {
                    if (p.Contains(key) && p != key)
                    {
                        rst.Remove(p);
                    }
                }
            }

            //内置资源就不需要再生成了
            string[] internalPyLib = Dictionaries.WordPinyin.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in internalPyLib)
            {
                string hz = line.Split(' ')[1];
                if (rst.ContainsKey(hz))
                {
                    rst.Remove(hz);
                }
            }
            return rst;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (processString != lastProcessString)
            {
                toolStripStatusLabel1.Text = processString;
                lastProcessString = processString;
            }
        }


        /// <summary>
        /// 打印错误消息
        /// </summary>
        /// <param name="message"></param>
        private void ShowTextErrorMessage(string message)
        {
            if (richTextBox1.InvokeRequired)
            {
                ShowTextMessageDelegate d = ShowTextErrorMessage;
                richTextBox1.Invoke(d, new object[] {message});
            }
            else
            {
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText(DateTime.Now + "\t" + message + "\r\n");
                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.Focus();
                richTextBox1.ScrollToCaret();
            }
        }

        /// <summary>
        /// 文本框中打印普通消息
        /// </summary>
        /// <param name="message"></param>
        private void ShowTextMessage(string message)
        {
            if (richTextBox1.InvokeRequired)
            {
                ShowTextMessageDelegate d = ShowTextMessage;
                richTextBox1.Invoke(d, new object[] {message});
            }
            else
            {
                richTextBox1.AppendText(DateTime.Now + "\t" + message + "\r\n");
                richTextBox1.Focus();
                richTextBox1.ScrollToCaret();
            }
        }

        #region Nested type: GetTextD

        private delegate string GetTextD();

        #endregion

        #region Nested type: ShowTextMessageDelegate

        /// <summary>
        /// 这个委托是用来多线程情况下使用的，防止跨线程调用
        /// </summary>
        /// <param name="msg"></param>
        private delegate void ShowTextMessageDelegate(string msg);

        #endregion
    }
}