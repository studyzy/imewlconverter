/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)
 *
 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.Helpers;
using ImeWlConverter.Core.WordRank;
using Microsoft.Extensions.DependencyInjection;

namespace Studyzy.IMEWLConverter;

public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConversionPipeline _pipeline;
    private readonly IDictionary<string, IFormatImporter> _importers = new Dictionary<string, IFormatImporter>();
    private readonly IDictionary<string, IFormatExporter> _exporters = new Dictionary<string, IFormatExporter>();

    private IFormatImporter? _selectedImporter;
    private IFormatExporter? _selectedExporter;

    private ChineseConversionMode _chineseConversionMode = ChineseConversionMode.None;

    private FilterConfig filterConfig = new();
    private IWordRankGenerator _wordRankGenerator;

    private string exportPath = "";
    private string outputDir = "";
    private int _convertedCount;

    private string? _exportContent;

    private CancellationTokenSource? _cts;

    private bool exportDirectly => toolStripMenuItemExportDirectly.Checked;
    private bool mergeTo1File => toolStripMenuItemMergeToOneFile.Checked;
    private bool streamExport => toolStripMenuItemStreamExport.Checked;

    public MainForm(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        LoadTitle();
        _serviceProvider = serviceProvider;
        _pipeline = serviceProvider.GetRequiredService<IConversionPipeline>();
        _wordRankGenerator = serviceProvider.GetRequiredService<IWordRankGenerator>();
    }

    private void LoadTitle()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var infoVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "3.3.1";
        if (infoVersion.Contains('+'))
            infoVersion = infoVersion.Split('+')[0];
        if (infoVersion.Contains('-'))
            infoVersion = infoVersion.Split('-')[0];

        Text = "深蓝词库转换" + infoVersion;
    }

    private void LoadImeList()
    {
        var importers = _serviceProvider.GetServices<IFormatImporter>()
            .OrderBy(i => i.Metadata.SortOrder).ToList();
        var exporters = _serviceProvider.GetServices<IFormatExporter>()
            .OrderBy(e => e.Metadata.SortOrder).ToList();

        _importers.Clear();
        _exporters.Clear();

        foreach (var imp in importers)
            _importers[imp.Metadata.DisplayName] = imp;
        foreach (var exp in exporters)
            _exporters[exp.Metadata.DisplayName] = exp;

        cbxFrom.Items.Clear();
        foreach (var imp in importers)
            cbxFrom.Items.Add(imp.Metadata.DisplayName);

        cbxTo.Items.Clear();
        foreach (var exp in exporters)
            cbxTo.Items.Add(exp.Metadata.DisplayName);
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        LoadImeList();
        InitOpenFileDialogFilter("");
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
        var idx = 0;
        for (var i = 0; i < types.Length; i++)
            if (!string.IsNullOrEmpty(select) && types[i].Contains(select))
                idx = i;
        openFileDialog1.Filter = string.Join("|", types);
        openFileDialog1.FilterIndex = idx;
    }

    #region 选择格式

    private void cbxFrom_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_importers.TryGetValue(cbxFrom.Text, out var imp))
        {
            _selectedImporter = imp;
            var form = new CoreWinFormMapping().GetConfigForm(imp.Metadata.Id);
            if (form != null) form.ShowDialog();
        }
    }

    private void cbxTo_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_exporters.TryGetValue(cbxTo.Text, out var exp))
        {
            _selectedExporter = exp;
            var form = new CoreWinFormMapping().GetConfigForm(exp.Metadata.Id);
            if (form != null)
            {
                if (form is SelfDefiningConfigForm selfDefForm)
                    selfDefForm.ShowDialog();
                else
                    form.ShowDialog();
            }
        }
    }

    #endregion

    #region 文件操作

    private void btnOpenFileDialog_Click(object sender, EventArgs e)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            var files = "";
            foreach (var file in openFileDialog1.FileNames) files += file + " | ";
            txbWLPath.Text = files.Remove(files.Length - 3);
            if (_selectedImporter?.Metadata.Id != "self")
            {
                var autoType = AutoMatchImportType(openFileDialog1.FileName);
                if (autoType != null) cbxFrom.Text = autoType;
            }
        }
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Link;
        else
            e.Effect = DragDropEffects.None;
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        var array = e.Data?.GetData(DataFormats.FileDrop) as Array;
        if (array == null || array.Length == 0) return;
        var files = "";

        foreach (var a in array)
        {
            var path = a.ToString();
            files += path + " | ";
        }

        txbWLPath.Text = files.Remove(files.Length - 3);
        if (array.Length == 1)
        {
            var autoType = AutoMatchImportType(array.GetValue(0)?.ToString() ?? "");
            if (autoType != null) cbxFrom.Text = autoType;
        }
    }

    private string? AutoMatchImportType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        var extToId = new Dictionary<string, string>
        {
            { ".scel", "scel" },
            { ".qcel", "qcel" },
            { ".uwl", "uwl" },
            { ".bin", "sgpybin" },
            { ".dat", "win10mspy" },
            { ".bcd", "bcd" },
            { ".bdict", "bdict" },
            { ".qpyd", "qpyd" },
            { ".ld2", "ld2" },
            { ".zip", "gboard" },
            { ".mb", "jdmb" },
        };

        if (extToId.TryGetValue(ext, out var formatId))
        {
            var match = _importers.Values.FirstOrDefault(i => i.Metadata.Id == formatId);
            if (match != null) return match.Metadata.DisplayName;
        }

        // 对于文本文件，通过内容检测格式
        if (Directory.Exists(filePath)) return null;
        var contentFormatId = DetectFormatByContent(filePath);
        if (contentFormatId != null)
        {
            var match = _importers.Values.FirstOrDefault(i => i.Metadata.Id == contentFormatId);
            if (match != null) return match.Metadata.DisplayName;
        }

        return null;
    }

    private static string? DetectFormatByContent(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return null;

            var encoding = FileOperationHelper.GetEncodingType(filePath);
            string? example = null;
            using (var sr = new StreamReader(filePath, encoding))
            {
                for (var i = 0; i < 5; i++)
                {
                    example = sr.ReadLine();
                    if (example == null) break;
                }
            }

            if (string.IsNullOrEmpty(example)) return null;

            // 搜狗拼音txt: 'ni'hao 你好
            if (Regex.IsMatch(example, @"^('[a-z]+)+\s[\u4E00-\u9FA5]+$"))
                return "sgpy";
            // FIT输入法: ni'hao,你好
            if (Regex.IsMatch(example, @"^([a-z]+')+[a-z]+\,[\u4E00-\u9FA5]+$"))
                return "fit";
            // QQ拼音: ni'hao 你好 123
            if (Regex.IsMatch(example, @"^[a-z']+\s[\u4E00-\u9FA5]+\s\d+$"))
                return "qqpy";
            // 拼音加加: 你ni好hao
            if (Regex.IsMatch(example, @"^([\u4E00-\u9FA5]+[a-z]+)+([\u4E00-\u9FA5]+[a-z]*)*$"))
                return "pyjj";
            // 华宇紫光拼音: 你好\tni'hao\t100
            if (Regex.IsMatch(example, @"^[\u4E00-\u9FA5]+\t[a-z']+\t\d+$"))
                return "zgpy";
            // 谷歌拼音: 你好\t100ni hao
            if (Regex.IsMatch(example, @"^[\u4E00-\u9FA5]+\t\d+[a-z\s]+$"))
                return "ggpy";
            // 百度手机: 你好 ni|hao 100
            if (Regex.IsMatch(example, @"^[\u4E00-\u9FA5]+\s[a-z\|]+\s\d+$"))
                return "bdsj";
            // 极点五笔: abcd 你好
            if (Regex.IsMatch(example, @"^[a-z]{1,4}\s[\u4E00-\u9FA5]+$"))
                return "jd";
            // 新浪拼音: nihao 你好
            if (Regex.IsMatch(example, @"^[a-z']+\s[\u4E00-\u9FA5]+$"))
                return "xlpy";

            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region 转换

    private bool CheckCanRun()
    {
        if (_selectedImporter == null || _selectedExporter == null)
        {
            MessageBox.Show(
                "请先选择导入词库类型和导出词库类型",
                "深蓝词库转换",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return false;
        }

        if (txbWLPath.Text == "")
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

    private async void btnConvert_Click(object? sender, EventArgs e)
    {
        if (!CheckCanRun()) return;

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
                outputDir = folderBrowserDialog1.SelectedPath;
            else
            {
                ShowStatusMessage("请选择词库保存的路径，否则将无法进行词库导出", true);
                return;
            }
        }

        richTextBox1.Clear();
        _exportContent = null;
        _cts = new CancellationTokenSource();
        SetConvertingState(true);

        var outputStream = (mergeTo1File && !streamExport) ? new MemoryStream() : null;

        var request = new ConversionRequest
        {
            InputFormatId = _selectedImporter!.Metadata.Id,
            OutputFormatId = _selectedExporter!.Metadata.Id,
            InputPaths = FileOperationHelper.GetFilesPath(txbWLPath.Text).ToList(),
            OutputPath = streamExport ? exportPath : null,
            OutputStream = outputStream,
            MergeToOneFile = mergeTo1File,
            OutputDirectory = outputDir,
            FilterConfig = filterConfig,
            Options = new ConversionOptions
            {
                ChineseConversion = _chineseConversionMode
            }
        };

        var progress = new Progress<ProgressInfo>(OnProgressReported);

        try
        {
            var result = await _pipeline.ExecuteAsync(request, progress, _cts.Token);
            if (result.IsSuccess)
            {
                _convertedCount = result.Value.ExportedCount;
                _exportContent = result.Value.ExportContent;
                HandleConversionCompleted(result.Value);
            }
            else
            {
                MessageBox.Show("转换失败：" + result.Error, "出错",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (OperationCanceledException)
        {
            ShowStatusMessage("转换已取消", false);
        }
        catch (Exception ex)
        {
            MessageBox.Show("不好意思，发生了错误：" + ex.Message, "出错",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            outputStream?.Dispose();
            SetConvertingState(false);
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void OnProgressReported(ProgressInfo info)
    {
        if (info.Total > 0)
        {
            toolStripProgressBar1.Maximum = info.Total;
            toolStripProgressBar1.Value = Math.Min(info.Current, info.Total);
        }
        if (info.Message is not null)
        {
            toolStripStatusLabel1.Text = info.Message;
            richTextBox1.AppendText(info.Message + "\r\n");
        }
    }

    private void SetConvertingState(bool converting)
    {
        if (converting)
        {
            btnConvert.Text = "取 消";
            btnConvert.Click -= btnConvert_Click;
            btnConvert.Click += btnCancel_Click;
            cbxFrom.Enabled = false;
            cbxTo.Enabled = false;
            btnOpenFileDialog.Enabled = false;
            toolStripProgressBar1.Value = 0;
        }
        else
        {
            btnConvert.Text = "转 换";
            btnConvert.Click -= btnCancel_Click;
            btnConvert.Click += btnConvert_Click;
            cbxFrom.Enabled = true;
            cbxTo.Enabled = true;
            btnOpenFileDialog.Enabled = true;
        }
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
        ShowStatusMessage("正在取消...", false);
    }

    private void HandleConversionCompleted(ConversionResult result)
    {
        toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
        ShowStatusMessage("转换完成", false);

        if (!string.IsNullOrEmpty(result.ErrorMessages))
        {
            var errForm = new ErrorLogForm(result.ErrorMessages);
            errForm.ShowDialog();
        }

        if (!mergeTo1File)
        {
            MessageBox.Show(
                "转换完成!",
                "深蓝词库转换",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        if (exportDirectly)
        {
            richTextBox1.Text =
                "为提高处理速度，\u201c高级设置\u201d中选中了\u201c不显示结果，直接导出\u201d，本文本框中不显示转换后的结果，若要查看转换后的结果再确定是否保存请取消该设置。";
        }
        else if (_exportContent != null)
        {
            if (toolStripMenuItemShowLess.Checked && _exportContent.Length > 200000)
                richTextBox1.Text =
                    "为避免输出时卡死，\u201c高级设置\u201d中选中了\u201c结果只显示首、末10万字\u201d，本文本框中不显示转换后的全部结果，若要查看转换后的结果再确定是否保存请取消该设置。\n\n"
                    + _exportContent.Substring(0, 100000)
                    + "\n\n\n...\n\n\n"
                    + _exportContent.Substring(_exportContent.Length - 100000);
            else if (_exportContent.Length > 0) richTextBox1.Text = _exportContent;
        }

        if (_convertedCount > 0)
        {
            if (
                MessageBox.Show(
                    "是否将导入的" + _convertedCount + "条词库保存到本地硬盘上？",
                    "是否保存",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) == DialogResult.No
            )
                return;

            var ext = _selectedExporter?.Metadata.FileExtension ?? ".txt";
            var filterName = ext == ".txt" ? "文本文件" : _selectedExporter!.Metadata.DisplayName;
            saveFileDialog1.DefaultExt = ext;
            saveFileDialog1.Filter = $"{filterName}|*{ext}|所有文件|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (_exportContent != null)
                {
                    File.WriteAllText(saveFileDialog1.FileName, _exportContent);
                }

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

    #region UI 辅助

    private void ShowStatusMessage(string statusMessage, bool showMessageBox)
    {
        toolStripStatusLabel1.Text = statusMessage;
        if (showMessageBox)
            MessageBox.Show(
                statusMessage,
                "深蓝词库转换",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
    }

    #endregion

    #region 菜单操作

    private void ToolStripMenuItemSplitFile_Click(object sender, EventArgs e)
    {
        new SplitFileForm().ShowDialog();
    }

    private void ToolStripMenuItemChineseTransConfig_Click(object sender, EventArgs e)
    {
        var form = new ChineseConverterSelectForm();
        if (form.ShowDialog() == DialogResult.OK)
        {
            _chineseConversionMode = form.SelectedConversionMode;
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
    }

    private void btnAbout_Click(object sender, EventArgs e)
    {
        new AboutBox().ShowDialog();
    }

    private void ToolStripMenuItemHelp_Click(object sender, EventArgs e)
    {
        new HelpForm().ShowDialog();
    }

    private void toolStripMenuItemFilterConfig_Click(object sender, EventArgs e)
    {
        var form = new FilterConfigForm();

        if (form.ShowDialog() == DialogResult.OK) filterConfig = form.FilterConfig;
    }

    private void ToolStripMenuItemMergeWL_Click(object sender, EventArgs e)
    {
        new MergeWLForm().ShowDialog();
    }

    private void ToolStripMenuItemRankGenerate_Click(object sender, EventArgs e)
    {
        var form = new WordRankGenerateForm();
        if (form.ShowDialog() == DialogResult.OK) _wordRankGenerator = form.SelectedWordRankGenerator;
    }

    #endregion
}
