using Avalonia.Controls;
using Avalonia.Interactivity;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;

namespace ImeWlConverterMac.Views;

public partial class WordRankGenerateWindow : Window
{
    public IWordRankGenerater WordRankGenerater { get; private set; }
    private static LlmConfig llmConfig = new LlmConfig();

    public WordRankGenerateWindow()
    {
        InitializeComponent();
        WordRankGenerater = new DefaultWordRankGenerater();
        LoadConfig();
    }

    public WordRankGenerateWindow(IWordRankGenerater currentGenerater)
    {
        InitializeComponent();
        WordRankGenerater = currentGenerater;
        LoadConfig();
    }

    private void LoadConfig()
    {
        txtLlmEndpoint.Text = llmConfig.ApiEndpoint;
        txtLlmKey.Text = llmConfig.ApiKey;
        txtLlmModel.Text = llmConfig.Model;

        if (WordRankGenerater is DefaultWordRankGenerater defaultGen)
        {
            rbtnDefault.IsChecked = true;
            numRank.Value = (int)defaultGen.Rank;
        }
        else if (WordRankGenerater is LlmWordRankGenerater)
        {
            rbtnLlm.IsChecked = true;
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
            WordRankGenerater = new DefaultWordRankGenerater { Rank = (int)(numRank.Value ?? 0) };
        }
        else if (rbtnLlm.IsChecked == true)
        {
            llmConfig.ApiEndpoint = txtLlmEndpoint.Text ?? "";
            llmConfig.ApiKey = txtLlmKey.Text ?? "";
            llmConfig.Model = txtLlmModel.Text ?? "";
            WordRankGenerater = new LlmWordRankGenerater(llmConfig);
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
