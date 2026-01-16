using Avalonia.Controls;
using Avalonia.Interactivity;
using Studyzy.IMEWLConverter.Entities;

namespace ImeWlConverterMac.Views;

public partial class FilterConfigWindow : Window
{
    public FilterConfig FilterConfig { get; private set; }

    public FilterConfigWindow()
    {
        InitializeComponent();
        FilterConfig = new FilterConfig();
        LoadConfig();
    }

    public FilterConfigWindow(FilterConfig filterConfig)
    {
        InitializeComponent();
        FilterConfig = filterConfig;
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

        cbxKeepEnglish.IsChecked = FilterConfig.KeepEnglish;
        cbxKeepNumber.IsChecked = FilterConfig.KeepNumber;
        cbxKeepPunctuation.IsChecked = FilterConfig.KeepPunctuation;
        cbxKeepSpace.IsChecked = FilterConfig.KeepSpace;
        cbxKeepEnglish_.IsChecked = FilterConfig.KeepEnglish_;
        cbxKeepNumber_.IsChecked = FilterConfig.KeepNumber_;
        cbxKeepPunctuation_.IsChecked = FilterConfig.KeepPunctuation_;
        cbxKeepSpace_.IsChecked = FilterConfig.KeepSpace_;

        cbxFullWidth.IsChecked = FilterConfig.FullWidth;
        cbxChsNumber.IsChecked = FilterConfig.ChsNumber;
        cbxPrefixEnglish.IsChecked = FilterConfig.PrefixEnglish;
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

        FilterConfig.KeepEnglish = cbxKeepEnglish.IsChecked ?? false;
        FilterConfig.KeepNumber = cbxKeepNumber.IsChecked ?? false;
        FilterConfig.KeepPunctuation = cbxKeepPunctuation.IsChecked ?? false;
        FilterConfig.KeepSpace = cbxKeepSpace.IsChecked ?? false;
        FilterConfig.KeepEnglish_ = cbxKeepEnglish_.IsChecked ?? false;
        FilterConfig.KeepNumber_ = cbxKeepNumber_.IsChecked ?? false;
        FilterConfig.KeepPunctuation_ = cbxKeepPunctuation_.IsChecked ?? false;
        FilterConfig.KeepSpace_ = cbxKeepSpace_.IsChecked ?? false;

        FilterConfig.FullWidth = cbxFullWidth.IsChecked ?? false;
        FilterConfig.ChsNumber = cbxChsNumber.IsChecked ?? false;
        FilterConfig.PrefixEnglish = cbxPrefixEnglish.IsChecked ?? false;

        Close(true);
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
