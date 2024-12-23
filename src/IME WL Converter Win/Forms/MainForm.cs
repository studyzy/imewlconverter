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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.IME;
using Studyzy.IMEWLConverter.Language;

namespace Studyzy.IMEWLConverter
{
    public partial class MainForm : Form
    {
        #region Init

        private readonly IDictionary<string, IWordLibraryExport> exports =
            new Dictionary<string, IWordLibraryExport>();
        private readonly IDictionary<string, IWordLibraryImport> imports =
            new Dictionary<string, IWordLibraryImport>();

        public MainForm()
        {
            InitializeComponent();
            LoadTitle();
        }

        private void LoadImeList()
        {
            Assembly assembly = typeof(MainBody).Assembly;
            Type[] d = assembly.GetTypes();
            var cbxImportItems = new List<ComboBoxShowAttribute>();
            var cbxExportItems = new List<ComboBoxShowAttribute>();

            foreach (Type type in d)
            {
                if (
                    type.Namespace != null
                    && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME")
                )
                {
                    object[] att = type.GetCustomAttributes(typeof(ComboBoxShowAttribute), false);
                    if (att.Length > 0)
                    {
                        var cbxa = att[0] as ComboBoxShowAttribute;
                        Debug.WriteLine(cbxa.Name);
                        Debug.WriteLine(cbxa.Index);
                        if (type.GetInterface("IWordLibraryImport") != null)
                        {
                            Debug.WriteLine("Import!!!!" + type.FullName);
                            cbxImportItems.Add(cbxa);
                            imports.Add(
                                cbxa.Name,
                                assembly.CreateInstance(type.FullName) as IWordLibraryImport
                            );
                        }
                        if (type.GetInterface("IWordLibraryExport") != null)
                        {
                            Debug.WriteLine("Export!!!!" + type.FullName);
                            cbxExportItems.Add(cbxa);
                            exports.Add(
                                cbxa.Name,
                                assembly.CreateInstance(type.FullName) as IWordLibraryExport
                            );
                        }
                    }
                }
            }
            cbxImportItems.Sort((a, b) => a.Index - b.Index);
            cbxExportItems.Sort((a, b) => a.Index - b.Index);
            cbxFrom.Items.Clear();
            foreach (ComboBoxShowAttribute comboBoxShowAttribute in cbxImportItems)
            {
                cbxFrom.Items.Add(comboBoxShowAttribute.Name);
            }
            cbxTo.Items.Clear();
            foreach (ComboBoxShowAttribute comboBoxShowAttribute in cbxExportItems)
            {
                cbxTo.Items.Add(comboBoxShowAttribute.Name);
            }
        }

        private void LoadTitle()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Text = "深蓝词库转换" + v.Major + "." + v.Minor;
            if (v.Revision != 0)
                Text += "." + v.Revision;
        }

        private void InitOpenFileDialogFilter(string select)
        {
            var types = new[]
            {
                "文本文件|*.txt",
                "细胞词库|*.scel",
                "QQ分类词库|*.qpyd",
                "百度分类词库|*.bdict",
                "百度分类词库|*.bcd",
                "搜狗备份词库|*.bin",
                "紫光分类词库|*.uwl",
                "微软拼音词库|*.dat",
                "Gboard词库|*.zip",
                "灵格斯词库|*.ld2",
                "所有文件|*.*"
            };
            int idx = 0;
            for (int i = 0; i < types.Length; i++)
            {
                if (!string.IsNullOrEmpty(select) && types[i].Contains(select))
                    idx = i;
            }
            openFileDialog1.Filter = string.Join("|", types);
            openFileDialog1.FilterIndex = idx;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadImeList();
            InitOpenFileDialogFilter("");
        }

        private IWordLibraryExport GetExportInterface(string str)
        {
            try
            {
                return exports[str];
            }
            catch
            {
                throw new ArgumentException("导出词库的输入法错误");
            }
        }

        private IWordLibraryImport GetImportInterface(string str)
        {
            try
            {
                IWordLibraryImport imp = imports[str];
                return imp;
            }
            catch
            {
                throw new ArgumentException("导入词库的输入法错误");
            }
        }

        #endregion

        //private Encoding ld2WordEncoding=Encoding.UTF8;
        private MainBody mainBody;

        private IWordLibraryExport export;
        private bool exportDirectly => toolStripMenuItemExportDirectly.Checked;

        //private int defaultRank = 10;
        protected string exportFileName;
        private string exportPath = "";

        //private bool isFolderBatchConvert = false;
        private FilterConfig filterConfig = new FilterConfig();

