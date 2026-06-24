using Avalonia.Controls;
using Avalonia.Interactivity;
using ImeWlConverter.Abstractions.Options;

namespace ImeWlConverterMac.Views;

public partial class FilterConfigWindow : Window
{
    public FilterConfig FilterConfig { get; private set; }

    /// <summary>
    /// 编码生成选项（由 UI 上的 KeepEnglish/KeepNumber/PrefixEnglish 等 checkbox 决定）。
    /// </summary>
    public CodeGenerationOptions CodeGenerationOptions { get; private set; } = new();

    public FilterConfigWindow()
    {
        InitializeComponent();
        FilterConfig = new FilterConfig();
        LoadConfig();
    }

    public FilterConfigWindow(FilterConfig filterConfig)
    {
        InitializeComponent();
        // Clone to avoid modifying the original on cancel
        FilterConfig = new FilterConfig
        {
            NoFilter = filterConfig.NoFilter,
            WordLengthFrom = filterConfig.WordLengthFrom,
            WordLengthTo = filterConfig.WordLengthTo,
            WordRankFrom = filterConfig.WordRankFrom,
            WordRankTo = filterConfig.WordRankTo,
            WordRankPercentage = filterConfig.WordRankPercentage,
            IgnoreEnglish = filterConfig.IgnoreEnglish,
            IgnoreNumber = filterConfig.IgnoreNumber,
            IgnoreSpace = filterConfig.IgnoreSpace,
            IgnorePunctuation = filterConfig.IgnorePunctuation,
            IgnoreNoAlphabetCode = filterConfig.IgnoreNoAlphabetCode,
            IgnoreFirstCJK = filterConfig.IgnoreFirstCJK,
            ReplaceEnglish = filterConfig.ReplaceEnglish,
            ReplaceNumber = filterConfig.ReplaceNumber,
            ReplaceSpace = filterConfig.ReplaceSpace,
            ReplacePunctuation = filterConfig.ReplacePunctuation,
        };
        LoadConfig();
    }

    private void LoadConfig()
    {
        numWordLengthFrom.Value = FilterConfig.WordLengthFrom;
        numWordLengthTo.Value = FilterConfig.WordLengthTo;
        numWordRankFrom.Value = FilterConfig.WordRankFrom;
        numWordRankTo.Value = FilterConfig.WordRankTo;
        numWordRankPercentage.Value = FilterConfig.WordRankPercentage;

        cbxFilterEnglish.IsChecked = FilterConfig.IgnoreEnglish;
        cbxFilterSpace.IsChecked = FilterConfig.IgnoreSpace;
        cbxFilterPunctuation.IsChecked = FilterConfig.IgnorePunctuation;
        cbxNoFilter.IsChecked = FilterConfig.NoFilter;
        cbxFilterNumber.IsChecked = FilterConfig.IgnoreNumber;
        cbxFilterNoAlphabetCode.IsChecked = FilterConfig.IgnoreNoAlphabetCode;
        cbxFilterFirstCJK.IsChecked = FilterConfig.IgnoreFirstCJK;

        cbxReplaceEnglish.IsChecked = FilterConfig.ReplaceEnglish;
        cbxReplaceNumber.IsChecked = FilterConfig.ReplaceNumber;
        cbxReplacePunctuation.IsChecked = FilterConfig.ReplacePunctuation;
        cbxReplaceSpace.IsChecked = FilterConfig.ReplaceSpace;

        cbxKeepEnglish.IsChecked = false;
        cbxKeepNumber.IsChecked = false;
        cbxKeepPunctuation.IsChecked = false;
        cbxKeepSpace.IsChecked = false;
        cbxKeepEnglish_.IsChecked = false;
        cbxKeepNumber_.IsChecked = false;
        cbxKeepPunctuation_.IsChecked = false;
        cbxKeepSpace_.IsChecked = false;

        cbxFullWidth.IsChecked = false;
        cbxChsNumber.IsChecked = false;
        cbxPrefixEnglish.IsChecked = false;
    }

    private void BtnOK_Click(object? sender, RoutedEventArgs e)
    {
        FilterConfig.WordLengthFrom = (int)(numWordLengthFrom.Value ?? 0);
        FilterConfig.WordLengthTo = (int)(numWordLengthTo.Value ?? 0);
        FilterConfig.WordRankFrom = (int)(numWordRankFrom.Value ?? 0);
        FilterConfig.WordRankTo = (int)(numWordRankTo.Value ?? 0);
        FilterConfig.WordRankPercentage = (int)(numWordRankPercentage.Value ?? 0);

        FilterConfig.IgnoreEnglish = cbxFilterEnglish.IsChecked ?? false;
        FilterConfig.IgnoreSpace = cbxFilterSpace.IsChecked ?? false;
        FilterConfig.IgnorePunctuation = cbxFilterPunctuation.IsChecked ?? false;
        FilterConfig.IgnoreNumber = cbxFilterNumber.IsChecked ?? false;
        FilterConfig.IgnoreNoAlphabetCode = cbxFilterNoAlphabetCode.IsChecked ?? false;
        FilterConfig.NoFilter = cbxNoFilter.IsChecked ?? false;
        FilterConfig.IgnoreFirstCJK = cbxFilterFirstCJK.IsChecked ?? false;

        FilterConfig.ReplaceNumber = cbxReplaceNumber.IsChecked ?? false;
        FilterConfig.ReplaceEnglish = cbxReplaceEnglish.IsChecked ?? false;
        FilterConfig.ReplaceSpace = cbxReplaceSpace.IsChecked ?? false;
        FilterConfig.ReplacePunctuation = cbxReplacePunctuation.IsChecked ?? false;

        CodeGenerationOptions = new CodeGenerationOptions
        {
            KeepEnglishInCode = cbxKeepEnglish.IsChecked ?? false,
            KeepNumberInCode = cbxKeepNumber.IsChecked ?? false,
            KeepPunctuationInCode = cbxKeepPunctuation.IsChecked ?? false,
            PrefixEnglishWithUnderscore = cbxPrefixEnglish.IsChecked ?? false,
            TranslateNumbersToChinese = cbxChsNumber.IsChecked ?? false,
            ConvertFullWidth = cbxFullWidth.IsChecked ?? false
        };

        Close(true);
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
