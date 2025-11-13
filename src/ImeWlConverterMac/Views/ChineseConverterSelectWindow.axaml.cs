using Avalonia.Controls;
using Avalonia.Interactivity;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Language;

namespace ImeWlConverterMac.Views;

public partial class ChineseConverterSelectWindow : Window
{
    public ChineseTranslate SelectedTranslate { get; private set; }
    public IChineseConverter? SelectedConverter { get; private set; }

    public ChineseConverterSelectWindow()
    {
        InitializeComponent();
        SelectedTranslate = ChineseTranslate.NotTrans;
        LoadConfig();
    }

    public ChineseConverterSelectWindow(ChineseTranslate currentTranslate, IChineseConverter? currentConverter)
    {
        InitializeComponent();
        SelectedTranslate = currentTranslate;
        SelectedConverter = currentConverter;
        LoadConfig();
    }

    private void LoadConfig()
    {
        // 设置转换类型
        switch (SelectedTranslate)
        {
            case ChineseTranslate.NotTrans:
                rbtnNotTrans.IsChecked = true;
                break;
            case ChineseTranslate.Trans2Chs:
                rbtnTransToChs.IsChecked = true;
                break;
            case ChineseTranslate.Trans2Cht:
                rbtnTransToCht.IsChecked = true;
                break;
        }

        // 设置转换引擎 - macOS 只支持系统内核
        rbtnKernel.IsChecked = true;
    }

    private void BtnOK_Click(object? sender, RoutedEventArgs e)
    {
        // 获取转换类型
        if (rbtnNotTrans.IsChecked == true)
        {
            SelectedTranslate = ChineseTranslate.NotTrans;
        }
        else if (rbtnTransToChs.IsChecked == true)
        {
            SelectedTranslate = ChineseTranslate.Trans2Chs;
        }
        else if (rbtnTransToCht.IsChecked == true)
        {
            SelectedTranslate = ChineseTranslate.Trans2Cht;
        }

        // 获取转换引擎 - macOS 只支持系统内核
        SelectedConverter = new SystemKernel();

        Close(true);
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
