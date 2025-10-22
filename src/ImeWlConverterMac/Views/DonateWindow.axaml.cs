using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ImeWlConverterMac.Views;

public partial class DonateWindow : Window
{
    public DonateWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
