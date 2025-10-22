using Avalonia.Controls;
using Avalonia.Interactivity;
using Studyzy.IMEWLConverter.Generaters;

namespace ImeWlConverterMac.Views;

public partial class WordRankGenerateWindow : Window
{
    public IWordRankGenerater WordRankGenerater { get; private set; }

    public WordRankGenerateWindow(IWordRankGenerater currentGenerater)
    {
        InitializeComponent();
        WordRankGenerater = currentGenerater;
        LoadConfig();
    }

    private void LoadConfig()
    {
        if (WordRankGenerater is DefaultWordRankGenerater defaultGen)
        {
            rbtnDefault.IsChecked = true;
            numRank.Value = defaultGen.Rank;
        }
        else if (WordRankGenerater is GoogleWordRankGenerater)
        {
            rbtnGoogle.IsChecked = true;
        }
        else if (WordRankGenerater is BaiduWordRankGenerater)
        {
            rbtnBaidu.IsChecked = true;
        }
        else if (WordRankGenerater is CalcWordRankGenerater)
        {
            rbtnCalc.IsChecked = true;
        }
        
        cbxForceUseNewRank.IsChecked = WordRankGenerater.ForceUse;
    }

    private void BtnOK_Click(object? sender, RoutedEventArgs e)
    {
        if (rbtnDefault.IsChecked == true)
        {
            WordRankGenerater = new DefaultWordRankGenerater { Rank = (int)numRank.Value };
        }
        else if (rbtnGoogle.IsChecked == true)
        {
            WordRankGenerater = new GoogleWordRankGenerater();
        }
        else if (rbtnBaidu.IsChecked == true)
        {
            WordRankGenerater = new BaiduWordRankGenerater();
        }
        else if (rbtnCalc.IsChecked == true)
        {
            WordRankGenerater = new CalcWordRankGenerater();
        }
        
        WordRankGenerater.ForceUse = cbxForceUseNewRank.IsChecked ?? false;
        
        Close(true);
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
