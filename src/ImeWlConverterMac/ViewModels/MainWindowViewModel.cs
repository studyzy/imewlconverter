using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Abstractions.Options;
using ImeWlConverter.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ImeWlConverterMac.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConversionPipeline _pipeline;
    private readonly IDictionary<string, IFormatImporter> _importers = new Dictionary<string, IFormatImporter>();
    private readonly IDictionary<string, IFormatExporter> _exporters = new Dictionary<string, IFormatExporter>();

    private string _filePath = "";
    private string _resultText = "";
    private string _statusMessage = "欢迎使用深蓝词库转换工具";
    private double _progress;
    private bool _isConverting;
    private bool _showLess = true;
    private bool _exportDirectly;
    private bool _streamExport;
    private bool _mergeToOneFile = true;

    private IFormatImporter? _selectedImporter;
    private IFormatExporter? _selectedExporter;
    private ChineseConversionMode _chineseConversion = ChineseConversionMode.None;
    private FilterConfig _filterConfig = new();
    private string? _lastExportContent;
    private CancellationTokenSource? _cts;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _pipeline = serviceProvider.GetRequiredService<IConversionPipeline>();

        LoadImeList();

        // 初始化命令
        OpenFileCommand = new RelayCommand(async () => await OpenFileAsync());
        ConvertCommand = new RelayCommand(async () => await ConvertAsync(), () => CanConvert());
        CancelCommand = new RelayCommand(() => _cts?.Cancel());
        FilterConfigCommand = new RelayCommand(() => ShowFilterConfig());
        RankGenerateCommand = new RelayCommand(() => ShowRankGenerate());
        ChineseTransConfigCommand = new RelayCommand(() => ShowChineseTransConfig());
        DonateCommand = new RelayCommand(() => ShowDonate());
        HelpCommand = new RelayCommand(() => ShowHelp());
        AboutCommand = new RelayCommand(() => ShowAbout());
        AccessWebSiteCommand = new RelayCommand(() => AccessWebSite());
        SplitFileCommand = new RelayCommand(() => ShowSplitFile());
        MergeWLCommand = new RelayCommand(() => ShowMergeWL());

        // 切换命令
        ToggleShowLessCommand = new RelayCommand(() => ShowLess = !ShowLess);
        ToggleExportDirectlyCommand = new RelayCommand(() => ExportDirectly = !ExportDirectly);
        ToggleStreamExportCommand = new RelayCommand(() => StreamExport = !StreamExport);
        ToggleMergeToOneFileCommand = new RelayCommand(() => MergeToOneFile = !MergeToOneFile);
    }

    #region Properties

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetField(ref _filePath, value))
                RaiseCanExecuteChanged();
        }
    }

    public string ResultText
    {
        get => _resultText;
        set => SetField(ref _resultText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetField(ref _progress, value);
    }

    public bool IsConverting
    {
        get => _isConverting;
        set
        {
            if (SetField(ref _isConverting, value))
                RaiseCanExecuteChanged();
        }
    }

    public bool ShowLess
    {
        get => _showLess;
        set => SetField(ref _showLess, value);
    }

    public bool ExportDirectly
    {
        get => _exportDirectly;
        set => SetField(ref _exportDirectly, value);
    }

    public bool StreamExport
    {
        get => _streamExport;
        set => SetField(ref _streamExport, value);
    }

    public bool MergeToOneFile
    {
        get => _mergeToOneFile;
        set => SetField(ref _mergeToOneFile, value);
    }

    public ObservableCollection<string> ImportTypes { get; } = new();
    public ObservableCollection<string> ExportTypes { get; } = new();

    private string? _selectedImportType;
    public string? SelectedImportType
    {
        get => _selectedImportType;
        set
        {
            if (SetField(ref _selectedImportType, value))
            {
                if (value != null)
                    _selectedImporter = _importers.TryGetValue(value, out var imp) ? imp : null;
                RaiseCanExecuteChanged();
            }
        }
    }

    private string? _selectedExportType;
    public string? SelectedExportType
    {
        get => _selectedExportType;
        set
        {
            if (SetField(ref _selectedExportType, value))
            {
                if (value != null)
                    _selectedExporter = _exporters.TryGetValue(value, out var exp) ? exp : null;
                RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    #region Commands

    public ICommand OpenFileCommand { get; }
    public ICommand ConvertCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand FilterConfigCommand { get; }
    public ICommand RankGenerateCommand { get; }
    public ICommand ChineseTransConfigCommand { get; }
    public ICommand DonateCommand { get; }
    public ICommand HelpCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand AccessWebSiteCommand { get; }
    public ICommand SplitFileCommand { get; }
    public ICommand MergeWLCommand { get; }

    // 切换命令
    public ICommand ToggleShowLessCommand { get; }
    public ICommand ToggleExportDirectlyCommand { get; }
    public ICommand ToggleStreamExportCommand { get; }
    public ICommand ToggleMergeToOneFileCommand { get; }

    #endregion

    #region Methods

    private void LoadImeList()
    {
        var importers = _serviceProvider.GetServices<IFormatImporter>()
            .OrderBy(i => i.Metadata.SortOrder).ToList();
        var exporters = _serviceProvider.GetServices<IFormatExporter>()
            .OrderBy(e => e.Metadata.SortOrder).ToList();

        foreach (var imp in importers)
            _importers[imp.Metadata.DisplayName] = imp;
        foreach (var exp in exporters)
            _exporters[exp.Metadata.DisplayName] = exp;

        ImportTypes.Clear();
        foreach (var imp in importers)
            ImportTypes.Add(imp.Metadata.DisplayName);
        ExportTypes.Clear();
        foreach (var exp in exporters)
            ExportTypes.Add(exp.Metadata.DisplayName);
    }

    private async Task OpenFileAsync()
    {
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel != null)
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择词库文件",
                    AllowMultiple = true,
                    FileTypeFilter = new[]
                    {
                        FilePickerFileTypes.All,
                        FilePickerFileTypes.TextPlain
                    }
                });

                if (files.Count > 0)
                {
                    var filePaths = files.Select(f => f.Path.LocalPath).ToArray();
                    FilePath = string.Join(" | ", filePaths);

                    if (filePaths.Length == 1)
                    {
                        var autoType = AutoMatchImportType(filePaths[0]);
                        if (autoType != null && ImportTypes.Contains(autoType))
                            SelectedImportType = autoType;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开文件失败: {ex.Message}";
        }
    }

    public void HandleFileDrop(string[] filePaths)
    {
        FilePath = string.Join(" | ", filePaths);

        if (filePaths.Length == 1)
        {
            var autoType = AutoMatchImportType(filePaths[0]);
            if (autoType != null && ImportTypes.Contains(autoType))
                SelectedImportType = autoType;
        }
    }

    private string? AutoMatchImportType(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) return null;

        var extToId = new Dictionary<string, string>
        {
            { ".scel", "scel" },
            { ".qcel", "qcel" },
            { ".qpyd", "qpyd" },
            { ".bcd", "baiduBcd" },
            { ".bdict", "baiduBdict" },
            { ".ld2", "lingoesLd2" },
            { ".uwl", "ziguangUwl" },
            { ".bin", "sougouBin" },
            { ".plist", "macPlist" },
        };

        if (extToId.TryGetValue(ext, out var formatId))
        {
            var match = _importers.Values.FirstOrDefault(i => i.Metadata.Id == formatId);
            if (match != null) return match.Metadata.DisplayName;
        }

        return null;
    }

    private bool CanConvert()
    {
        return _selectedImporter != null && _selectedExporter != null
            && !string.IsNullOrEmpty(FilePath) && !IsConverting;
    }

    private async Task ConvertAsync()
    {
        if (!CanConvert()) return;

        IsConverting = true;
        Progress = 0;
        ResultText = "";
        _lastExportContent = null;
        _cts = new CancellationTokenSource();

        try
        {
            var inputFiles = FilePath.Split([" | "], StringSplitOptions.RemoveEmptyEntries).ToList();
            var outputStream = MergeToOneFile ? new MemoryStream() : null;

            var targetCodeType = CodeTypeInference.InferFromOutputFormat(_selectedExporter!.Metadata.Id);

            var request = new ConversionRequest
            {
                InputFormatId = _selectedImporter!.Metadata.Id,
                OutputFormatId = _selectedExporter!.Metadata.Id,
                InputPaths = inputFiles,
                OutputStream = outputStream,
                MergeToOneFile = MergeToOneFile,
                FilterConfig = _filterConfig,
                Options = new ConversionOptions
                {
                    ChineseConversion = _chineseConversion,
                    CodeGeneration = new CodeGenerationOptions
                    {
                        TargetCodeType = targetCodeType
                    }
                }
            };

            var progress = new Progress<ProgressInfo>(info =>
            {
                if (info.Total > 0)
                    Progress = info.Percentage;
                if (info.Message is not null)
                    StatusMessage = info.Message;
            });

            var result = await Task.Run(() => _pipeline.ExecuteAsync(request, progress, _cts.Token));

            if (result.IsSuccess)
            {
                _lastExportContent = result.Value.ExportContent;
                StatusMessage = $"转换完成，导入 {result.Value.ImportedCount} 条，导出 {result.Value.ExportedCount} 条";

                if (_lastExportContent != null)
                {
                    if (ShowLess && _lastExportContent.Length > 200000)
                    {
                        ResultText = "为避免输出时卡死，本文本框中不显示转换后的全部结果。\n\n" +
                                     _lastExportContent[..100000] + "\n\n\n...\n\n\n" +
                                     _lastExportContent[^100000..];
                    }
                    else
                    {
                        ResultText = _lastExportContent;
                    }
                }

                if (!string.IsNullOrEmpty(result.Value.ErrorMessages))
                    StatusMessage += " (部分文件有错误)";

                await ShowSaveDialog();
            }
            else
            {
                StatusMessage = $"转换失败: {result.Error}";
                ResultText = result.Error ?? "未知错误";
            }

            outputStream?.Dispose();
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "转换已取消";
        }
        catch (Exception ex)
        {
            StatusMessage = $"转换失败: {ex.Message}";
            ResultText = $"错误: {ex.Message}\n{ex.StackTrace}";
        }
        finally
        {
            IsConverting = false;
            Progress = 100;
            _cts?.Dispose();
            _cts = null;
        }
    }

    #endregion

    #region Command Implementations

    private async void ShowFilterConfig()
    {
        try
        {
            var window = new Views.FilterConfigWindow(_filterConfig);
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                if (result == true)
                {
                    _filterConfig = window.FilterConfig;
                    StatusMessage = "过滤配置已更新";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开过滤配置失败: {ex.Message}";
        }
    }

    private async void ShowRankGenerate()
    {
        try
        {
            var window = new Views.WordRankGenerateWindow();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                if (result == true)
                {
                    StatusMessage = $"词频生成模式已设置为: {window.SelectedMode}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开词频生成配置失败: {ex.Message}";
        }
    }

    private async void ShowChineseTransConfig()
    {
        try
        {
            var window = new Views.ChineseConverterSelectWindow(_chineseConversion);
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                if (result == true)
                {
                    _chineseConversion = window.SelectedConversionMode;
                    StatusMessage = "简繁体转换配置已更新";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开简繁体转换配置失败: {ex.Message}";
        }
    }

    private async void ShowDonate()
    {
        try
        {
            var window = new Views.DonateWindow();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
                await window.ShowDialog(mainWindow);
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开捐赠窗口失败: {ex.Message}";
        }
    }

    private async void ShowHelp()
    {
        try
        {
            var window = new Views.HelpWindow();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
                await window.ShowDialog(mainWindow);
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开帮助窗口失败: {ex.Message}";
        }
    }

    private async void ShowAbout()
    {
        try
        {
            var window = new Views.AboutWindow();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
                await window.ShowDialog(mainWindow);
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开关于窗口失败: {ex.Message}";
        }
    }

    private void AccessWebSite()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/studyzy/imewlconverter/releases",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"无法打开网站: {ex.Message}";
        }
    }

    private async void ShowSplitFile()
    {
        try
        {
            var window = new Views.SplitFileWindow();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
                await window.ShowDialog(mainWindow);
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开文件分割窗口失败: {ex.Message}";
        }
    }

    private async void ShowMergeWL()
    {
        try
        {
            var window = new Views.MergeWLWindow();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
                await window.ShowDialog(mainWindow);
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开词库合并窗口失败: {ex.Message}";
        }
    }

    private TopLevel? GetTopLevel()
    {
        var mainWindow = GetMainWindow();
        return mainWindow != null ? TopLevel.GetTopLevel(mainWindow) : null;
    }

    private Window? GetMainWindow()
    {
        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }

    private async Task ShowSaveDialog()
    {
        if (string.IsNullOrEmpty(_lastExportContent)) return;

        try
        {
            var topLevel = GetTopLevel();
            if (topLevel != null)
            {
                var ext = _selectedExporter?.Metadata.FileExtension ?? ".txt";
                var filterName = ext == ".txt" ? "文本文件" : _selectedExporter!.Metadata.DisplayName;
                var fileTypes = new List<FilePickerFileType>
                {
                    new(filterName) { Patterns = new[] { $"*{ext}" } }
                };

                if (ext != ".txt")
                {
                    fileTypes.Add(new("文本文件") { Patterns = new[] { "*.txt" } });
                }

                fileTypes.Add(FilePickerFileTypes.All);

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "保存转换结果",
                    FileTypeChoices = fileTypes,
                    DefaultExtension = ext,
                    SuggestedFileName = $"转换结果{ext}"
                });

                if (file != null)
                {
                    var filePath = file.Path.LocalPath;
                    await File.WriteAllTextAsync(filePath, _lastExportContent);
                    StatusMessage = $"保存成功，词库路径：{filePath}";
                }
                else
                {
                    StatusMessage = "用户取消了保存操作";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
    }

    private void RaiseCanExecuteChanged()
    {
        if (ConvertCommand is RelayCommand convertCmd)
            convertCmd.RaiseCanExecuteChanged();
    }

    #endregion
}