        //private ParsePattern fromUserSetPattern;
        private SortType sortType = SortType.Default;
        private bool sortDesc = false;

        private IWordLibraryImport import;

        private bool mergeTo1File => toolStripMenuItemMergeToOneFile.Checked;

        private bool streamExport => toolStripMenuItemStreamExport.Checked;
        private IWordRankGenerater wordRankGenerater = new DefaultWordRankGenerater();

        private void btnOpenFileDialog_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //this.txbWLPath.Text = openFileDialog1.FileName;
                string files = "";
                foreach (string file in openFileDialog1.FileNames)
                {
                    files += file + " | ";
                }
                txbWLPath.Text = files.Remove(files.Length - 3);
                if (cbxFrom.Text != ConstantString.SELF_DEFINING)
                {
                    cbxFrom.Text = FileOperationHelper.AutoMatchSourceWLType(
                        openFileDialog1.FileName
                    );
                }
            }
        }

        private bool CheckCanRun()
        {
            if (import == null || export == null)
            {
                MessageBox.Show(
                    "请先选择导入词库类型和导出词库类型",
                    "深蓝词库转换",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }
            if (this.txbWLPath.Text == "")
            {
                MessageBox.Show(
                    "请先选择源词库文件",
                    "深蓝词库转换",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }
            return true;
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (!CheckCanRun())
            {
                return;
            }
            richTextBox1.Clear();
            errorMessages = "";
            try
            {
                if (streamExport)
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        exportPath = saveFileDialog1.FileName;
                    else
                    {
                        ShowStatusMessage("请选择词库保存的路径，否则将无法进行词库导出", true);
                        return;
                    }
                }

                if (!mergeTo1File)
                {
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        outputDir = folderBrowserDialog1.SelectedPath;
                    }
                    else
                    {
                        ShowStatusMessage("请选择词库保存的路径，否则将无法进行词库导出", true);
                        return;
                    }
                }
                mainBody = new MainBody();
                mainBody.SelectedWordRankGenerater = wordRankGenerater;
                mainBody.Import = import;
                mainBody.Export = export;
                mainBody.SelectedTranslate = this.translate;
                mainBody.SelectedConverter = this.chineseConverter;
                mainBody.Filters = GetFilters();
                mainBody.SortType = this.sortType;
                mainBody.SortDesc = this.sortDesc;
                mainBody.BatchFilters = GetBatchFilters();
                mainBody.ReplaceFilters = GetReplaceFilters();
                mainBody.FilterConfig = filterConfig;
                mainBody.Import.ImportLineErrorNotice += WriteErrorMessage;
                mainBody.Export.ExportErrorNotice += WriteErrorMessage;
                mainBody.ProcessNotice += RichTextBoxShow;
                timer1.Enabled = true;
                backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IList<IBatchFilter> GetBatchFilters()
        {
            var filters = new List<IBatchFilter>();
            if (filterConfig.NoFilter)
            {
                return filters;
            }
            if (filterConfig.WordRankPercentage < 100)
            {
                var filter = new RankPercentageFilter
                {
                    Percentage = filterConfig.WordRankPercentage
                };
                filters.Add(filter);
            }
            return filters;
        }

        private IList<IReplaceFilter> GetReplaceFilters()
        {
            var filters = new List<IReplaceFilter>();
            if (filterConfig.ReplaceEnglish)
            {
                filters.Add(new EnglishFilter());
            }
            if (filterConfig.ReplacePunctuation)
            {
                filters.Add(new EnglishPunctuationFilter());
                filters.Add(new ChinesePunctuationFilter());
            }
            if (filterConfig.ReplaceSpace)
            {
                filters.Add(new SpaceFilter());
            }
            if (filterConfig.ReplaceNumber)
            {
                filters.Add(new NumberFilter());
            }
            if (cbxFrom.Text == ConstantString.EMOJI)
            {
                filters.Add(new EmojiReplacer(txbWLPath.Text));
            }
            return filters;
        }

        private IList<ISingleFilter> GetFilters()
        {
            var filters = new List<ISingleFilter>();
            if (filterConfig.NoFilter)
            {
                return filters;
            }
            if (filterConfig.IgnoreEnglish)
            {
                filters.Add(new EnglishFilter());
            }
            if (filterConfig.IgnoreFirstCJK)
            {
                filters.Add(new FirstCJKFilter());
            }
            var lenFilter = new LengthFilter();
            lenFilter.MinLength = filterConfig.WordLengthFrom;
            lenFilter.MaxLength = filterConfig.WordLengthTo;

            if (filterConfig.WordLengthFrom > 1 || filterConfig.WordLengthTo < 9999)
            {
                filters.Add(lenFilter);
            }
            var rankFilter = new RankFilter();
            rankFilter.MaxLength = filterConfig.WordRankTo;
            rankFilter.MinLength = filterConfig.WordRankFrom;
            if (filterConfig.WordRankFrom > 1 || filterConfig.WordRankTo < 999999)
            {
                filters.Add(rankFilter);
            }
            if (filterConfig.IgnoreSpace)
            {
                filters.Add(new SpaceFilter());
            }
            if (filterConfig.IgnorePunctuation)
            {
                filters.Add(new ChinesePunctuationFilter());
                filters.Add(new EnglishPunctuationFilter());
            }
            if (filterConfig.IgnoreNumber)
            {
                filters.Add(new NumberFilter());
            }
            if (filterConfig.IgnoreNoAlphabetCode)
            {
                filters.Add(new NoAlphabetCodeFilter());
            }
            return filters;
        }

        private void cbxFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            import = GetImportInterface(cbxFrom.Text);
            Form form = new CoreWinFormMapping().GetImportForm(import);
            if (form != null)
            {
                form.ShowDialog();
            }
        }

        private void cbxTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            export = GetExportInterface(cbxTo.Text);
            Form form = new CoreWinFormMapping().GetExportForm(export);
            if (form != null)
            {
                form.ShowDialog();
            }
        }

        /// <summary>
        ///     在状态上显示消息
        /// </summary>
        /// <param name="statusMessage">消息内容</param>
        /// <param name="showMessageBox">是否弹出窗口显示消息</param>
        private void ShowStatusMessage(string statusMessage, bool showMessageBox)
        {
            toolStripStatusLabel1.Text = statusMessage;
            if (showMessageBox)
            {
                MessageBox.Show(
                    statusMessage,
                    "深蓝词库转换",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var array = (Array)e.Data.GetData(DataFormats.FileDrop);
            string files = "";

            foreach (object a in array)
            {
                string path = a.ToString();
                files += path + " | ";
            }
            txbWLPath.Text = files.Remove(files.Length - 3);
            if (array.Length == 1)
            {
                cbxFrom.Text = FileOperationHelper.AutoMatchSourceWLType(
                    array.GetValue(0).ToString()
                );
            }
        }

        #region 菜单操作

        private void ToolStripMenuItemSplitFile_Click(object sender, EventArgs e)
        {
            new SplitFileForm().ShowDialog();
        }

        private ChineseTranslate translate = ChineseTranslate.NotTrans;
        private IChineseConverter chineseConverter = null;

        private void ToolStripMenuItemChineseTransConfig_Click(object sender, EventArgs e)
        {
            var form = new ChineseConverterSelectForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                translate = form.SelectedTranslate;
                chineseConverter = form.SelectedConverter;
            }
        }

        private void ToolStripMenuItemAccessWebSite_Click(object sender, EventArgs e)
        {
            Process.Start(
                new ProcessStartInfo("https://github.com/studyzy/imewlconverter/releases")
                {
                    UseShellExecute = true
                }
            );
        }

        private void ToolStripMenuItemDonate_Click(object sender, EventArgs e)
        {
            new DonateForm().ShowDialog();
            //Process.Start(new ProcessStartInfo"https://github.com/studyzy/imewlconverter/wiki/Donate") { UseShellExecute = true });
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void ToolStripMenuItemHelp_Click(object sender, EventArgs e)
        {
            new HelpForm().ShowDialog();
        }

        //private void ToolStripMenuItemCreatePinyinWL_Click(object sender, EventArgs e)
        //{
        //    var f = new CreatePinyinWLForm();
        //    f.Show();
        //}

        private void toolStripMenuItemFilterConfig_Click(object sender, EventArgs e)
        {
            var form = new FilterConfigForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                filterConfig = form.FilterConfig;
            }
        }

        private void ToolStripMenuItemMergeWL_Click(object sender, EventArgs e)
        {
            new MergeWLForm().ShowDialog();
        }

        private void ToolStripMenuItemRankGenerate_Click(object sender, EventArgs e)
        {
            var form = new WordRankGenerateForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                wordRankGenerater = form.SelectedWordRankGenerater;
            }
        }

        #endregion

        #region 多线程操作

        private string outputDir;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mainBody != null)
            {
                try
                {
                    ShowStatusMessage(mainBody.ProcessMessage, false);
                    int count = mainBody.CountWord;
                    int current = mainBody.CurrentStatus;
                    //ShowStatusMessage("转换进度：" + mainBody.CurrentStatus + "/" + mainBody.CountWord, false);
                    toolStripProgressBar1.Maximum = count;
                    if (count > 0 && current <= count)
                    {
                        toolStripProgressBar1.Value = current;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var files = FileOperationHelper.GetFilesPath(txbWLPath.Text);

            if (streamExport && import.IsText) //流转换,只有文本类型的才支持。
            {
                mainBody.StreamConvert(files, exportPath);
                timer1.Enabled = false;
                return;
            }

            if (mergeTo1File)
            {
                if (!streamExport)
                {
                    try
                    {
                        mainBody.Convert(files);
                    }
                    catch (Exception ex)
                    {
                        mainBody.Dispose();
                        RichTextBoxShow(ex.Message);
                        throw;
                    }
                }
            }
            else
            {
                mainBody.Convert(files, outputDir);
            }
            timer1.Enabled = false;
        }

        private void RichTextBoxShow(string msg)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(msg + "\r\n")));
            }
            else
            {
                richTextBox1.AppendText(msg + "\r\n");
            }
        }

        private string errorMessages = "";

        private void WriteErrorMessage(string msg)
        {
            errorMessages += msg + "\r\n";
        }

        private void backgroundWorker1_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e
        )
        {
            timer1.Enabled = false;
            mainBody.StopNotice();
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
            ShowStatusMessage("转换完成", false);
            if (this.errorMessages.Length > 0)
            {
                var errForm = new ErrorLogForm(errorMessages);
                errForm.ShowDialog();
            }
            if (e.Error != null)
            {
                MessageBox.Show(
                    "不好意思，发生了错误：" + e.Error.Message,
                    "出错",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                if (e.Error.InnerException != null)
                {
                    RichTextBoxShow(e.Error.InnerException.ToString());
                }
                return;
            }
            if (streamExport && import.IsText)
            {
                ShowStatusMessage("转换完成,词库保存到文件：" + exportPath, true);
                return;
            }
            if (exportDirectly)
            {
                richTextBox1.Text =
                    "为提高处理速度，“高级设置”中选中了“不显示结果，直接导出”，本文本框中不显示转换后的结果，若要查看转换后的结果再确定是否保存请取消该设置。";
            }
            else if (mergeTo1File)
            {
                var dataText = string.Join("\r\n", mainBody.ExportContents);
                if (toolStripMenuItemShowLess.Checked && (dataText.Length > 200000))
                {
                    richTextBox1.Text =
                        "为避免输出时卡死，“高级设置”中选中了“结果只显示首、末10万字”，本文本框中不显示转换后的全部结果，若要查看转换后的结果再确定是否保存请取消该设置。\n\n"
                        + dataText.Substring(0, 100000)
                        + "\n\n\n...\n\n\n"
                        + dataText.Substring(dataText.Length - 100000);
                }
                else if (dataText.Length > 0)
                {
                    richTextBox1.Text = dataText;
                    //btnExport.Enabled = true;
                }
            }
            if (
                !mergeTo1File
                || export is Win10MsPinyin
                || export is Win10MsWubi
                || export is Win10MsPinyinSelfStudy
                || export is Gboard
            ) //微软拼音是二进制文件，不需要再提示保存
            {
                MessageBox.Show(
                    "转换完成!",
                    "深蓝词库转换",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            if (mainBody.Count > 0)
            {
                if (
                    MessageBox.Show(
                        "是否将导入的" + mainBody.Count + "条词库保存到本地硬盘上？",
                        "是否保存",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    ) == DialogResult.No
                )
                    return;
                if (!string.IsNullOrEmpty(exportFileName))
                {
                    saveFileDialog1.FileName = exportFileName;
                }
                else if (export is MsPinyin)
                {
                    saveFileDialog1.DefaultExt = ".dctx";
                    saveFileDialog1.Filter = "微软拼音2010|*.dctx";
                }
                else
                {
                    saveFileDialog1.DefaultExt = ".txt";
                    saveFileDialog1.Filter = "文本文件|*.txt";
                }
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    mainBody.ExportToFile(saveFileDialog1.FileName);

                    ShowStatusMessage("保存成功，词库路径：" + saveFileDialog1.FileName, true);
                }
            }
            else
            {
                MessageBox.Show(
                    "转换失败，没有找到词条",
                    "深蓝词库转换",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        #endregion
    }
}
