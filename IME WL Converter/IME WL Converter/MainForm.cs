using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter
{
    public partial class MainForm : Form
    {
        #region Init

        private readonly IDictionary<string, IWordLibraryExport> exports = new Dictionary<string, IWordLibraryExport>();
        private readonly IDictionary<string, IWordLibraryImport> imports = new Dictionary<string, IWordLibraryImport>();

        public MainForm()
        {
            InitializeComponent();
            LoadTitle();
        }

        private void LoadImeList()
        {
            Assembly assembly = GetType().Assembly;
            Type[] d = assembly.GetTypes();
            var cbxImportItems = new List<ComboBoxShowAttribute>();
            var cbxExportItems = new List<ComboBoxShowAttribute>();

            foreach (Type type in d)
            {
                if (type.Namespace != null && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME"))
                {
                    object[] att = type.GetCustomAttributes(typeof (ComboBoxShowAttribute), false);
                    if (att.Length > 0)
                    {
                        var cbxa = att[0] as ComboBoxShowAttribute;
                        Debug.WriteLine(cbxa.Name);
                        Debug.WriteLine(cbxa.Index);
                        if (type.GetInterface("IWordLibraryImport") != null)
                        {
                            Debug.WriteLine("Import!!!!" + type.FullName);
                            cbxImportItems.Add(cbxa);
                            imports.Add(cbxa.Name, assembly.CreateInstance(type.FullName) as IWordLibraryImport);
                        }
                        if (type.GetInterface("IWordLibraryExport") != null)
                        {
                            Debug.WriteLine("Export!!!!" + type.FullName);
                            cbxExportItems.Add(cbxa);
                            exports.Add(cbxa.Name, assembly.CreateInstance(type.FullName) as IWordLibraryExport);
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
            //cbxFrom.Items.Add(ConstantString.SOUGOU_PINYIN);
            //cbxFrom.Items.Add(ConstantString.SOUGOU_XIBAO_SCEL);
            //cbxFrom.Items.Add(ConstantString.SOUGOU_PINYIN_BIN);
            //cbxFrom.Items.Add(ConstantString.SOUGOU_WUBI);

            //cbxFrom.Items.Add(ConstantString.QQ_PINYIN);
            //cbxFrom.Items.Add(ConstantString.QQ_PINYIN_QPYD);
            //cbxFrom.Items.Add(ConstantString.QQ_WUBI);
            //cbxFrom.Items.Add(ConstantString.QQ_PINYIN_ENG);

            //cbxFrom.Items.Add(ConstantString.BAIDU_PINYIN);

            //cbxFrom.Items.Add(ConstantString.BAIDU_BDICT);


            //cbxFrom.Items.Add(ConstantString.GOOGLE_PINYIN);
            //cbxFrom.Items.Add(ConstantString.PINYIN_JIAJIA);
            //cbxFrom.Items.Add(ConstantString.MS_PINYIN);
            //cbxFrom.Items.Add(ConstantString.FIT);
            //cbxFrom.Items.Add(ConstantString.RIME);
            //cbxFrom.Items.Add(ConstantString.ENGKOO_PINYIN);

            //cbxFrom.Items.Add(ConstantString.ZIGUANG_PINYIN);
            //cbxFrom.Items.Add(ConstantString.SINA_PINYIN);
            //cbxFrom.Items.Add(ConstantString.ZHENGMA);


            //cbxFrom.Items.Add(ConstantString.LINGOES_LD2);


            //cbxFrom.Items.Add(ConstantString.BAIDU_SHOUJI);
            //cbxFrom.Items.Add(ConstantString.BAIDU_SHOUJI_ENG);
            //cbxFrom.Items.Add(ConstantString.BAIDU_BCD);
            //cbxFrom.Items.Add(ConstantString.QQ_SHOUJI);
            //cbxFrom.Items.Add(ConstantString.TOUCH_PAL);
            //cbxFrom.Items.Add(ConstantString.IFLY_IME);
            //cbxFrom.Items.Add(ConstantString.SELF_DEFINING);
            //cbxFrom.Items.Add(ConstantString.WORD_ONLY);


            //cbxTo.Items.Add(ConstantString.SOUGOU_PINYIN);
            //cbxTo.Items.Add(ConstantString.SOUGOU_WUBI);
            //cbxTo.Items.Add(ConstantString.QQ_PINYIN);
            //cbxTo.Items.Add(ConstantString.BAIDU_PINYIN);
            //cbxTo.Items.Add(ConstantString.QQ_PINYIN_ENG);
            //cbxTo.Items.Add(ConstantString.SINA_PINYIN);
            //cbxTo.Items.Add(ConstantString.GOOGLE_PINYIN);
            //cbxTo.Items.Add(ConstantString.ZIGUANG_PINYIN);
            //cbxTo.Items.Add(ConstantString.PINYIN_JIAJIA);
            //cbxTo.Items.Add(ConstantString.MS_PINYIN);
            //cbxTo.Items.Add(ConstantString.XIAOXIAO);
            //cbxTo.Items.Add(ConstantString.FIT);
            //cbxTo.Items.Add(ConstantString.RIME);
            //cbxTo.Items.Add(ConstantString.ENGKOO_PINYIN);

            //cbxTo.Items.Add(ConstantString.BAIDU_SHOUJI);
            //cbxTo.Items.Add(ConstantString.BAIDU_SHOUJI_ENG);
            //cbxTo.Items.Add(ConstantString.QQ_SHOUJI);
            //cbxTo.Items.Add(ConstantString.IFLY_IME);
            //cbxTo.Items.Add(ConstantString.SELF_DEFINING);
            //cbxTo.Items.Add(ConstantString.WORD_ONLY);
            LoadImeList();
            InitOpenFileDialogFilter("");
        }

        private IWordLibraryExport GetExportInterface(string str)
        {
            //switch (str)
            //{
            //    case ConstantString.BAIDU_SHOUJI:
            //        return new BaiduShouji();
            //    case ConstantString.QQ_SHOUJI:
            //        return new QQShouji();
            //    case ConstantString.SOUGOU_PINYIN:
            //        return new SougouPinyin();

            //    case ConstantString.SOUGOU_WUBI:
            //        return new SougouWubi();
            //    case ConstantString.QQ_PINYIN:
            //        return new QQPinyin();
            //    case ConstantString.GOOGLE_PINYIN:
            //        return new GooglePinyin();
            //    case ConstantString.WORD_ONLY:
            //        return new NoPinyinWordOnly();
            //    case ConstantString.ZIGUANG_PINYIN:
            //        return new ZiGuangPinyin();
            //    case ConstantString.PINYIN_JIAJIA:
            //        return new PinyinJiaJia();
            //    case ConstantString.SINA_PINYIN:
            //        return new SinaPinyin();
            //    case ConstantString.TOUCH_PAL:
            //        return new TouchPal();
            //    case ConstantString.IFLY_IME:
            //        return new iFlyIME();
            //    case ConstantString.MS_PINYIN:
            //        return new MsPinyin();
            //    case ConstantString.XIAOXIAO:
            //        return new Xiaoxiao();
            //    case ConstantString.SELF_DEFINING:
            //        return new SelfDefining();
            //    case ConstantString.FIT:
            //        return new FitInput();
            //    case ConstantString.BAIDU_PINYIN:
            //        return new BaiduPinyin();
            //    case ConstantString.QQ_PINYIN_ENG:
            //        return new QQPinyinEng();
            //    case ConstantString.BAIDU_SHOUJI_ENG:
            //        return new BaiduShoujiEng();
            //    case ConstantString.RIME:
            //        return new Rime();

            //    case ConstantString.ENGKOO_PINYIN:
            //        return new EngkooPinyin();
            //    default:
            //        throw new ArgumentException("导出词库的输入法错误");
            //}
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
            //switch (str)
            //{
            //    case ConstantString.BAIDU_SHOUJI:
            //        return new BaiduShouji();
            //    case ConstantString.BAIDU_BDICT:
            //        return new BaiduPinyinBdict();
            //    case ConstantString.BAIDU_BCD:
            //        return new BaiduShoujiBcd();
            //    case ConstantString.QQ_SHOUJI:
            //        return new QQShouji();
            //    case ConstantString.SOUGOU_PINYIN:
            //        return new SougouPinyin();
            //    case ConstantString.SOUGOU_PINYIN_BIN:
            //        return new SougouPinyinBin();
            //    case ConstantString.SOUGOU_WUBI:
            //        return new SougouWubi();
            //    case ConstantString.QQ_PINYIN:
            //        return new QQPinyin();
            //    case ConstantString.QQ_PINYIN_QPYD:
            //        return new QQPinyinQpyd();
            //    case ConstantString.QQ_WUBI:
            //        return new QQWubi();
            //    case ConstantString.GOOGLE_PINYIN:
            //        return new GooglePinyin();
            //    case ConstantString.ZIGUANG_PINYIN:
            //        return new ZiGuangPinyin();
            //    case ConstantString.PINYIN_JIAJIA:
            //        return new PinyinJiaJia();
            //    case ConstantString.WORD_ONLY:
            //        return new NoPinyinWordOnly();
            //    case ConstantString.SINA_PINYIN:
            //        return new SinaPinyin();
            //    case ConstantString.SOUGOU_XIBAO_SCEL:
            //        return new SougouPinyinScel();
            //    case ConstantString.ZHENGMA:
            //        return new Zhengma();
            //    case ConstantString.SELF_DEFINING:
            //        return new SelfDefining();
            //    case ConstantString.TOUCH_PAL:
            //        return new TouchPal();
            //    case ConstantString.IFLY_IME:
            //        return new iFlyIME();
            //    case ConstantString.MS_PINYIN:
            //        return new MsPinyin();
            //    case ConstantString.FIT:
            //        return new FitInput();
            //    case ConstantString.RIME:
            //        return new Rime();
            //    case ConstantString.QQ_PINYIN_ENG:
            //        return new QQPinyinEng();
            //    case ConstantString.BAIDU_SHOUJI_ENG:
            //        return new BaiduShoujiEng();
            //    case ConstantString.LINGOES_LD2:
            //        return new LingoesLd2();
            //    case ConstantString.ENGKOO_PINYIN:
            //        return new EngkooPinyin();
            //    default:
            //        throw new ArgumentException("导入词库的输入法错误");
            //}
            try
            {
                var imp = imports[str];
                imp.DefaultRank = defaultRank;
                return imp;
            }
            catch
            {
                throw new ArgumentException("导入词库的输入法错误");
            }
        }

        #endregion
        private bool filterEnglish = true;
        private bool filterSpaceWord = true;
        private int minLength = 1;
        private int maxLength = 100;
        private int minRank = 1;
        private int maxRank = 999999;

        private MainBody mainBody = new MainBody();
      
        private IWordLibraryExport export;
        private bool exportDirectly;
        private int defaultRank = 10;
        protected string exportFileName;
        private string exportPath = "";
        private string fileContent;

        private ParsePattern fromUserSetPattern;
       
       
        private IWordLibraryImport import;
        
        private bool mergeTo1File = true;

        private bool streamExport;
        private ParsePattern toUserSetPattern;
        private CodeType fromCodeType;
        private CodeType toCodeType;

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
                    cbxFrom.Text = FileOperationHelper.AutoMatchSourceWLType(openFileDialog1.FileName);
                }
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();

            try
            {
                import = GetImportInterface(cbxFrom.Text);
                export = GetExportInterface(cbxTo.Text);
                if (import is SelfDefining)
                {
                    ((SelfDefining) import).UserDefiningPattern = fromUserSetPattern;
                }
                if (export is SelfDefining)
                {
                    ((SelfDefining) export).UserDefiningPattern = toUserSetPattern;
                }
                if (import is IMultiCodeType)
                {
                    ((IMultiCodeType) import).CodeType = fromCodeType;
                }
                if (export is IMultiCodeType)
                {
                    ((IMultiCodeType) export).CodeType = toCodeType;
                }
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
                mainBody.Import = import;
                mainBody.Export = export;
                mainBody.Filters = GetFilters();
                timer1.Enabled = true;
                backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private IList<ISingleFilter> GetFilters()
        {
            var filters = new List<ISingleFilter>();
            if (filterEnglish)
            {
                filters.Add(new EnglishFilter());
            }
            var lenFilter = new LengthFilter();
                lenFilter.MinLength = minLength;
                lenFilter.MaxLength = maxLength;
          
            if (minLength>1 || maxLength<9999)
            {
                filters.Add(lenFilter);
            }
            var rankFilter = new RankFilter();
            rankFilter.MaxLength = maxRank;
            rankFilter.MinLength = minRank;
            if (minRank > 1 || maxRank < 999999)
            {
                filters.Add(rankFilter);
            }
            if (filterSpaceWord)
            {
                filters.Add(new SpaceFilter());
            }
            return filters;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将文本框中的所有词条保存到本地硬盘上？", "是否保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                saveFileDialog1.DefaultExt = ".txt";
                if (cbxTo.Text == ConstantString.MS_PINYIN)
                {
                    saveFileDialog1.DefaultExt = ".dctx";
                }
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (FileOperationHelper.WriteFile(saveFileDialog1.FileName, export.Encoding, richTextBox1.Text))
                    {
                        ShowStatusMessage("保存成功，词库路径：" + saveFileDialog1.FileName, true);
                    }
                    else
                    {
                        ShowStatusMessage("保存失败", false);
                    }
                }
            }
        }


        private void cbxFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxFrom.Text == ConstantString.SELF_DEFINING) //弹出自定义窗口
            {
                var selfDefining = new SelfDefiningConverterForm();
                DialogResult show = selfDefining.ShowDialog();
                if (show != DialogResult.OK)
                {
                    cbxFrom.SelectedText = "";
                    return;
                }
                else //选了自定义
                {
                    fromUserSetPattern = selfDefining.SelectedParsePattern;
                }
            }
            if (cbxFrom.Text == ConstantString.XIAOXIAO)
            {
                var diag = new XiaoxiaoConfigForm();
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    fromCodeType = diag.SelectedCodeType;
                }
            }
            if (cbxFrom.Text == ConstantString.RIME)
            {
                var diag = new RimeConfigForm();
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    fromCodeType = diag.SelectedCodeType;
                }
            }
        }
        private void cbxTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxTo.Text == ConstantString.SELF_DEFINING) //弹出自定义窗口
            {
                var selfDefining = new SelfDefiningConverterForm();
                selfDefining.IsImport = false;
                DialogResult show = selfDefining.ShowDialog();
                if (show != DialogResult.OK)
                {
                    cbxFrom.SelectedText = "";
                    return;
                }
                else //选了自定义
                {
                    toUserSetPattern = selfDefining.SelectedParsePattern;
                }
            }
            if (cbxTo.Text == ConstantString.XIAOXIAO)
            {
                var diag = new XiaoxiaoConfigForm();
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    toCodeType = diag.SelectedCodeType;
                }
            }
            if (cbxTo.Text == ConstantString.RIME)
            {
                var diag = new RimeConfigForm();
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    toCodeType = diag.SelectedCodeType;
                }
            }
        }

        #region 多线程操作
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (import != null)
            {
                ShowStatusMessage("转换进度：" + import.CurrentStatus + "/" + import.CountWord, false);
                toolStripProgressBar1.Maximum = import.CountWord;
                if (import.CountWord > 0 && import.CurrentStatus <= import.CountWord)
                {
                    toolStripProgressBar1.Value = import.CurrentStatus;
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] files = txbWLPath.Text.Split('|');
            foreach (string file in files)
            {
                //cbxFrom.Text+"转"+cbxTo.Text+"_"+
                exportFileName = Path.GetFileNameWithoutExtension(file);
                string path = file.Trim();
                if (streamExport && import.IsText) //流转换,只有文本类型的才支持。
                {
                    mainBody.StreamConvert(files, exportFileName);
                }
                else
                {
                    fileContent = mainBody.Convert(files);
                }
            }
            timer1.Enabled = false;
            //简繁体转换


        }



        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Enabled = false;
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
            ShowStatusMessage("转换完成", false);
            if (e.Error != null)
            {
                MessageBox.Show("不好意思，发生了错误：" + e.Error.Message);
                if (e.Error.InnerException != null)
                {
                    richTextBox1.Text = (e.Error.InnerException.ToString());
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
                richTextBox1.Text = "为提高处理速度，高级设置中默认设置为直接导出，本文本框中不显示转换后的结果，若要查看、修改转换后的结果再导出请取消该设置。";
            }
            else
            {
                richTextBox1.Text = fileContent;
                btnExport.Enabled = true;
            }

            if (
                MessageBox.Show("是否将导入的" + mainBody.Count + "条词库保存到本地硬盘上？", "是否保存", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(exportFileName))
                {
                    saveFileDialog1.FileName = exportFileName;
                }
                //if (export is TouchPal)
                //{
                //    saveFileDialog1.DefaultExt = ".bak";
                //    saveFileDialog1.Filter = "触宝备份文件|*.bak";
                //}
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
                    //if (export is TouchPal)
                    //{
                    //    File.Move(fileContent, saveFileDialog1.FileName);
                    //}
                    //else 
                        if (FileOperationHelper.WriteFile(saveFileDialog1.FileName, export.Encoding, fileContent))
                    {
                        ShowStatusMessage("保存成功，词库路径：" + saveFileDialog1.FileName, true);
                    }
                    else
                    {
                        ShowStatusMessage("保存失败", true);
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// 在状态上显示消息
        /// </summary>
        /// <param name="statusMessage">消息内容</param>
        /// <param name="showMessageBox">是否弹出窗口显示消息</param>
        private void ShowStatusMessage(string statusMessage, bool showMessageBox)
        {
            toolStripStatusLabel1.Text = statusMessage;
            if (showMessageBox)
            {
                MessageBox.Show(statusMessage);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var array = (Array) e.Data.GetData(DataFormats.FileDrop);
            string files = "";


            foreach (object a in array)
            {
                string path = a.ToString();
                files += path + " | ";
            }
            txbWLPath.Text = files.Remove(files.Length - 3);
            if (array.Length == 1)
            {
                cbxFrom.Text = FileOperationHelper.AutoMatchSourceWLType(array.GetValue(0).ToString());
            }
        }

      

       

        #region 菜单操作
        private void ToolStripMenuItemSplitFile_Click(object sender, EventArgs e)
        {
            var form = new SplitFileForm();
            form.Show();
        }

        private void ToolStripMenuItemChineseTransConfig_Click(object sender, EventArgs e)
        {
            var form = new ChineseConverterSelectForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                mainBody.SelectedTranslate = form.SelectedTranslate;
                mainBody.SelectedConverter = form.SelectedConverter;
            }
        }
        private void ToolStripMenuItemAccessWebSite_Click(object sender, EventArgs e)
        {
            Process.Start("http://code.google.com/p/imewlconverter/");
        }

        private void toolStripMenuItemExportDirectly_Click(object sender, EventArgs e)
        {
            exportDirectly = toolStripMenuItemExportDirectly.Checked;
        }


       

        private void ToolStripMenuItemDonate_Click(object sender, EventArgs e)
        {
            Process.Start("http://imewlconverter.googlecode.com/svn/wiki/donate.html");
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            var a = new AboutBox();
            a.Show();
        }

        private void ToolStripMenuItemHelp_Click(object sender, EventArgs e)
        {
            var help = new HelpForm();
            help.Show();
        }

        private void toolStripMenuItemStreamExport_Click(object sender, EventArgs e)
        {
            streamExport = toolStripMenuItemStreamExport.Checked;
        }

        private void ToolStripMenuItemCreatePinyinWL_Click(object sender, EventArgs e)
        {
            var f = new CreatePinyinWLForm();
            f.Show();
        }

        private void toolStripMenuItemMergeToOneFile_Click(object sender, EventArgs e)
        {
            mergeTo1File = toolStripMenuItemMergeToOneFile.Checked;
        }

        private void toolStripMenuItemFilterConfig_Click(object sender, EventArgs e)
        {
            FilterConfigForm form=new FilterConfigForm();
            form.WordLengthFrom = minLength;
            form.WordLengthTo = maxLength;
            form.WordRankFrom = minRank;
            form.WordRankTo = maxRank;
            form.IgnoreEnglish = filterEnglish;
            form.IgnoreSpace = filterSpaceWord;

            if (form.ShowDialog() == DialogResult.OK)
            {
                minLength =       form.WordLengthFrom;
                maxLength =       form.WordLengthTo;
                minRank =         form.WordRankFrom;
                maxRank =         form.WordRankTo;
                filterEnglish =   form.IgnoreEnglish;
                filterSpaceWord = form.IgnoreSpace;
            }
        }
        #endregion

    }
}