using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Studyzy.IMEWLConverter;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;
using Studyzy.IMEWLConverter.IME;
using Studyzy.IMEWLConverter.Language;

namespace ImeWlConverterMac.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDictionary<string, IWordLibraryExport> _exports = new Dictionary<string, IWordLibraryExport>();
    private readonly IDictionary<string, IWordLibraryImport> _imports = new Dictionary<string, IWordLibraryImport>();
    
    private string _filePath = "";
    private string _resultText = "";
    private string _statusMessage = "欢迎使用深蓝词库转换工具";
    private double _progress = 0;
    private bool _isConverting = false;
    private bool _showLess = true;
    private bool _exportDirectly = false;
    private bool _streamExport = false;
    private bool _mergeToOneFile = true;
    
    private IWordLibraryImport? _import;
    private IWordLibraryExport? _export;
    private MainBody? _mainBody;
    private FilterConfig _filterConfig = new();
    private IWordRankGenerater _wordRankGenerater = new DefaultWordRankGenerater();
    private ChineseTranslate _translate = ChineseTranslate.NotTrans;
    private IChineseConverter? _chineseConverter;

    public MainWindowViewModel()
    {
        LoadImeList();
        
        // 初始化命令
        OpenFileCommand = new RelayCommand(async () => await OpenFileAsync());
        ConvertCommand = new RelayCommand(async () => await ConvertAsync(), () => CanConvert());
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
            {
                RaiseCanExecuteChanged();
            }
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
            {
                RaiseCanExecuteChanged();
            }
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
                {
                    _import = GetImportInterface(value);
                }
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
                {
                    _export = GetExportInterface(value);
                }
                RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    #region Commands

    public ICommand OpenFileCommand { get; }
    public ICommand ConvertCommand { get; }
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
        var assembly = typeof(MainBody).Assembly;
        var types = assembly.GetTypes();
        var cbxImportItems = new List<ComboBoxShowAttribute>();
        var cbxExportItems = new List<ComboBoxShowAttribute>();

        foreach (var type in types)
        {
            if (type.Namespace != null && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME"))
            {
                var att = type.GetCustomAttributes(typeof(ComboBoxShowAttribute), false);
                if (att.Length > 0)
                {
                    var cbxa = att[0] as ComboBoxShowAttribute;
                    if (cbxa != null)
                    {
                        if (type.GetInterface("IWordLibraryImport") != null)
                        {
                            cbxImportItems.Add(cbxa);
                            var instance = assembly.CreateInstance(type.FullName!) as IWordLibraryImport;
                            if (instance != null)
                                _imports.Add(cbxa.Name, instance);
                        }

                        if (type.GetInterface("IWordLibraryExport") != null)
                        {
                            cbxExportItems.Add(cbxa);
                            var instance = assembly.CreateInstance(type.FullName!) as IWordLibraryExport;
                            if (instance != null)
                                _exports.Add(cbxa.Name, instance);
                        }
                    }
                }
            }
        }

        cbxImportItems.Sort((a, b) => a.Index - b.Index);
        cbxExportItems.Sort((a, b) => a.Index - b.Index);

        ImportTypes.Clear();
        foreach (var item in cbxImportItems)
            ImportTypes.Add(item.Name);

        ExportTypes.Clear();
        foreach (var item in cbxExportItems)
            ExportTypes.Add(item.Name);
    }

    private IWordLibraryImport GetImportInterface(string name)
    {
        if (_imports.TryGetValue(name, out var import))
            return import;
        throw new ArgumentException("导入词库的输入法错误");
    }

    private IWordLibraryExport GetExportInterface(string name)
    {
        if (_exports.TryGetValue(name, out var export))
            return export;
        throw new ArgumentException("导出词库的输入法错误");
    }

    private async Task OpenFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
            if (topLevel != null)
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择词库文件",
                    AllowMultiple = true,
                    FileTypeFilter = new[]
                    {
                        FilePickerFileTypes.TextPlain,
                        FilePickerFileTypes.All
                    }
                });

                if (files.Count > 0)
                {
                    var filePaths = files.Select(f => f.Path.LocalPath).ToArray();
                    FilePath = string.Join(" | ", filePaths);
                    
                    if (filePaths.Length == 1 && SelectedImportType != ConstantString.SELF_DEFINING)
                    {
                        var autoType = FileOperationHelper.AutoMatchSourceWLType(filePaths[0]);
                        if (ImportTypes.Contains(autoType))
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
        
        if (filePaths.Length == 1 && SelectedImportType != ConstantString.SELF_DEFINING)
        {
            var autoType = FileOperationHelper.AutoMatchSourceWLType(filePaths[0]);
            if (ImportTypes.Contains(autoType))
                SelectedImportType = autoType;
        }
    }

    private bool CanConvert()
    {
        return _import != null && _export != null && !string.IsNullOrEmpty(FilePath) && !IsConverting;
    }

    private async Task ConvertAsync()
    {
        if (!CanConvert()) return;

        try
        {
            IsConverting = true;
            Progress = 0;
            ResultText = "";
            StatusMessage = "开始转换...";

            await Task.Run(() => PerformConversion());
            
            // 转换完成后处理保存逻辑
            await HandleConversionCompleted();
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
        }
    }

    private void PerformConversion()
    {
        var files = FileOperationHelper.GetFilesPath(FilePath);
        
        _mainBody = new MainBody
        {
            SelectedWordRankGenerater = _wordRankGenerater,
            Import = _import!,
            Export = _export!,
            SelectedTranslate = _translate,
            SelectedConverter = _chineseConverter,
            Filters = GetFilters(),
            SortType = SortType.Default,
            SortDesc = false,
            BatchFilters = GetBatchFilters(),
            ReplaceFilters = GetReplaceFilters(),
            FilterConfig = _filterConfig
        };

        _mainBody.ProcessNotice += (msg) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                StatusMessage = msg;
                ResultText += msg + "\n";
            });
        };

        try
        {
            _mainBody.Convert(files);
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (!ExportDirectly && MergeToOneFile)
                {
                    var dataText = string.Join("\n", _mainBody.ExportContents);
                    if (ShowLess && dataText.Length > 200000)
                    {
                        ResultText = "为避免输出时卡死，本文本框中不显示转换后的全部结果。\n\n" +
                                   dataText.Substring(0, 100000) + "\n\n\n...\n\n\n" +
                                   dataText.Substring(dataText.Length - 100000);
                    }
                    else if (dataText.Length > 0)
                    {
                        ResultText = dataText;
                    }
                }
                
                StatusMessage = $"转换完成，共转换 {_mainBody.Count} 条词库";
            });
        }
        catch (Exception ex)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                StatusMessage = $"转换失败: {ex.Message}";
                ResultText += $"\n错误: {ex.Message}";
            });
        }
    }

    private IList<ISingleFilter> GetFilters()
    {
        var filters = new List<ISingleFilter>();
        if (_filterConfig.NoFilter) return filters;
        
        if (_filterConfig.IgnoreEnglish) filters.Add(new EnglishFilter());
        if (_filterConfig.IgnoreFirstCJK) filters.Add(new FirstCJKFilter());
        
        var lenFilter = new LengthFilter
        {
            MinLength = _filterConfig.WordLengthFrom,
            MaxLength = _filterConfig.WordLengthTo
        };
        if (_filterConfig.WordLengthFrom > 1 || _filterConfig.WordLengthTo < 9999) 
            filters.Add(lenFilter);
        
        var rankFilter = new RankFilter
        {
            MaxLength = _filterConfig.WordRankTo,
            MinLength = _filterConfig.WordRankFrom
        };
        if (_filterConfig.WordRankFrom > 1 || _filterConfig.WordRankTo < 999999) 
            filters.Add(rankFilter);
        
        if (_filterConfig.IgnoreSpace) filters.Add(new SpaceFilter());
        if (_filterConfig.IgnorePunctuation)
        {
            filters.Add(new ChinesePunctuationFilter());
            filters.Add(new EnglishPunctuationFilter());
        }
        if (_filterConfig.IgnoreNumber) filters.Add(new NumberFilter());
        if (_filterConfig.IgnoreNoAlphabetCode) filters.Add(new NoAlphabetCodeFilter());
        
        return filters;
    }

    private IList<IBatchFilter> GetBatchFilters()
    {
        var filters = new List<IBatchFilter>();
        if (_filterConfig.NoFilter) return filters;
        
        if (_filterConfig.WordRankPercentage < 100)
        {
            var filter = new RankPercentageFilter
            {
                Percentage = _filterConfig.WordRankPercentage
            };
            filters.Add(filter);
        }
        
        return filters;
    }

    private IList<IReplaceFilter> GetReplaceFilters()
    {
        var filters = new List<IReplaceFilter>();
        if (_filterConfig.ReplaceEnglish) filters.Add(new EnglishFilter());
        if (_filterConfig.ReplacePunctuation)
        {
            filters.Add(new EnglishPunctuationFilter());
            filters.Add(new ChinesePunctuationFilter());
        }
        if (_filterConfig.ReplaceSpace) filters.Add(new SpaceFilter());
        if (_filterConfig.ReplaceNumber) filters.Add(new NumberFilter());
        
        return filters;
    }

    #endregion

    #region Command Implementations

    private void ShowFilterConfig()
    {
        // TODO: 实现过滤配置对话框
        StatusMessage = "过滤配置功能待实现";
    }

    private void ShowRankGenerate()
    {
        // TODO: 实现词频生成配置对话框
        StatusMessage = "词频生成配置功能待实现";
    }

    private void ShowChineseTransConfig()
    {
        // TODO: 实现简繁体转换配置对话框
        StatusMessage = "简繁体转换配置功能待实现";
    }

    private void ShowDonate()
    {
        // TODO: 实现捐赠对话框
        StatusMessage = "感谢您的支持！";
    }

    private void ShowHelp()
    {
        // TODO: 实现帮助对话框
        StatusMessage = "帮助功能待实现";
    }

    private void ShowAbout()
    {
        // TODO: 实现关于对话框
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        StatusMessage = $"深蓝词库转换 {version?.Major}.{version?.Minor}";
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

    private void ShowSplitFile()
    {
        // TODO: 实现文件分割对话框
        StatusMessage = "文件分割功能待实现";
    }

    private void ShowMergeWL()
    {
        // TODO: 实现词库合并对话框
        StatusMessage = "词库合并功能待实现";
    }

    private async Task HandleConversionCompleted()
    {
        if (_mainBody == null) return;

        // 检查是否需要保存文件（参考Windows版本逻辑）
        bool needsSaveDialog = MergeToOneFile && 
                              !(_export?.GetType().Name.Contains("Win10") == true ||
                                _export?.GetType().Name.Contains("Gboard") == true);

        if (!needsSaveDialog)
        {
            StatusMessage = "转换完成！";
            return;
        }

        if (_mainBody.Count > 0)
        {
            // 显示转换结果
            if (!ExportDirectly && MergeToOneFile)
            {
                var dataText = string.Join("\n", _mainBody.ExportContents);
                if (ShowLess && dataText.Length > 200000)
                {
                    ResultText = "为避免输出时卡死，本文本框中不显示转换后的全部结果。\n\n" +
                               dataText.Substring(0, 100000) + "\n\n\n...\n\n\n" +
                               dataText.Substring(dataText.Length - 100000);
                }
                else if (dataText.Length > 0)
                {
                    ResultText = dataText;
                }
            }

            // 询问用户是否保存
            await ShowSaveDialog();
        }
        else
        {
            StatusMessage = "转换失败，没有找到词条";
        }
    }

    private async Task ShowSaveDialog()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
            if (topLevel != null)
            {
                // 确定文件扩展名和过滤器
                string defaultExt = ".txt";
                var fileTypes = new List<FilePickerFileType>
                {
                    new("文本文件") { Patterns = new[] { "*.txt" } }
                };

                if (_export?.GetType().Name.Contains("MsPinyin") == true)
                {
                    defaultExt = ".dctx";
                    fileTypes.Insert(0, new("微软拼音") { Patterns = new[] { "*.dctx" } });
                }

                fileTypes.Add(FilePickerFileTypes.All);

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "保存转换结果",
                    FileTypeChoices = fileTypes,
                    DefaultExtension = defaultExt,
                    SuggestedFileName = $"转换结果{defaultExt}"
                });

                if (file != null)
                {
                    var filePath = file.Path.LocalPath;
                    _mainBody?.ExportToFile(filePath);
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
        {
            convertCmd.RaiseCanExecuteChanged();
        }
    }

    #endregion
}

// 简单的命令实现
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}